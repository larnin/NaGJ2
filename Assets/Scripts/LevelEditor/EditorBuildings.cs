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
        foreach (var b in m_buildings)
        {
            var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
            if (bounds.Contains(e.pos))
            {
                e.buildingPos = b.pos;
                e.rotation = b.rotation;
                e.buildingType = b.buildingType;
                e.team = b.team;
                e.ID = b.ID;

                return;
            }
        }
    }

    void GetBuilding(EditorGetBuildingEvent e)
    {
        var b = GetBuilding(e.ID);
        if(b != null)
        {

            e.buildingPos = b.pos;
            e.rotation = b.rotation;
            e.buildingType = b.buildingType;
            e.team = b.team;
        }
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

        for(int i = newBounds.xMin; i <= newBounds.xMax; i++)
        {
            for(int j = newBounds.yMin; j <= newBounds.yMax; j++)
            {
                for(int k = newBounds.zMin; k <= newBounds.zMax; k++)
                {
                    EditorGetBlockEvent blockData = new EditorGetBlockEvent(new Vector3Int(i, j, k));
                    Event<EditorGetBlockEvent>.Broadcast(blockData);
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
    }

    void RemoveBuilding(EditorRemoveBuildingEvent e)
    {
        var b = GetBuilding(e.ID);
        if(b.ID == e.ID)
        {
            DestroyBuilding(b);
            m_buildings.Remove(b);
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

        Event<EditorSetBuildingInstance>.Broadcast(new EditorSetBuildingInstance(b.ID), b.instance);
    }

    void DestroyBuilding(BuildingElement b)
    {
        Destroy(b.instance);
    }
}

