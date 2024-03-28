using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorPlaceBuilding : MonoBehaviour
{
    GameObject m_buildingInstance;

    BuildingType m_buildingType;
    int m_level;

    Rotation m_rotation = Rotation.rot_0;

    public void SetBuilding(BuildingType type, int level)
    {
        m_buildingType = type;
        m_level = level;

        LoadInstance();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                m_rotation = RotationEx.Sub(m_rotation, Rotation.rot_90);
            else m_rotation = RotationEx.Add(m_rotation, Rotation.rot_90);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Event<EnableSelectCursorEvent>.Broadcast(new EnableSelectCursorEvent());
            return;
        }

        if (Input.GetMouseButtonDown(0))
            PlaceBuilding();

        UpdateBuildingState();
    }

    void LoadInstance()
    {
        

    }

    Vector3Int GetCursorPosition()
    {
        return Vector3Int.zero;
    }

    bool CanPlaceAt(Vector3Int pos)
    {
        return false;
    }

    void UpdateBuildingState()
    {

    }

    void PlaceBuilding()
    {

    }
}
