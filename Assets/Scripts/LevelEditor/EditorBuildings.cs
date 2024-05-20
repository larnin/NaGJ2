using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorBuildings : MonoBehaviour
{
    List<BuildingElement> m_buildings = new List<BuildingElement>();

    SubscriberList m_subscriberList = new SubscriberList();

    int m_nextID = 1;

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorHaveBuildingEvent>.Subscriber(HaveBuilding));
        m_subscriberList.Add(new Event<EditorGetBuildingEvent>.Subscriber(GetBuilding));
        m_subscriberList.Add(new Event<EditorGetBuildingAtEvent>.Subscriber(GetBuildingAt));
        m_subscriberList.Add(new Event<EditorCanPlaceBuildingEvent>.Subscriber(CanPlaceBuilding));
        m_subscriberList.Add(new Event<EditorPlaceBuildingEvent>.Subscriber(PlaceBuilding));
        m_subscriberList.Add(new Event<EditorRemoveBuildingEvent>.Subscriber(RemoveBuilding));
        m_subscriberList.Add(new Event<SaveEvent>.Subscriber(OnSave));
        m_subscriberList.Add(new Event<LoadEvent>.Subscriber(OnLoad));
        m_subscriberList.Add(new Event<NewLevelEvent>.Subscriber(OnNewLevel));
        m_subscriberList.Add(new Event<GetNearBeltsEvent>.Subscriber(GetNearBelts));
        m_subscriberList.Add(new Event<GetNearPipesEvent>.Subscriber(GetNearPipes));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void HaveBuilding(EditorHaveBuildingEvent e)
    {
        e.haveBuilding = false;

        foreach(var b in m_buildings)
        {
            if(b.ID == e.ID)
            {
                e.haveBuilding = true;
            }
        }
    }

    void GetBuildingAt(EditorGetBuildingAtEvent e)
    {
        var b = GetBuildingAt(e.pos);
        if (b != null)
            e.building = b.Clone();
    }

    BuildingElement GetBuildingAt(Vector3Int pos)
    {
        foreach (var b in m_buildings)
        {
            var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
            if (bounds.Contains(pos))
                return b;
        }

        return null;
    }

    void GetBuilding(EditorGetBuildingEvent e)
    {
        var b = GetBuilding(e.ID);
        if(b != null)
            e.building = b.Clone();
    }

    BuildingElement GetBuilding(int ID)
    {
        foreach (var b in m_buildings)
        {
            if (b.ID == ID)
                return b;
        }

        return null;
    }

    void CanPlaceBuilding(EditorCanPlaceBuildingEvent e)
    {
        e.canBePlaced = true;

        var newBounds = BuildingDataEx.GetBuildingBounds(e.buildingType, e.pos, e.rotation);

        foreach (var b in m_buildings)
        {
            var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
            if(bounds.Intersects(newBounds))
            {
                e.canBePlaced = false;
                return;
            }
        }

        for(int i = newBounds.xMin; i < newBounds.xMax; i++)
        {
            for(int j = newBounds.yMin; j < newBounds.yMax; j++)
            {
                for(int k = newBounds.zMin; k < newBounds.zMax; k++)
                {
                    GetBlockEvent blockData = new GetBlockEvent(new Vector3Int(i, j, k));
                    Event<GetBlockEvent>.Broadcast(blockData);
                    if(blockData.type != BlockType.air)
                    {
                        e.canBePlaced = false;
                        return;
                    }
                }
            }
        }
    }

    void PlaceBuilding(EditorPlaceBuildingEvent e)
    {
        BuildingElement b = new BuildingElement();
        b.ID = m_nextID;
        m_nextID++;
        b.pos = e.pos;
        b.rotation = e.rotation;
        b.team = e.team;
        b.buildingType = e.buildingType;

        m_buildings.Add(b);
        UpdateBuilding(b);

        if (NeedUpdate(e.buildingType))
        UpdateNearBuildings(e.pos);
    }

    void RemoveBuilding(EditorRemoveBuildingEvent e)
    {
        var b = GetBuilding(e.ID);
        if(b.ID == e.ID)
        {
            DestroyBuilding(b);
            m_buildings.Remove(b);            
            
            if (NeedUpdate(b.buildingType))
                UpdateNearBuildings(b.pos);
        }
    }

    void UpdateNearBuildings(Vector3Int pos)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (i == 0 && j == 0 && k == 0)
                        continue;

                    var building = GetBuildingAt(pos + new Vector3Int(i, j, k));

                    if (building == null || building.instance == null || !NeedUpdate(building.buildingType))
                        continue;

                    Event<EditorUpdateInstanceEvent>.Broadcast(new EditorUpdateInstanceEvent(), building.instance);
                }
            }
        }
    }

    void OnSave(SaveEvent e)
    {
        BuildingEx.Save(e.document, m_buildings, m_nextID);
    }

    void OnLoad(LoadEvent e)
    {
        OnNewLevel(new NewLevelEvent());

        m_buildings = BuildingEx.Load(e.document, out m_nextID);

        foreach (var b in m_buildings)
            UpdateBuilding(b);
    }

    void OnNewLevel(NewLevelEvent e)
    {
        foreach (var b in m_buildings)
            DestroyBuilding(b);
        m_buildings.Clear();

        m_nextID = 1;
    }

    void UpdateBuilding(BuildingElement b)
    {
        if(b.instance == null)
        {
            b.instance = new GameObject("Building " + b.ID);
            b.instance.AddComponent<EditorBuildingComponent>();
            b.instance.transform.parent = transform;
        }

        Event<EditorSetBuildingInstanceEvent>.Broadcast(new EditorSetBuildingInstanceEvent(b.ID), b.instance);
    }

    void DestroyBuilding(BuildingElement b)
    {
        Destroy(b.instance);
    }

    bool NeedUpdate(BuildingType b)
    {
        return b == BuildingType.Belt || b == BuildingType.Pipe;
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

                    if (building != null && building.buildingType == BuildingType.Belt)
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

