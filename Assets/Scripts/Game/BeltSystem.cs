using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BeltSystem : MonoBehaviour
{
    class Container
    {
        public BuildingContainer container;

        public List<ResourceType> resources = new List<ResourceType>();
    }

    class Resource
    {
        public ResourceType resource;
        public GameObject instance;

        //add belt advancement info
    }

    class BeltNode
    {
        public Vector3Int pos = Vector3Int.zero;
        public Rotation rotation = Rotation.rot_0;
        public int nextIndex = -1;
        public List<int> previousIndexs = new List<int>();
        public int portIndex = -1;
    }

    List<Container> m_containers = new List<Container>();
    List<BuildingOnePortData> m_ports = new List<BuildingOnePortData>();
    List<BeltNode> m_belts = new List<BeltNode>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<LoadEndedEvent>.Subscriber(OnLoad));
        m_subscriberList.Add(new Event<BuildingUpdatedEvent>.Subscriber(OnUpdate));
        m_subscriberList.Add(new Event<AddResourceEvent>.Subscriber(AddResouce));
        m_subscriberList.Add(new Event<RemoveResourceEvent>.Subscriber(RemoveResource));
        m_subscriberList.Add(new Event<GetContainerCapacityEvent>.Subscriber(GetCapacity));
        m_subscriberList.Add(new Event<GetContainerItems>.Subscriber(GetItems));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnLoad(LoadEndedEvent e)
    {
        RegenerateWorld();
    }

    void OnUpdate(BuildingUpdatedEvent e)
    {
        //update all the belts again, like on load - todo optimize this function
        RegenerateWorld();
    }

    void RegenerateWorld()
    {
        GetBuildingBeltsEvent buildingBelts = new GetBuildingBeltsEvent();
        Event<GetBuildingBeltsEvent>.Broadcast(buildingBelts);

        List<Container> newContainers = new List<Container>();
        m_ports = buildingBelts.ports.ToList();
        List<BeltNode> newBelts = new List<BeltNode>();

        //update containers
        foreach(var c in buildingBelts.containers)
        {
            Container container = new Container();
            container.container = c;

            var oldContainer = GetContainer(c.id, c.index);
            if(oldContainer != null)
            {
                foreach(var item in oldContainer.resources)
                {
                    if (container.container.filter.IsValid(item))
                        container.resources.Add(item);
                }
            }

            newContainers.Add(container);
        }

        //first, get the belt that the current one face
        int[] nextBeltIndexs = new int[buildingBelts.belts.Count];

        for(int i = 0; i < buildingBelts.belts.Count; i++)
        {
            var b = buildingBelts.belts[i];

            var nextPos = b.pos + RotationEx.ToVector3Int(b.rotation);
            int nextIndex = -1;
            for(int j = 0; j < buildingBelts.belts.Count; j++)
            {
                if (i == j)
                    continue;

                var nextBelt = buildingBelts.belts[j];

                if(nextBelt.pos == nextPos)
                {
                    if (nextBelt.rotation == RotationEx.Add(b.rotation, Rotation.rot_180))
                        break;

                    nextIndex = j;
                    break;
                }
            }

            nextBeltIndexs[i] = nextIndex;
        }

        //next, get the tail belts
        bool[] visitedIndex = new bool[buildingBelts.belts.Count];
        List<int> firstIndexs = new List<int>();
        for(int i = 0; i < buildingBelts.belts.Count; i++)
        {
            int currentIndex = i;
            if (visitedIndex[currentIndex])
                continue;

            firstIndexs.Add(i);

            while(!visitedIndex[currentIndex])
            {
                visitedIndex[currentIndex] = true;
                currentIndex = nextBeltIndexs[currentIndex];
                if (currentIndex < 0)
                    break;
                firstIndexs.Remove(currentIndex);
            }
        }

        int[] indexInBeltNode = new int[buildingBelts.belts.Count];
        for (int i = 0; i < buildingBelts.belts.Count; i++)
            indexInBeltNode[i] = -1;

        //with the tails belts, create the belt tree
        for(int i = 0; i < firstIndexs.Count; i++)
        {
            int lastIndex = -1;
            int currentIndex = firstIndexs[i];
            
            while(true)
            {
                if(indexInBeltNode[currentIndex] != -1)
                {
                    if(lastIndex != -1)
                    {
                        var node = newBelts[indexInBeltNode[currentIndex]];
                        if (!node.previousIndexs.Contains(lastIndex))
                            node.previousIndexs.Add(lastIndex);
                    }
                    break;
                }

                var newNode = new BeltNode();
                var belt = buildingBelts.belts[currentIndex];
                newNode.pos = belt.pos;
                newNode.rotation = belt.rotation;
                newNode.previousIndexs.Add(lastIndex);

                int nextIndex = nextBeltIndexs[currentIndex];
                newNode.nextIndex = nextIndex;

                if (nextIndex < 0)
                    break;

                lastIndex = currentIndex;
                currentIndex = nextIndex;
            }
        }

        //add ports
        for(int i = 0; i < m_ports.Count; i++)
        {
            int nextIndex = newBelts.Count;

            var port = m_ports[i];

            var pos = port.pos;
            var nextPos = pos + RotationEx.ToVector3Int(port.rotation);

            int beltIndex = -1;
            for(int j = 0; j < newBelts.Count; j++)
            {
                if(newBelts[j].pos == nextPos)
                {
                    beltIndex = j;
                    break;
                }
            }

            var newNode = new BeltNode();
            newNode.pos = pos;
            newNode.portIndex = i;

            if (port.direction == BuildingPortDirection.output || port.direction == BuildingPortDirection.inout)
                newNode.rotation = port.rotation;
            else if (port.direction == BuildingPortDirection.input)
                newNode.rotation = RotationEx.Add(port.rotation, Rotation.rot_180);

            if(beltIndex > 0)
            {
                 var belt = newBelts[beltIndex];
                if(port.direction == BuildingPortDirection.output)
                {
                    if(belt.rotation != RotationEx.Add(port.rotation, Rotation.rot_180))
                    {
                        newNode.nextIndex = beltIndex;
                        belt.previousIndexs.Add(nextIndex);
                    }
                }
                else if(port.direction == BuildingPortDirection.inout)
                {
                    if (belt.rotation == RotationEx.Add(port.rotation, Rotation.rot_180))
                    {
                        belt.nextIndex = nextIndex;
                        newNode.previousIndexs.Add(beltIndex);
                    }
                    else
                    {
                        newNode.nextIndex = beltIndex;
                        belt.previousIndexs.Add(nextIndex);
                    }
                }
                else if(port.direction == BuildingPortDirection.input)
                {
                    if (belt.rotation == RotationEx.Add(port.rotation, Rotation.rot_180))
                    {
                        belt.nextIndex = nextIndex;
                        newNode.previousIndexs.Add(beltIndex);
                    }
                }
            }

            newBelts.Add(newNode);
        }

        //todo move resources to new belts

        //change nodes
        m_belts = newBelts;
    }

    void AddResouce(AddResourceEvent e)
    {

    }

    void RemoveResource(RemoveResourceEvent e)
    {

    }

    void GetCapacity(GetContainerCapacityEvent e)
    {

    }

    void GetItems(GetContainerItems e)
    {

    }

    Container GetContainer(int buildingID, int containerIndex)
    {
        foreach(var c in m_containers)
        {
            if (c.container.id == buildingID && c.container.index == containerIndex)
                return c;
        }

        return null;
    }

    private void Update()
    {
        DebugDrawBelts();
    }

    void DebugDrawBelts()
    {
        Vector3 size = Global.instance.allBlocks.blockSize;

        for(int i = 0; i < m_belts.Count; i++)
        {
            var b = m_belts[i];

            var pos = new Vector3(b.pos.x * size.x, b.pos.y * size.y, b.pos.z * size.z);

            DebugDraw.Box(pos, Vector3.one * 0.1f, Color.green);

            if(b.nextIndex >= 0)
            {
                var next = m_belts[b.nextIndex];

                var nextPos = new Vector3(next.pos.x * size.x, next.pos.y * size.y, next.pos.z * size.z);

                DebugDraw.Line(pos, nextPos, Color.green);
            }

            if(b.portIndex >= 0)
            {
                var port = m_ports[b.portIndex];

                var upPos = pos;
                upPos.y += 1;

                Color c = Color.magenta;
                if (port.direction == BuildingPortDirection.input)
                    c = Color.cyan;
                else if (port.direction == BuildingPortDirection.output)
                    c = Color.red;
                else if (port.direction == BuildingPortDirection.inout)
                    c = Color.yellow;

                DebugDraw.Line(pos, upPos, c);
            }
        }
    }
}
