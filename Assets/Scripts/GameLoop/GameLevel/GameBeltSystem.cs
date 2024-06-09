using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Resource
{
    public int ID;
    public ResourceType resource;

    public Vector3 pos;
    public int beltIndex;
    public Rotation startDirection;

    public void Save(JsonObject obj)
    {
        obj.AddElement("Type", resource.ToString());
        obj.AddElement("ID", ID);
        obj.AddElement("Pos", Json.FromVector3(pos));
        obj.AddElement("Belt", beltIndex);
        obj.AddElement("StartDir", startDirection.ToString());
    }

    public void Load(JsonObject obj)
    {
        string str = obj.GetElement("Type")?.String();
        if(str != null)
            Enum.TryParse(str, out resource);

        ID = obj.GetElement("ID")?.Int() ?? 0;

        pos = Json.ToVector3(obj.GetElement("Pos")?.JsonArray(), Vector3.zero);

        beltIndex = obj.GetElement("Belt")?.Int() ?? 0;

        str = obj.GetElement("StartDir")?.String();
        if (str != null)
            Enum.TryParse(str, out startDirection);
    }
}

public class GameBeltSystem
{
    class ResouceContainer
    {
        public ResourceType resouce;
        public float value;
    }

    class Container
    {
        public int buildingID;
        public int containerIndex;
        public BuildingContainerData buildingContainer;

        public List<ResouceContainer> resources = new List<ResouceContainer>();

        public float GetItemNb()
        {
            float nb = 0;
            foreach (var resource in resources)
                nb += resource.value;

            return nb;
        }

        public float GetFreeSpace()
        {
            if (buildingContainer.count < 0)
                return 100000000;

            float free = buildingContainer.count - GetItemNb();
            if (free < 0)
                return 0;
            return free;
        }

        public void AddResource(ResourceType type, float nb)
        {
            if(resources.Count > 0)
            {
                if (ResourceDataEx.IsLiquid(type))
                {
                    foreach(var r in resources)
                    {
                        if (r.resouce == type)
                        {
                            r.value += nb;
                            return;
                        }
                    }
                }
                else
                {
                    var r = resources[resources.Count - 1];
                    if (r.resouce == type)
                    {
                        r.value += nb;
                        return;
                    }
                }
            }

            var newR = new ResouceContainer();
            newR.resouce = type;
            newR.value = nb;
            resources.Add(newR);
        }

        public float GetFirstResource(bool liquid, out ResourceType resource)
        {
            resource = default(ResourceType);
            bool found = false;

            float nb = 0;

            for(int i = 0; i < resources.Count; i++)
            {
                var r = resources[i];

                if (ResourceDataEx.IsLiquid(r.resouce) != liquid)
                    continue;

                if (!found)
                {
                    found = true;
                    resource = r.resouce;
                }
                else if (resource != r.resouce)
                    continue;

                nb += r.value;
            }

            return nb;
        }

        public float PopResource(ResourceType resource, float nb)
        {
            float popped = 0;

            for(int i = 0; i < resources.Count; i++)
            {
                var r = resources[i];

                if (r.resouce != resource)
                    continue;

                if(r.value <= nb - popped)
                {
                    popped += r.value;
                    resources.RemoveAt(i);
                    i--;

                    if (popped == nb)
                        break;
                }
                else
                {
                    r.value -= nb - popped;
                    popped = nb;
                    break;
                }
            }

            return popped;
        }

        public void Save(JsonObject obj)
        {
            obj.AddElement("ID", buildingID);
            obj.AddElement("Index", containerIndex);

            var resourcesArray = new JsonArray();
            obj.AddElement("Resources", resourcesArray);

            foreach(var r in resources)
            {
                var resource = new JsonObject();
                resourcesArray.Add(resource);

                resource.AddElement("T", r.resouce.ToString());
                resource.AddElement("C", r.value);
            }
        }

        public void Load(JsonObject obj)
        {
            buildingID = obj.GetElement("ID")?.Int() ?? 0;
            containerIndex = obj.GetElement("Index")?.Int() ?? 0;

            var resourcesElt = obj.GetElement("Resources");
            if(resourcesElt != null && resourcesElt.IsJsonArray())
            {
                var resourcesArray = resourcesElt.JsonArray();

                foreach(var r in resourcesArray)
                {
                    var resource = new ResouceContainer();
                    
                    string str = obj.GetElement("T")?.String();
                    if (str == null)
                        continue;
                    if (!Enum.TryParse(str, out resource.resouce))
                        continue;
                    resource.value = obj.GetElement("C")?.Float() ?? 0.0f;

                    resources.Add(resource);
                }
            }
        }
    }

