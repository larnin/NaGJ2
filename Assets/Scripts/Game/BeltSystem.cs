using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BeltSystem : MonoBehaviour
{
    [SerializeField] float m_resourceOffset;

    class Container
    {
        public BuildingContainer container;

        public List<ResourceType> resources = new List<ResourceType>();

        public int GetFreeSpace()
        {
            if (container.count < 0)
                return -1;

            int free = container.count - resources.Count;
            if (free < 0)
                return 0;
            return free;
        }
    }

    class Resource
    {
        public ResourceType resource;
        public GameObject instance;

        public Vector3 pos;
        public int beltIndex;
    }

    class BeltNode
    {
        public Vector3Int pos = Vector3Int.zero;
        public Rotation rotation = Rotation.rot_0;
        public int nextIndex = -1;
        public List<int> previousIndexs = new List<int>();
        public int portIndex = -1;

        public List<Resource> resources = new List<Resource>();
    }

    List<Container> m_containers = new List<Container>();
    List<BuildingOnePortData> m_ports = new List<BuildingOnePortData>();
    List<BeltNode> m_belts = new List<BeltNode>();
    List<Resource> m_resources = new List<Resource>();

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

        //change nodes
        m_belts = newBelts;
        m_containers = newContainers;

        ReplaceResources();
    }

    void ReplaceResources()
    {
        for(int i = 0; i < m_resources.Count; i++)
        {
            var r = m_resources[i];
            r.beltIndex = -1;

            var pos = new Vector3Int(Mathf.RoundToInt(r.pos.x), Mathf.RoundToInt(r.pos.y), Mathf.RoundToInt(r.pos.z));

            int beltIndex = -1;
            for(int j = 0; j < m_belts.Count; j++)
            {
                if(m_belts[j].pos == pos)
                {
                    beltIndex = j;
                    break;
                }
            }

            if(beltIndex >= 0)
            {
                r.beltIndex = beltIndex;
                m_belts[beltIndex].resources.Add(r);
            }
            else
            {
                DestroyResource(r);
                m_resources.RemoveAt(i);
                i--;
            }
        }
    }

    void AddResouce(AddResourceEvent e)
    {
        e.addedCount = 0;

        foreach (var c in m_containers)
        {
            if (c.container.id == e.buildingID && c.container.index == e.containerIndex)
            {
                if (!c.container.filter.IsValid(e.resource))
                    break;

                int freeSpace = c.GetFreeSpace();

                if (freeSpace < 0 || freeSpace >= e.count)
                    e.addedCount = e.count;

                for (int i = 0; i < e.addedCount; i++)
                    c.resources.Add(e.resource);

                break;
            }
        }
    }

    void RemoveResource(RemoveResourceEvent e)
    {
        e.removedCount = 0;
        if (e.count == 0)
            return;

        foreach (var c in m_containers)
        {
            if (c.container.id == e.buildingID && c.container.index == e.containerIndex)
            {
                for(int i = 0; i < c.resources.Count; i++)
                {
                    if(c.resources[i] == e.resource)
                    {
                        e.removedCount++;
                        c.resources.RemoveAt(i);
                        i--;
                    }

                    if (e.count > 0 && e.removedCount >= e.count)
                        break;
                }

                break;
            }
        }
    }

    void GetCapacity(GetContainerCapacityEvent e)
    {
        foreach(var c in m_containers)
        {
            if(c.container.id == e.buildingID && c.container.index == e.containerIndex)
            {
                e.capacity = c.container.count;
                e.freeSpace = c.GetFreeSpace();

                break;
            }
        }
    }

    void GetItems(GetContainerItems e)
    {
        foreach (var c in m_containers)
        {
            if (c.container.id == e.buildingID && c.container.index == e.containerIndex)
            {
                e.resources = c.resources.ToList();

                break;
            }
        }
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

    bool CanCreateResource(int beltIndex)
    {
        var node = m_belts[beltIndex];

        float space = Global.instance.allResources.beltResourceSpacing;

        Vector3 pos = new Vector3(node.pos.x, node.pos.y, node.pos.z);

        foreach(var r in node.resources)
        {
            float dist = (pos - r.pos).magnitude;
            if (dist <= space)
                return false;
        }

        return true;
    }

    void CreateResource(ResourceType type, int beltIndex)
    {
        var node = m_belts[beltIndex];

        Resource r = new Resource();
        r.resource = type;
        r.beltIndex = beltIndex;
        r.pos = new Vector3(node.pos.x, node.pos.y, node.pos.z);

        node.resources.Add(r);

        var oneResource = Global.instance.allResources.Get(type);
        r.instance = Instantiate(oneResource.prefab);
        r.instance.transform.parent = transform;

        m_resources.Add(r);
    }

    void DestroyResource(Resource r)
    {
        if (r.instance != null)
            Destroy(r.instance);

        if (r.beltIndex >= 0)
            m_belts[r.beltIndex].resources.Remove(r);
    }

    private void Update()
    {
        ProcessBelts();
        UpdateResourcesInstances();

        DebugDrawBelts();
    }

    class BeltMove
    {
        public Resource resource;
        public int newBeltIndex;
    }

    void ProcessBelts()
    {
        List<BeltMove> movingResources = new List<BeltMove>();

        for(int i = 0; i < m_belts.Count; i++)
        {
            var belt = m_belts[i];

            for(int j = 0; j < belt.resources.Count; j++)
            {
                var r = belt.resources[j];

                var beltPos = new Vector3(belt.pos.x, belt.pos.y, belt.pos.z);

                var dir = r.pos - beltPos;
                float dist = dir.magnitude;
                var rot = RotationEx.FromVector(dir);

                var target = beltPos;

                if(rot == belt.rotation || dist < 0.1f)
                {
                    if(belt.portIndex >= 0)
                    {
                        var port = m_ports[belt.portIndex];
                        if(port.direction == BuildingPortDirection.inout || port.direction == BuildingPortDirection.input)
                        {
                            var container = m_containers[port.containerIndex];

                            int nbFree = container.GetFreeSpace();

                            if (nbFree == 0)
                                continue;
                            if (!container.container.filter.IsValid(r.resource))
                                continue;

                            r.beltIndex = -1;
                            DestroyResource(r);

                            container.resources.Add(r.resource);
                            belt.resources.RemoveAt(j);
                            m_resources.Remove(r);
                            j--;

                            continue;
                        }
                    }

                    target = beltPos + RotationEx.ToVector3(belt.rotation);
                }

                dir = target - r.pos;

                var newPos = r.pos + dir.normalized * Time.deltaTime * Global.instance.allResources.beltSpeed;

                BeltNode nextBelt = null;
                if (belt.nextIndex >= 0)
                    nextBelt = m_belts[belt.nextIndex];

                List<Resource> otherResources = new List<Resource>();
                foreach(var otherR in belt.resources)
                {
                    if (otherR == r)
                        continue;
                    otherResources.Add(otherR);
                }
                if (nextBelt != null)
                    otherResources.AddRange(nextBelt.resources);

                var moving = MoveResource(r, otherResources, newPos);

                if (!moving)
                    continue;

                Vector3Int newPosI = new Vector3Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y), Mathf.RoundToInt(newPos.z));
                if(newPosI != belt.pos)
                {
                    if (nextBelt == null)
                        continue;

                    var beltMove = new BeltMove();
                    beltMove.resource = r;
                    beltMove.newBeltIndex = belt.nextIndex;

                    movingResources.Add(beltMove);
                }

                r.pos = newPos;
            }

            if(belt.portIndex >= 0)
            {
                var port = m_ports[belt.portIndex];
                if (port.direction == BuildingPortDirection.input)
                    continue;
                if (port.direction == BuildingPortDirection.inout && belt.nextIndex < 0)
                    continue;

                var container = m_containers[port.containerIndex];
                if (container.resources.Count == 0)
                    continue;

                if (!CanCreateResource(i))
                    continue;

                var resource = container.resources[0];
                container.resources.RemoveAt(0);

                CreateResource(resource, i);
            }
        }

        foreach(var b in movingResources)
        {
            if(b.resource.beltIndex >= 0)
                m_belts[b.resource.beltIndex].resources.Remove(b.resource);

            m_belts[b.newBeltIndex].resources.Add(b.resource);
            b.resource.beltIndex = b.newBeltIndex;
        }
    }

    bool MoveResource(Resource current, List<Resource> blocking, Vector3 newPos)
    {
        float space = Global.instance.allResources.beltResourceSpacing;

        var beltPos = m_belts[current.beltIndex].pos;
        var target = GetTarget(current);
        var targetF = new Vector3(target.x, target.y, target.z);
        bool isSelfBlockTarget = target == beltPos;

        foreach (var other in blocking)
        {
            var otherTarget = GetTarget(other);
            var otherTargetF = new Vector3(otherTarget.x, otherTarget.y, otherTarget.z);

            if (!isSelfBlockTarget && otherTarget == beltPos)
                continue;

            if(otherTarget == target)
            {
                if ((other.pos - otherTargetF).sqrMagnitude > (current.pos - targetF).sqrMagnitude)
                    continue;
            }

            float d = (other.pos - newPos).sqrMagnitude;
            float oldD = (other.pos - current.pos).sqrMagnitude;

            if (d < space * space && oldD > d)
                return false;
        }

        return true;
    }

    Vector3Int GetTarget(Resource r)
    {
        var belt = m_belts[r.beltIndex];

        Vector3 beltPos = new Vector3(belt.pos.x, belt.pos.y, belt.pos.z);

        var dir = r.pos - beltPos;
        float sqrDist = dir.magnitude;

        var target = belt.pos;

        var rot = RotationEx.FromVector(dir);
        if (rot == belt.rotation || sqrDist < 0.1f * 0.1f)
            target = belt.pos + RotationEx.ToVector3Int(belt.rotation);

        return target;
    }

    void UpdateResourcesInstances()
    {
        var size = Global.instance.allBlocks.blockSize;

        foreach(var r in m_resources)
        {
            if (r.instance == null)
                continue;

            var pos = new Vector3(r.pos.x * size.x, r.pos.y * size.y + m_resourceOffset, r.pos.z * size.z);

            r.instance.transform.localPosition = pos;
        }
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
