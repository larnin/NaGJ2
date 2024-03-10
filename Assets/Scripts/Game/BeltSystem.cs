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


        int beltCount = buildingBelts.belts.Count;
        List<int> othersBelts = new List<int>();
        for(int i = 0; i < beltCount; i++)
        {
            othersBelts.Clear();

            var b = buildingBelts.belts[i];

            for(int j = 0; j < beltCount; j++)
            {
                var b2 = buildingBelts.belts[j];

                var offset = b2.pos - b.pos;

                if (MathF.Abs(offset.x) + MathF.Abs(offset.z) == 1)
                    othersBelts.Add(j);
            }

            var newNode = new BeltNode();
            newNode.pos = b.pos;
            newNode.rotation = b.rotation;

            var nextPos = newNode.pos + RotationEx.ToVector3Int(newNode.rotation);

            int nextNodeIndex = -1;

            for(int j = 0; j < othersBelts.Count; j++)
            {
                var b2 = buildingBelts.belts[othersBelts[j]];

                if (b2.pos != nextPos)
                    continue;

                if (b2.rotation == RotationEx.Add(newNode.rotation, Rotation.rot_180))
                    continue;

                nextNodeIndex = j;
                break;
            }

            if (nextNodeIndex >= 0)
                newNode.nextIndex = othersBelts[nextNodeIndex];

            for(int j = 0; j < othersBelts.Count; j++)
            {
                if (j == nextNodeIndex)
                    continue;

                var b2 = buildingBelts.belts[othersBelts[j]];

                nextPos = b2.pos + RotationEx.ToVector3Int(b2.rotation);
                if (nextPos == newNode.pos)
                    newNode.previousIndexs.Add(othersBelts[j]);
            }

            newBelts.Add(newNode);
        }

        //add ports
        for (int i = 0; i < m_ports.Count; i++)
        {
            int nextIndex = newBelts.Count;

            var port = m_ports[i];

            var pos = port.pos;
            var nextPos = pos + RotationEx.ToVector3Int(port.rotation);

            int beltIndex = -1;
            for (int j = 0; j < newBelts.Count; j++)
            {
                if (newBelts[j].pos == nextPos)
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

            if (beltIndex >= 0)
            {
                var belt = newBelts[beltIndex];
                if (port.direction == BuildingPortDirection.output)
                {
                    if (belt.rotation != RotationEx.Add(port.rotation, Rotation.rot_180))
                    {
                        newNode.nextIndex = beltIndex;
                        belt.previousIndexs.Add(nextIndex);
                    }
                }
                else if (port.direction == BuildingPortDirection.inout)
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
                else if (port.direction == BuildingPortDirection.input)
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

            DebugDraw.CentredBox(pos, Vector3.one * 0.1f, Color.green);

            var pos2 = pos;
            pos2.y += 0.2f;
            var pos3 = pos2 + RotationEx.ToVector3(b.rotation) * 0.5f;
            DebugDraw.Line(pos2, pos3, Color.green);

            if(b.nextIndex >= 0)
            {
                var next = m_belts[b.nextIndex];

                var nextPos = new Vector3(next.pos.x * size.x, next.pos.y * size.y, next.pos.z * size.z);

                DebugDraw.Line(pos, nextPos, Color.green);
            }

            if(b.portIndex >= 0)
            {
                var port = m_ports[b.portIndex];

                pos2 = pos;
                pos2.y += 0.3f;
                pos3 = pos2 + RotationEx.ToVector3(port.rotation) * 0.5f;
                DebugDraw.Line(pos2, pos3, Color.blue);

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
