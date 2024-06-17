using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameBuildingList
{
    List<BuildingBase> m_buildings = new List<BuildingBase>();
    Dictionary<ulong, int> m_posDictionary = new Dictionary<ulong, int>();
    Dictionary<int, int> m_idDictionary = new Dictionary<int, int>();

    int m_nextID = 1;

    GameLevel m_level;

    public GameBuildingList(GameLevel level)
    {
        m_level = level;
    }

    public void Load(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var buildingsObject = rootObject.GetElement("Buildings")?.JsonObject();
            if (buildingsObject != null)
            {
                m_nextID = buildingsObject.GetElement("NextID")?.Int() ?? 0;

                var dataArray = buildingsObject.GetElement("List")?.JsonArray();
                if (dataArray != null)
                {
                    foreach (var elem in dataArray)
                    {
                        var buildingObject = elem.JsonObject();
                        if (buildingObject != null)
                        {
                            var building = BuildingBase.Create(buildingObject, m_level);
                            m_buildings.Add(building);
                        }
                    }
                }
            }
        }

        CreateDictionaties();

        foreach(var b in m_buildings)
            b.Start();
    }

    void CreateDictionaties()
    {
        m_posDictionary.Clear();
        m_idDictionary.Clear();

        for(int x = 0; x < m_buildings.Count; x++)
        {
            m_idDictionary.Add(m_buildings[x].GetID(), x);

            var infos = m_buildings[x].GetInfos();

            var bounds = BuildingDataEx.GetBuildingBounds(infos.buildingType, infos.pos, infos.rotation);
            
            for (int i = bounds.xMin; i < bounds.xMax; i++)
            {
                for (int j = bounds.yMin; j < bounds.yMax; j++)
                {
                    for (int k = bounds.zMin; k < bounds.zMax; k++)
                        m_posDictionary.Add(PosToID(new Vector3Int(i, j, k)), x);
                }
            }
        }
    }

    public void Save(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var worldObject = new JsonObject();
            rootObject.AddElement("Buildings", worldObject);

            var buildingArray = new JsonArray();
            worldObject.AddElement("List", buildingArray);
            worldObject.AddElement("NextID", new JsonNumber(m_nextID));

            foreach(var b in m_buildings)
            {
                var buildingObject = new JsonObject();
                buildingArray.Add(buildingObject);
                b.Save(buildingObject);
            }
        }
    }

    public void Process(float deltaTime)
    {
        foreach (var b in m_buildings)
            b.Process(deltaTime);
    }

    public bool Add(BuildingBase building)
    {
        if (building.GetID() > 0)
            return false;

        var infos = building.GetInfos();

        var bounds = BuildingDataEx.GetBuildingBounds(infos.buildingType, infos.pos, infos.rotation, infos.level);

        for (int i = bounds.xMin; i < bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j < bounds.yMax; j++)
            {
                for (int k = bounds.zMin; k < bounds.zMax; k++)
                {
                    var id = PosToID(new Vector3Int(i, j, k));
                    if (m_posDictionary.ContainsKey(id))
                        return false;
                }
            }
        }

        building.SetID(m_nextID);
        m_nextID++;

        int index = m_buildings.Count();
        m_buildings.Add(building);

        m_idDictionary.Add(building.GetID(), index);

        for (int i = bounds.xMin; i < bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j < bounds.yMax; j++)
            {
                for (int k = bounds.zMin; k < bounds.zMax; k++)
                {
                    var id = PosToID(new Vector3Int(i, j, k));
                    m_posDictionary.Add(id, index);
                }
            }
        }

        building.Start();

        m_level.OnBuildingUpdate(building.GetID(), ElementUpdateType.added);

        return true;
    }

    public bool Remove(int ID)
    {
        int index = -1;
        if (!m_idDictionary.TryGetValue(ID, out index))
            return false;

        var building = m_buildings[index];
        m_buildings.RemoveAt(index);

        var infos = building.GetInfos();

        var bounds = BuildingDataEx.GetBuildingBounds(infos.buildingType, infos.pos, infos.rotation, infos.level);
        for (int i = bounds.xMin; i < bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j < bounds.yMax; j++)
            {
                for (int k = bounds.zMin; k < bounds.zMax; k++)
                {
                    var id = PosToID(new Vector3Int(i, j, k));
                    m_posDictionary.Remove(id);
                }
            }
        }

        //move next keys
        foreach (var k in m_idDictionary.Keys.ToList())
        {
            int value = m_idDictionary[k];
            if (value > index)
                m_idDictionary[k] = value - 1;
        }

        foreach (var k in m_posDictionary.Keys.ToList())
        {
            int value = m_posDictionary[k];
            if (value > index)
                m_posDictionary[k] = value - 1;
        }

        m_level.OnBuildingUpdate(ID, ElementUpdateType.removed);

        return true;
    }

    public int GetBuildingIndex(int ID)
    {
        int index = -1;

        if (!m_idDictionary.TryGetValue(ID, out index))
            return -1;

        return index;
    }

    public BuildingBase GetBuilding(int ID)
    {
        int index = -1;

        if (!m_idDictionary.TryGetValue(ID, out index))
            return null;

        return m_buildings[index];
    }

    public int GetBuildingNb()
    {
        return m_buildings.Count;
    }

    public BuildingBase GetBuildingFromIndex(int index)
    {
        if (index < 0 || index >= m_buildings.Count)
            return null;

        return m_buildings[index];
    }

    public int GetBuildingIndexAt(Vector3Int pos)
    {
        var id = PosToID(pos);

        int index = -1;
        if (!m_posDictionary.TryGetValue(id, out index))
            return -1;

        return index;
    }

    public BuildingBase GetBuildingAt(Vector3Int pos)
    {
        var id = PosToID(pos);

        int index = -1;
        if (!m_posDictionary.TryGetValue(id, out index))
            return null;

        return m_buildings[index];
    }

    public void GetNearBelts(Vector3Int pos, NearMatrix3<SimpleBeltInfos> beltsInfos)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    var data = new SimpleBeltInfos { haveBelt = false };

                    Vector3Int currentPos = pos + new Vector3Int(i, j, k);

                    var building = GetBuildingAt(currentPos);
                    if (building != null)
                    {
                        var infos = building.GetInfos();

                        if (infos.buildingType == BuildingType.Belt)
                        {
                            data.haveBelt = true;
                            data.rotation = infos.rotation;
                        }
                    }

                    beltsInfos.Set(data, i, j, k);
                }
            }
        }
    }

    public void GetNearPipes(Vector3Int pos, NearMatrix3<bool> pipeInfos)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    Vector3Int currentPos = pos + new Vector3Int(i, j, k);

                    bool havePipe = false;

                    var building = GetBuildingAt(currentPos);
                    if (building != null)
                    {
                        var infos = building.GetInfos();

                        if (infos.buildingType == BuildingType.Pipe)
                            havePipe = true;
                    }

                    pipeInfos.Set(havePipe, i, j, k);
                }
            }
        }
    }

    public List<int> GetAllBeltID()
    {
        List<int> ids = new List<int>();

        foreach(var b in m_buildings)
        {
            var infos = b.GetInfos();
            if (infos.buildingType != BuildingType.Belt)
                continue;

            ids.Add(b.GetID());
        }

        return ids;
    }

    public List<int> GetAllBuildingContainers()
    {
        List<int> ids = new List<int>();

        foreach (var b in m_buildings)
        {
            var infos = b.GetInfos();
            if(BuildingDataEx.GetContainer(infos.buildingType) == null)
                continue;

            ids.Add(b.GetID());
        }

        return ids;
    }

    public bool Raycast(Vector3 pos, Vector3 dir, out Vector3 hit, out Vector3 normal, out int buildingID)
    {
        hit = Vector3.zero;
        normal = Vector3.zero;
        buildingID = 0;

        dir = dir.normalized;

        var size = Global.instance.allBlocks.blockSize;

        bool haveHit = false;
        float distance = 0;

        foreach(var b in m_buildings)
        {
            var infos = b.GetInfos();

            var boundint = BuildingDataEx.GetBuildingBounds(infos.buildingType, infos.pos, infos.rotation, infos.level);
            var bounds = new Bounds(new Vector3(boundint.center.x * size.x, (boundint.center.y - 0.5f) * size.y, boundint.center.z * size.z),
                new Vector3(boundint.size.x * size.x, boundint.size.y * size.y, boundint.size.z * size.z));

            Vector3 localpos, localNormal;
            var shape = Collisions.GetShape(bounds);
            bool localHit = Collisions.Raycast(shape, pos, dir, out localpos, out localNormal);
            if(localHit)
            {
                float localDistance = (localpos - pos).sqrMagnitude;
                if(!haveHit || localDistance < distance)
                {
                    haveHit = true;
                    distance = localDistance;
                    normal = localNormal;
                    hit = localpos;
                    buildingID = b.GetID();
                }
            }
        }

        return haveHit;
    }

    ulong PosToID(Vector3Int pos)
    {
        const int powLimit = 20;
        const int absLimit = 1 << powLimit;

        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(pos[i]) > absLimit - 1)
                pos[i] = (absLimit - 1) * ((pos[i] > 0) ? 1 : -1);
            pos[i] += absLimit - 1;
        }

        ulong ID = (ulong)pos.x;
        ID <<= powLimit;
        ID += (ulong)pos.y;
        ID <<= powLimit;
        ID += (ulong)pos.z;

        return ID;
    }
}
