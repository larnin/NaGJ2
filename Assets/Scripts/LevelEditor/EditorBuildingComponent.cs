using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorBuildingComponent : MonoBehaviour
{
    int m_buildingID;
    BuildingType m_type;
    Team m_team;
    Vector3Int m_pos;
    Rotation m_rot;

    SubscriberList m_subscriberList = new SubscriberList();

    GameObject m_instance;

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorSetBuildingInstanceEvent>.LocalSubscriber(SetInstance, gameObject));
        m_subscriberList.Add(new Event<EditorUpdateInstanceEvent>.LocalSubscriber(UpdateInstance, gameObject));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetInstance(EditorSetBuildingInstanceEvent e)
    {
        Vector3 size = Global.instance.allBlocks.blockSize;
        
        EditorGetBuildingEvent building = new EditorGetBuildingEvent(e.ID);
        Event<EditorGetBuildingEvent>.Broadcast(building);

        Vector3 offset = new Vector3(size.x * building.building.pos.x, size.y * building.building.pos.y, size.z * building.building.pos.z);

        transform.localPosition = offset;

        m_buildingID = e.ID;
        m_type = building.building.buildingType;
        m_team = building.building.team;
        m_pos = building.building.pos;
        m_rot = building.building.rotation;

        SetBuilding();
    }

    void SetBuilding()
    {
        if (m_instance != null)
            Destroy(m_instance);

        if(m_type == BuildingType.Belt)
        {
            SetBelt();
            return;
        }

        var prefab = BuildingDataEx.GetBaseBuildingPrefab(m_type);

        var obj = Instantiate(prefab);
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = RotationEx.ToQuaternion(m_rot);

        m_instance = obj;
    }

    void SetBelt()
    {
        GetNearBlocsEvent blocks = new GetNearBlocsEvent(m_pos);
        Event<GetNearBlocsEvent>.Broadcast(blocks);

        GetNearBeltsEvent belts = new GetNearBeltsEvent(m_pos);
        Event<GetNearBeltsEvent>.Broadcast(belts);

        var obj = BuildingDataEx.InstantiateBelt(Vector3Int.zero, blocks.matrix, belts.matrix);
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;

        m_instance = obj;
    }

    void UpdateInstance(EditorUpdateInstanceEvent e)
    {
        SetBuilding();
    }
}
