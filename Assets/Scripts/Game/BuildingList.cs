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
        m_subscriberList.Add(new Event<GetBuildingBeltsEvent>.Subscriber(GetBelts));
        m_subscriberList.Add(new Event<GetNearBeltsEvent>.Subscriber(GetNearBelts));

        m_subscriberList.Add(new Event<SaveEvent>.Subscriber(OnSave));
        m_subscriberList.Add(new Event<LoadEvent>.Subscriber(OnLoad));

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

        var prefab = BuildingDataEx.GetBaseBuildingPrefab(b.buildingType, b.level);
        if (prefab == null)
            return;

        var size = Global.instance.allBlocks.blockSize;
        Vector3 offset = new Vector3(size.x * b.pos.x, size.y * b.pos.y, size.z * b.pos.z);
        
        var instance = Instantiate(prefab);
        instance.transform.parent = transform;

        instance.transform.localPosition = offset;
        instance.transform.localRotation = RotationEx.ToQuaternion(b.rotation);

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

        var bounds = BuildingDataEx.GetBuildingBounds(e.buildingType, e.pos, e.rotation);

        for (int i = bounds.xMin; i <= bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j <= bounds.yMax; j++)
            {
                for (int k = bounds.zMin; k <= bounds.zMax; k++)
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

        int index = m_buildings.Count;
        m_buildings.Add(b);

        m_idDictionary.Add(e.ID, index);

        for (int i = bounds.xMin; i <= bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j <= bounds.yMax; j++)
            {
                for (int k = bounds.zMin; k <= bounds.zMax; k++)
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

        m_idDictionary.Remove(e.ID);
        foreach(var i in m_idDictionary)
        {
            if (i.Value > index)
                m_idDictionary[i.Key] = i.Value - 1;
        }

        var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
        for (int i = bounds.xMin; i <= bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j <= bounds.yMax; j++)
            {
                for (int k = bounds.zMin; k <= bounds.zMax; k++)
                {
                    var id = PosToID(new Vector3Int(i, j, k));
                    m_posDictionary.Remove(id);
                }
            }
        }
        foreach (var p in m_posDictionary)
        {
            if (p.Value > index)
                m_posDictionary[p.Key] = p.Value - 1;
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
                var data = new BuildingOneBeltData();
                data.pos = b.pos;
                data.rotation = b.rotation;
                data.verticalOffset = 0; //todo later
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
                    var data = new GetNearBeltsEvent.BeltData { haveBelt = false };

                    Vector3Int pos = e.pos + new Vector3Int(i, j, k);

                    var building = GetBuildingAt(pos);
                    if(building.buildingType == BuildingType.Belt)
                    {
                        data.haveBelt = true;
                        data.rotation = building.rotation;
                    }

                    e.matrix.Set(data, i, j, k);
                }
            }
        }
    }
}
