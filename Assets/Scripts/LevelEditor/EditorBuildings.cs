using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

struct EditorBuilding
{
    public BuildingType buildingType;
    public Vector3Int pos;
    public Rotation rotation;
    public GameObject instance;
}

public class EditorBuildings : MonoBehaviour
{
    [SerializeField] GameObject m_editorBuildingPrefab;

    List<EditorBuilding> m_buildings = new List<EditorBuilding>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorHaveBuildingEvent>.Subscriber(HaveBuilding));
        m_subscriberList.Add(new Event<EditorGetBuildingEvent>.Subscriber(GetBuilding));
        m_subscriberList.Add(new Event<EditorCanPlaceBuildingEvent>.Subscriber(CanPlaceBuilding));
        m_subscriberList.Add(new Event<EditorPlaceBuildingEvent>.Subscriber(PlaceBuilding));
        m_subscriberList.Add(new Event<EditorRemoveBuildingEvent>.Subscriber(RemoveBuilding));

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
            var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
            if(bounds.Contains(e.pos))
            {
                e.haveBuilding = true;
                return;
            }
        }
    }

    void GetBuilding(EditorGetBuildingEvent e)
    {
        foreach (var b in m_buildings)
        {
            var bounds = BuildingDataEx.GetBuildingBounds(b.buildingType, b.pos, b.rotation);
            if (bounds.Contains(e.pos))
            {
                e.buildingPos = b.pos;
                e.rotation = b.rotation;
                e.buildingType = b.buildingType;

                return;
            }
        }
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
        //todo
    }

    void RemoveBuilding(EditorRemoveBuildingEvent e)
    {
        //todo
    }
}