    class BeltNode
    {
        public Vector3Int pos = Vector3Int.zero;
        public Rotation rotation = Rotation.rot_0;
        public BeltDirection direction = BeltDirection.Horizontal;
        public int nextIndex = -1;
        public List<int> previousIndexs = new List<int>();
        public int containerIndex = -1;
        public int portIndex = -1;

        public List<Resource> resources = new List<Resource>();
    }

    GameLevel m_level;

    int m_nextID = 1;

    List<Container> m_containers = new List<Container>();
    List<BeltNode> m_beltNodes = new List<BeltNode>();
    Dictionary<int, Resource> m_resources = new Dictionary<int, Resource>();

    List<Resource> m_tempList = new List<Resource>();

    public GameBeltSystem(GameLevel level)
    {
        m_level = level;
    }

    public void AfterLoad()
    {
        UpdateBelts();
    }

    public void OnBuildingUpdate(int buildingID, ElementUpdateType type)
    {
        if (type == ElementUpdateType.removed)
            UpdateBelts();
        else
        {
            var building = m_level.buildingList.GetBuilding(buildingID);
            if(building != null)
            {
                var infos = building.GetInfos();
                if (infos.buildingType == BuildingType.Belt || BuildingDataEx.GetContainer(infos.buildingType) != null)
                    UpdateBelts();
            }
        }
    }

    public float ContainerAddResource(ResourceType type, float count, int buildingID, int containerIndex)
    {
        Container container = GetContainer(buildingID, containerIndex);
        if (container == null)
            return 0;

        float freeSpace = container.GetFreeSpace();
        float realCount = count;
        if(!ResourceDataEx.IsLiquid(type))
        {
            freeSpace = Mathf.Floor(freeSpace);
            realCount = Mathf.Floor(realCount);
        }

        if (freeSpace < realCount)
            realCount = freeSpace;
        if (realCount <= 0)
            return 0;

        container.AddResource(type, realCount);

        return realCount;
    }

    public float ContainerGetResourceNb(ResourceType type, int buildingID, int containerIndex)
    {
        Container container = GetContainer(buildingID, containerIndex);
        if (container == null)
            return 0;

        float nb = 0;
        foreach(var r in container.resources)
        {
            if (r.resouce == type)
                nb += r.value;
        }

        return nb;
    }

    public float ContainerGetFirstResource(bool liquid, out ResourceType type, int buildingID, int containerIndex)
    {
        type = default(ResourceType);
        Container container = GetContainer(buildingID, containerIndex);
        if (container == null)
            return 0;

        return container.GetFirstResource(liquid, out type);
    }

    public List<ResourceType> ContainerGetAllResources(int buildingID, int containerIndex)
    {
        List<ResourceType> resources = new List<ResourceType>();

        Container container = GetContainer(buildingID, containerIndex);
        if (container == null)
            return resources;

        foreach(var r in container.resources)
        {
            if (!resources.Contains(r.resouce))
                resources.Add(r.resouce);
        }

        return resources;
    }

    public float ContainerRemoveResource(ResourceType type, float count, int buildingID, int containerIndex)
    {
        Container container = GetContainer(buildingID, containerIndex);
        if (container == null)
            return 0;

        if (!ResourceDataEx.IsLiquid(type))
            count = Mathf.Floor(count);

        if (count <= 0)
            return 0;

        return container.PopResource(type, count);
    }

    public Resource GetResource(int id)
    {
        Resource r = null;
        if (!m_resources.TryGetValue(id, out r))
            return null;
        return r;
    }

    public int GetResourceNb()
    {
        return m_resources.Count();
    }

    public List<int> GetAllResourcesID()
    {
        List<int> resources = new List<int>();

        foreach (var r in m_resources)
            resources.Add(r.Key);

        return resources;
    }

