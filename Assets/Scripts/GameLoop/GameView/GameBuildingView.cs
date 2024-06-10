using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameBuildingView : MonoBehaviour
{
    BuildingBase m_building;

    public void SetBuilding(BuildingBase building)
    {
        m_building = building;
    }
    public BuildingBase GetBuildng()
    {
        return m_building;
    }
}