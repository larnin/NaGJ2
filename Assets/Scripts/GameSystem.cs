using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GameSystem : MonoBehaviour
{
    [SerializeField] float m_growTimer = 5;
    [SerializeField] int m_housePerFactory = 2;
    [SerializeField] int m_housePerLab = 20;
    [SerializeField] float m_minElemTimer = 30;

    bool m_initialized = false;

    int m_nbHouse = 0;
    int m_nbFactory = 0;
    int m_nbLab = 0;

    float m_timer = 0;

    private void Update()
    {
        if(!m_initialized)
            SetInitialState();
        else
        {
            m_timer += Time.deltaTime;
            if(m_timer >= m_growTimer)
            {
                m_timer = 0;
                SpawnBuilding();
            }
        }
    }   
    
    void SetInitialState()
    {
        if (WorldHolder.Instance() == null)
            return;

        m_initialized = true;

        WorldHolder.Instance().SetGround(GroundType.normal, 0, 0);
        WorldHolder.Instance().SetBuilding(BuildingType.house, 1, 0, 0);
        m_nbHouse++;
    }

    void SpawnBuilding()
    {
        if (WorldHolder.Instance() == null)
            return;
        if (ElementHolder.Instance() == null)
            return;

        var validPos = WorldHolder.Instance().GetEmptyBuildingSpaces(m_minElemTimer);
        if (validPos.Count == 0)
            return;

        int validIndex = UnityEngine.Random.Range(0, validPos.Count);

        int nbHouse = ElementHolder.Instance().GetMaxBuildingLevel(BuildingType.house);
        int nbFactory = ElementHolder.Instance().GetMaxBuildingLevel(BuildingType.factory);
        int nbScience = ElementHolder.Instance().GetMaxBuildingLevel(BuildingType.scienceLab);

        int buildingIndex = UnityEngine.Random.Range(0, nbHouse + nbFactory + nbScience);

        BuildingType selectedBuilding = BuildingType.empty;
        int selectedLevel = 0;

        if(buildingIndex < nbHouse)
        {
            selectedBuilding = BuildingType.house;
            selectedLevel = buildingIndex;
        }
        else
        {
            buildingIndex -= nbHouse;
            if(buildingIndex < nbFactory)
            {
                selectedBuilding = BuildingType.factory;
                selectedLevel = buildingIndex;
            }
            else
            {
                buildingIndex -= nbFactory;
                selectedBuilding = BuildingType.scienceLab;
                selectedLevel = buildingIndex;
            }
        }

        WorldHolder.Instance().SetBuilding(selectedBuilding, selectedLevel, validPos[validIndex].x, validPos[validIndex].y);
    }
}
