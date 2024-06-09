using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    List<BuildingElement> m_buildings = new List<BuildingElement>();
    Dictionary<ulong, int> m_posDictionary = new Dictionary<ulong, int>();
    Dictionary<int, int> m_idDictionary = new Dictionary<int, int>();

    SubscriberList m_subscriberList = new SubscriberList();

    int m_nextID = 1;

    private void Awake()
    {
        m_subscriberList.Add(new Event<PlaceBuildingEvent>.Subscriber(PlaceBuilding));
        m_subscriberList.Add(new Event<RemoveBuildingEvent>.Subscriber(RemoveBuilding));
        m_subscriberList.Add(new Event<GetBuildingEvent>.Subscriber(GetBuilding));
        m_subscriberList.Add(new Event<GetBuildingAtEvent>.Subscriber(GetBuildingAt));
        m_subscriberList.Add(new Event<GetBuildingNbEvent>.Subscriber(GetBuildingNb));
        m_subscriberList.Add(new Event<GetBuildingByIndexEvent>.Subscriber(GetBuildingByIndex));

        m_subscriberList.Add(new Event<GetBuildingBeltsEvent>.Subscriber(GetBelts));
        m_subscriberList.Add(new Event<GetNearBeltsEvent>.Subscriber(GetNearBelts));
        m_subscriberList.Add(new Event<GetNearPipesEvent>.Subscriber(GetNearPipes));

        m_subscriberList.Add(new Event<SaveEvent>.Subscriber(OnSave));
        m_subscriberList.Add(new Event<LoadEvent>.Subscriber(OnLoad));
        m_subscriberList.Add(new Event<LoadEndedEvent>.Subscriber(OnLoadEnd));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnSave(SaveEvent e)
    {
        BuildingEx.Save(e.document, m_buildings, m_nextID);
    }

    void OnLoad(LoadEvent e)
    {
        m_buildings = BuildingEx.Load(e.document, out m_nextID);

        CreateDictionaries();
    }

    void OnLoadEnd(LoadEndedEvent e)
    {
        foreach (var b in m_buildings)
            UpdateBuilding(b);
    }

    void CreateDictionaries()
    {
        m_posDictionary.Clear();
        m_idDictionary.Clear();

        for (int x = 0; x < m_buildings.Count; x++)
        {
            var b = m_buildings[x];
            m_idDictionary.Add(b.ID, x);

            var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
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

    void UpdateBuilding(BuildingElement b)
    {
        RemoveBuilding(b);

        GameObject instance = null;

        if(b.buildingType == BuildingType.Belt)
        {
            GetNearBlocsEvent blocks = new GetNearBlocsEvent(b.pos);
            Event<GetNearBlocsEvent>.Broadcast(blocks);

            GetNearBeltsEvent belts = new GetNearBeltsEvent(b.pos);
            Event<GetNearBeltsEvent>.Broadcast(belts);

            BeltDirection dir;
            instance = BuildingDataEx.InstantiateBelt(b.pos, blocks.matrix, belts.matrix, out dir, transform);
            b.beltDirection = dir;
        }
        else if(b.buildingType == BuildingType.Pipe)
        {
            GetNearBlocsEvent blocks = new GetNearBlocsEvent(b.pos);
            Event<GetNearBlocsEvent>.Broadcast(blocks);

            GetNearPipesEvent pipes = new GetNearPipesEvent(b.pos);
            Event<GetNearPipesEvent>.Broadcast(pipes);

            instance = BuildingDataEx.InstantiatePipe(b.pos, blocks.matrix, pipes.matrix, transform);
            b.beltDirection = BeltDirection.Horizontal;
        }
        else
        {
            var prefab = BuildingDataEx.GetBaseBuildingPrefab(b.buildingType, b.level);
            if (prefab == null)
                return;
            instance = Instantiate(prefab);

            var size = Global.instance.allBlocks.blockSize;
            Vector3 offset = new Vector3(size.x * b.pos.x, size.y * b.pos.y, size.z * b.pos.z);
            instance.transform.parent = transform;
            instance.transform.localPosition = offset;
            instance.transform.localRotation = RotationEx.ToQuaternion(b.rotation);
            b.beltDirection = BeltDirection.Horizontal;
        }

        var buildingID = instance.GetComponent<BuildingID>();
        if(buildingID == null)
            buildingID = instance.AddComponent<BuildingID>();

        Event<SetBuildingInstanceIDEvent>.Broadcast(new SetBuildingInstanceIDEvent(b.ID), instance);

        b.instance = instance;
    }

    void RemoveBuilding(BuildingElement b)
    {
        if(b.instance != null)
        {
            Destroy(b.instance);
        }
    }

    ulong PosToID(Vector3Int pos)
    {
        const int powLimit = 20;
        const int absLimit = 1 << powLimit;

        for(int i = 0; i < 3; i++)
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

    void PlaceBuilding(PlaceBuildingEvent e)
    {
        e.ID = 0;

        var bounds = BuildingDataEx.GetBuildingBounds(e.buildingType, e.pos, e.rotation, e.level);

        for (int i = bounds.xMin; i < bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j < bounds.yMax; j++)
            {
                for (int k = bounds.zMin; k < bounds.zMax; k++)
                {
                    var id = PosToID(new Vector3Int(i, j, k));
                    if (m_posDictionary.ContainsKey(id))
                        return;
                }
            }
        }


        e.ID = m_nextID;
        m_nextID++;

        BuildingElement b = new BuildingElement();
        b.ID = e.ID;
        b.pos = e.pos;
        b.team = e.team;
        b.rotation = e.rotation;
        b.buildingType = e.buildingType;
        b.level = e.level;

        int index = m_buildings.Count;
        m_buildings.Add(b);

        m_idDictionary.Add(e.ID, index);

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

        UpdateBuilding(b);

        Event<BuildingUpdatedEvent>.Broadcast(new BuildingUpdatedEvent(e.ID, bounds));
    }

    void RemoveBuilding(RemoveBuildingEvent e)
    {
        if(!m_idDictionary.ContainsKey(e.ID))
        {
            e.removed = false;
            return;
        }

        e.removed = true;

        int index = m_idDictionary[e.ID];

        var b = m_buildings[index];
        RemoveBuilding(b);
        m_buildings.RemoveAt(index);

        m_idDictionary.Remove(e.ID);
        foreach(var k in m_idDictionary.Keys.ToList())
        {
            int value = m_idDictionary[k];
            if (value > index)
                m_idDictionary[k] = value - 1;
        }

        var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
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
        foreach (var k in m_posDictionary.Keys.ToList())
        {
            int value = m_posDictionary[k];
            if (value > index)
                m_posDictionary[k] = value - 1;
        }

        Event<BuildingUpdatedEvent>.Broadcast(new BuildingUpdatedEvent(e.ID, bounds));
    }

    void GetBuilding(GetBuildingEvent e)
    {
        e.element = null;

        int index = -1;
        if (!m_idDictionary.TryGetValue(e.ID, out index))
            return;

        e.element = m_buildings[index].Clone();
    }

    BuildingElement GetBuildingAt(Vector3Int pos)
    {
        int index = -1;
        var ID = PosToID(pos);
        if (!m_posDictionary.TryGetValue(ID, out index))
            return null;

        return m_buildings[index];
    }

    void GetBuildingAt(GetBuildingAtEvent e)
    {
        var element = GetBuildingAt(e.pos);
        if (element == null)
            e.element = null;
        else e.element = element.Clone();
    }

    void GetBuildingNb(GetBuildingNbEvent e)
    {
        e.nb = m_buildings.Count;
    }

    void GetBuildingByIndex(GetBuildingByIndexEvent e)
    {
        if (e.index < 0 || e.index >= m_buildings.Count)
            e.element = null;
        else e.element = m_buildings[e.index].Clone();
    }

    void GetBelts(GetBuildingBeltsEvent e)
    {
        e.belts.Clear();
        e.ports.Clear();
        e.containers.Clear();

        GetBuildingPortsEvent portData = new GetBuildingPortsEvent();

        foreach (var b in m_buildings)
        {
            int firstContainer = e.containers.Count;

            portData.ports.Clear();
            portData.containers.Clear();
            Event<GetBuildingPortsEvent>.Broadcast(portData, b.instance);
            foreach (var d in portData.ports)
            {
                d.containerIndex += firstContainer;
                e.ports.Add(d);
            }

            foreach(var d in portData.containers)
                e.containers.Add(d);

            if (b.buildingType == BuildingType.Belt)
            {
                var data = new BuildingOneBeltDataOLD();
                data.pos = b.pos;
                data.rotation = b.rotation;
                data.direction = b.beltDirection;
                e.belts.Add(data);
            }
        }
    }

    void GetNearBelts(GetNearBeltsEvent e)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    var data = new SimpleBeltInfos { haveBelt = false };

                    Vector3Int pos = e.pos + new Vector3Int(i, j, k);

                    var building = GetBuildingAt(pos);
                    if(building != null && building.buildingType == BuildingType.Belt)
                    {
                        data.haveBelt = true;
                        data.rotation = building.rotation;
                    }

                    e.matrix.Set(data, i, j, k);
                }
            }
        }
    }

    void GetNearPipes(GetNearPipesEvent e)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    Vector3Int pos = e.pos + new Vector3Int(i, j, k);

                    var building = GetBuildingAt(pos);
                    if (building != null && building.buildingType == BuildingType.Pipe)
                        e.matrix.Set(true, i, j, k);
                    else e.matrix.Set(false, i, j, k);
                }
            }
        }
    }
}
