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

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorSetBuildingInstance>.LocalSubscriber(SetInstance, gameObject));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetInstance(EditorSetBuildingInstance e)
    {
        Vector3 size = Global.instance.allBlocks.blockSize;
        
        EditorGetBuildingEvent building = new EditorGetBuildingEvent(e.ID);
        Event<EditorGetBuildingEvent>.Broadcast(building);

        Vector3 offset = new Vector3(size.x * building.buildingPos.x, size.y * building.buildingPos.y, size.z * building.buildingPos.z);

        transform.localPosition = offset;

        m_buildingID = e.ID;
        m_type = building.buildingType;
        m_team = building.team;
        m_pos = building.buildingPos;
        m_rot = building.rotation;

        SetBuilding();
    }

    void SetBuilding()
    {
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
    }

    void SetBelt()
    {
        //todo later

        var prefab = BuildingDataEx.GetBaseBuildingPrefab(m_type);

        var obj = Instantiate(prefab);
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = RotationEx.ToQuaternion(m_rot);
    }
}