    void UpdateBelts()
    {
        var beltsId = m_level.buildingList.GetAllBeltID();
        var containersBuildingsId = m_level.buildingList.GetAllBuildingContainers();

        List<Container> newContainers = new List<Container>();
        List<BeltNode> newBelts = new List<BeltNode>();

        //update containers
        foreach (var id in containersBuildingsId)
        {
            var building = m_level.buildingList.GetBuilding(id);
            if (building == null)
                continue;

            var infos = building.GetInfos();
            var containers = BuildingDataEx.GetContainer(infos.buildingType);

            for(int i = 0; i < containers.Count; i++)
            {
                Container c = new Container();
                c.buildingContainer = new BuildingContainerData();
                c.buildingContainer.filter = containers[i].filter;
                c.buildingContainer.count = containers[i].count;
                c.buildingContainer.ports = new List<BuildingPortData>();
                foreach(var p in containers[i].ports)
                {
                    BuildingPortData port = new BuildingPortData();
                    port.direction = p.direction;
                    port.rotation = RotationEx.Add(p.rotation, infos.rotation);
                    port.pos = infos.pos + RotationEx.Rotate(p.pos, infos.rotation);
                    c.buildingContainer.ports.Add(port);
                }

                c.buildingID = id;
                c.containerIndex = i;

                var oldContainer = GetContainer(id, i);
                if(oldContainer != null)
                {
                    foreach(var item in oldContainer.resources)
                    {
                        if(c.buildingContainer.filter.IsValid(item.resouce))
                            c.resources.Add(item);
                    }
                }

                newContainers.Add(c);
            }
        }

        //add belts
        List<BuildingInfos> beltInfos = new List<BuildingInfos>();
        foreach(var id in beltsId)
        {
            var belt = m_level.buildingList.GetBuilding(id);
            if (belt == null)
                continue;
            var infos = belt.GetInfos();
            if (infos.buildingType != BuildingType.Belt)
                continue;
            beltInfos.Add(infos);
        }

        int beltCount = beltInfos.Count;
        for(int i = 0; i < beltCount; i++)
        {
            var b = beltInfos[i];

            var newNode = new BeltNode();
            newNode.pos = b.pos;
            newNode.rotation = b.rotation;
            newNode.direction = b.beltDirection;

            for(int j = 0; j < beltCount; j++)
            {
                if (i == j)
                    continue;

                var b2 = beltInfos[j];

                var offset = b2.pos - b.pos;

                if (MathF.Abs(offset.x) + MathF.Abs(offset.z) == 1 && Mathf.Abs(offset.y) <= 1)
                {
                    if (newNode.nextIndex < 0 && IsForward(b, b2))
                        newNode.nextIndex = j;
                    else if (IsForward(b2, b))
                        newNode.previousIndexs.Add(j);
                }
            }

            newBelts.Add(newNode);
        }

        //add ports in belts
        for(int i = 0; i < newContainers.Count; i++)
        {
            for(int j = 0; j < newContainers[i].buildingContainer.ports.Count; j++)
            {
                int nextIndex = newBelts.Count;

                var port = newContainers[i].buildingContainer.ports[j];

                var pos = port.pos;
                var nextPos = pos + RotationEx.ToVector3Int(port.rotation);

                int beltIndex = -1;
                for(int k = 0; k < newBelts.Count; k++)
                {
                    if (newBelts[k].pos == nextPos)
                    {
                        beltIndex = k;
                        break;
                    }
                }

                var newNode = new BeltNode();
                newNode.pos = pos;
                newNode.containerIndex = i;
                newNode.portIndex = j;

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
        }

        //change nodes
        m_beltNodes = newBelts;
        m_containers = newContainers;

        ReplaceResources();
    }

    bool IsForward(BuildingInfos first, BuildingInfos second)
    {
        if (first.rotation == RotationEx.Add(second.rotation, Rotation.rot_180))
            return false;

        if (second.beltDirection != BeltDirection.Horizontal && first.rotation != second.rotation)
            return false;

        Vector3Int target = first.pos + RotationEx.ToVector3Int(first.rotation);
        if (first.beltDirection == BeltDirection.Down)
            target.y--;

        if (second.beltDirection == BeltDirection.Up)
            target.y++;

        return target == second.pos;
    }

    void ReplaceResources()
    {
        List<int> removeIDs = new List<int>();

        foreach(var resource in m_resources)
        {
            var r = resource.Value;
            r.beltIndex = -1;

            var pos = new Vector3Int(Mathf.RoundToInt(r.pos.x), Mathf.RoundToInt(r.pos.y), Mathf.RoundToInt(r.pos.z));

            int beltIndex = -1;
            for (int j = 0; j < m_beltNodes.Count; j++)
            {
                if (m_beltNodes[j].pos == pos)
                {
                    beltIndex = j;
                    break;
                }
            }

            if (beltIndex >= 0)
            {
                r.beltIndex = beltIndex;
                m_beltNodes[beltIndex].resources.Add(r);
            }
            else removeIDs.Add(resource.Key);
        }

        foreach (var id in removeIDs)
        {
            m_resources.Remove(id);
            m_level.OnResourceUpdate(id, ElementUpdateType.removed);
        }
    }

    Container GetContainer(int buildingID, int containerIndex)
    {
        foreach (var c in m_containers)
        {
            if (c.buildingID == buildingID && c.containerIndex == containerIndex)
                return c;
        }

        return null;
    }

    public void Process(float deltaTime)
    {
        ProcessBelts(deltaTime);
    }

    public void Save(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var resourcesObject = new JsonObject();
            rootObject.AddElement("Resources", resourcesObject);

            var resourcesArray = new JsonArray();
            resourcesObject.AddElement("List", resourcesArray);
            resourcesObject.AddElement("NextID", new JsonNumber(m_nextID));

            foreach (var r in m_resources)
            {
                var resourceObject = new JsonObject();
                resourcesArray.Add(resourceObject);
                r.Value.Save(resourceObject);
            }
        }
    }

    public void Load(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var resourcesObject = rootObject.GetElement("Resources")?.JsonObject();
            if (resourcesObject != null)
            {
                m_nextID = resourcesObject.GetElement("NextID")?.Int() ?? 0;

                var dataArray = resourcesObject.GetElement("List")?.JsonArray();
                if (dataArray != null)
                {
                    foreach (var elem in dataArray)
                    {
                        var resourceObject = elem.JsonObject();
                        if (resourceObject != null)
                        {
                            Resource r = new Resource();
                            r.Load(resourceObject);
                            if (r.ID > 0)
                                m_resources.Add(r.ID, r);
                        }
                    }
                }
            }
        }
    }

    class BeltMove
    {
        public Resource resource;
        public int newBeltIndex;
    }

    void ProcessBelts(float deltaTime)
    {
        List<BeltMove> movingResources = new List<BeltMove>();

        for (int i = 0; i < m_beltNodes.Count; i++)
        {
            var belt = m_beltNodes[i];

            for (int j = 0; j < belt.resources.Count; j++)
            {
                var r = belt.resources[j];

                var beltPos = new Vector3(belt.pos.x, belt.pos.y, belt.pos.z);

                var dir = r.pos - beltPos;
                float dist = dir.magnitude;
                var rot = RotationEx.FromVector(dir);

                var target = beltPos;

                if (rot == belt.rotation || dist < 0.001f)
                {
                    if (belt.portIndex >= 0)
                    {
                        var port = m_containers[belt.containerIndex].buildingContainer.ports[belt.portIndex];
                        if (port.direction == BuildingPortDirection.inout || port.direction == BuildingPortDirection.input)
                        {
                            var container = m_containers[belt.containerIndex];

                            float nbFree = container.GetFreeSpace();

                            if (nbFree < 1)
                                continue;
                            if (!container.buildingContainer.filter.IsValid(r.resource))
                                continue;

                            r.beltIndex = -1;

                            container.AddResource(r.resource, 1);
                            belt.resources.RemoveAt(j);
                            m_resources.Remove(r.ID);
                            m_level.OnResourceUpdate(r.ID, ElementUpdateType.removed);
                            j--;

                            continue;
                        }
                    }

                    target = beltPos + RotationEx.ToVector3(belt.rotation);
                }

                dir = target - r.pos;

                float dirNorm = dir.magnitude;
                dir /= dirNorm;
                float distMove = deltaTime * Global.instance.allResources.beltSpeed;
                if (distMove > dirNorm)
                    distMove = dirNorm;

                var newPos = r.pos + distMove * dir;

                BeltNode nextBelt = null;
                if (belt.nextIndex >= 0)
                    nextBelt = m_beltNodes[belt.nextIndex];

                m_tempList.Clear();
                foreach (var otherR in belt.resources)
                {
                    if (otherR == r)
                        continue;
                    m_tempList.Add(otherR);
                }
                if (nextBelt != null)
                    m_tempList.AddRange(nextBelt.resources);

                var moving = MoveResource(r, m_tempList, newPos);

                if (!moving)
                    continue;

                Vector3Int newPosI = new Vector3Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y), Mathf.RoundToInt(newPos.z));
                if (newPosI != belt.pos)
                {
                    if (nextBelt == null)
                        continue;

                    if (nextBelt.pos.y != belt.pos.y)
                        newPos.y = nextBelt.pos.y;

                    var beltMove = new BeltMove();
                    beltMove.resource = r;
                    beltMove.newBeltIndex = belt.nextIndex;

                    movingResources.Add(beltMove);
                }

                r.pos = newPos;
            }

            if (belt.portIndex >= 0)
            {
                var port = m_containers[belt.containerIndex].buildingContainer.ports[belt.portIndex];
                if (port.direction == BuildingPortDirection.input)
                    continue;
                if (port.direction == BuildingPortDirection.inout && belt.nextIndex < 0)
                    continue;

                var container = m_containers[belt.containerIndex];
                ResourceType resourceType;
                float count = container.GetFirstResource(false, out resourceType);

                if (count < 1)
                    continue;

                if (!CanCreateResource(i))
                    continue;

                float popped = container.PopResource(resourceType, 1);
                if (popped < 1)
                    continue;

                CreateResource(resourceType, i);
            }
        }

        foreach (var b in movingResources)
        {
            if (b.resource.beltIndex >= 0)
            {
                var belt = m_beltNodes[b.resource.beltIndex];
                b.resource.startDirection = RotationEx.Add(belt.rotation, Rotation.rot_180);
                belt.resources.Remove(b.resource);
            }

            m_beltNodes[b.newBeltIndex].resources.Add(b.resource);
            b.resource.beltIndex = b.newBeltIndex;
        }
    }

    bool MoveResource(Resource current, List<Resource> blocking, Vector3 newPos)
    {
        float space = Global.instance.allResources.beltResourceSpacing;

        var beltPos = m_beltNodes[current.beltIndex].pos;
        var target = GetTarget(current);
        var targetF = new Vector3(target.x, target.y, target.z);
        bool isSelfBlockTarget = target == beltPos;

        foreach (var other in blocking)
        {
            var otherTarget = GetTarget(other);
            var otherTargetF = new Vector3(otherTarget.x, otherTarget.y, otherTarget.z);

            if (!isSelfBlockTarget && otherTarget == beltPos)
                continue;

            if (otherTarget == target)
            {
                if ((other.pos - otherTargetF).SqrMagnitudeXZ() > (current.pos - targetF).SqrMagnitudeXZ())
                    continue;
            }

            float d = (other.pos - newPos).SqrMagnitudeXZ();
            float oldD = (other.pos - current.pos).SqrMagnitudeXZ();

            if (d < space * space && oldD > d)
                return false;
        }

        return true;
    }

    Vector3Int GetTarget(Resource r)
    {
        var belt = m_beltNodes[r.beltIndex];

        Vector3 beltPos = new Vector3(belt.pos.x, belt.pos.y, belt.pos.z);

        var dir = r.pos - beltPos;
        float sqrDist = dir.magnitude;

        var target = belt.pos;

        var rot = RotationEx.FromVector(dir);
        if (rot == belt.rotation || sqrDist < 0.1f * 0.1f)
            target = belt.pos + RotationEx.ToVector3Int(belt.rotation);

        return target;
    }

    bool CanCreateResource(int beltIndex)
    {
        var node = m_beltNodes[beltIndex];

        float space = Global.instance.allResources.beltResourceSpacing;

        Vector3 pos = new Vector3(node.pos.x, node.pos.y, node.pos.z);

        foreach (var r in node.resources)
        {
            float dist = (pos - r.pos).magnitude;
            if (dist <= space)
                return false;
        }

        return true;
    }

    void CreateResource(ResourceType type, int beltIndex)
    {
        var node = m_beltNodes[beltIndex];

        Resource r = new Resource();
        r.ID = m_nextID;
        m_nextID++;
        r.resource = type;
        r.beltIndex = beltIndex;
        r.pos = new Vector3(node.pos.x, node.pos.y, node.pos.z);
        r.startDirection = RotationEx.Add(node.rotation, Rotation.rot_180);

        node.resources.Add(r);

        m_resources.Add(r.ID, r);

        m_level.OnResourceUpdate(r.ID, ElementUpdateType.added);
    }
}
