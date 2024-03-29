﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class OLDOneLevelInfo
{
    public int populationPerHouse = 1;
    public int populationPerFactory = 1;
    public float moneyPerHouse = 1;
    public float moneyPerFactory = 1;
}

public class OLDCorruptionInfo
{
    public Vector2Int pos;
    public Vector2Int buildingPos;

    public OLDCorruptionInfo(Vector2Int _pos, Vector2Int _buildingPos)
    {
        pos = _pos;
        buildingPos = _buildingPos;
    }
}

class OLDGameSystem : MonoBehaviour
{
    [SerializeField] float m_growTimer = 5;
    [SerializeField] int m_populationPerLab = 10;
    [SerializeField] List<OLDOneLevelInfo> m_levelInfos;
    [SerializeField] float m_minElemTimer = 30;
    [SerializeField] float m_labWeight = 0.1f;
    [SerializeField] int m_initialMoney = 10;
    [SerializeField] GameObject m_pauseMenu;
    [SerializeField] GameObject m_gameOverMenu;

    List<OLDCorruptionInfo> m_corruption = new List<OLDCorruptionInfo>();

    bool m_initialized = false;

    int m_population = 0;
    int m_workingPopulation = 0;
    float m_moneyGeneration = 0;
    int m_money;
    float m_moneyToAdd; //range[0;1]
    OldBuildingType m_nextWantedBuilding = OldBuildingType.empty;

    int m_bestMoney = 0;
    int m_bestPopulation = 0;

    float m_timer = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    static OLDGameSystem m_instance = null;

    public static OLDGameSystem Instance()
    {
        return m_instance;
    }

    private void Awake()
    {
        m_instance = this;

        m_subscriberList.Add(new Event<GetPopulationAndMoneyEvent>.Subscriber(GetPopulationAndMoney));
        m_subscriberList.Add(new Event<GetNearestBuildingEvent>.Subscriber(GetNearestBuilding));
        m_subscriberList.Add(new Event<GetMaxPopulationMoneyWaveEvent>.Subscriber(GetMax));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        m_money = m_initialMoney;
    }

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

        m_moneyToAdd += m_moneyGeneration * Time.deltaTime;
        m_money += Mathf.FloorToInt(m_moneyToAdd);
        m_moneyToAdd = m_moneyToAdd - MathF.Floor(m_moneyToAdd);

        if (m_money > m_bestMoney)
            m_bestMoney = m_money;
        if (m_population > m_bestPopulation)
            m_bestPopulation = m_population;

        if(!Gamestate.instance.paused)
        {
            if(m_population <= 0)
            {
                Instantiate(m_gameOverMenu);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))

                {
                    GetSelectedCursorButtonEvent e = new GetSelectedCursorButtonEvent();
                    Event<GetSelectedCursorButtonEvent>.Broadcast(e);
                    if (e.m_currentButton == -1)
                        Instantiate(m_pauseMenu);
                }
                
            }
        }
    }   
    
    void SetInitialState()
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        m_initialized = true;

        OLDWorldHolder.Instance().SetGround(OLDGroundType.normal, 0, 0);
        OLDWorldHolder.Instance().SetBuilding(OldBuildingType.house, 1, 0, 0);
        CountBuildings();
    }

    void SpawnBuilding()
    {
        if (OLDWorldHolder.Instance() == null)
            return;
        if (OLDElementHolder.Instance() == null)
            return;

        var validPos = OLDWorldHolder.Instance().GetEmptyBuildingSpaces(m_minElemTimer);
        if (validPos.Count == 0)
            return;

        if(m_nextWantedBuilding == OldBuildingType.empty)
        {
            if (UnityEngine.Random.value < m_labWeight)
                m_nextWantedBuilding = OldBuildingType.scienceLab;
            else m_nextWantedBuilding = OldBuildingType.factory;
        }
        if (m_workingPopulation <= 4)
            m_nextWantedBuilding = OldBuildingType.factory;


        int validIndex = UnityEngine.Random.Range(0, validPos.Count);

        Vector2Int pos = validPos[validIndex];

        var groundType = GetGroundType(pos.x, pos.y);
        if (groundType == OLDGroundType.empty || groundType == OLDGroundType.deadly)
            return;

        int level = 1;
        if (groundType == OLDGroundType.low)
            level = 0;
        else if (groundType == OLDGroundType.hight)
            level = 2;

        int nbWantedWorker = 0;
        if (m_nextWantedBuilding == OldBuildingType.factory && m_levelInfos.Count > level)
            nbWantedWorker = m_levelInfos[level].populationPerFactory;
        else if (m_nextWantedBuilding == OldBuildingType.scienceLab)
            nbWantedWorker = m_populationPerLab;

        int nbFreeWorker = m_population - m_workingPopulation;

        if (nbFreeWorker >= nbWantedWorker)
        {
            if (m_nextWantedBuilding == OldBuildingType.scienceLab)
                level = 0;
            OLDWorldHolder.Instance().SetBuilding(m_nextWantedBuilding, level, pos.x, pos.y);
            m_nextWantedBuilding = OldBuildingType.empty;
        }
        else OLDWorldHolder.Instance().SetBuilding(OldBuildingType.house, level, pos.x, pos.y);

        CountBuildings();
    }

    void CountBuildings()
    {
        m_population = 0;
        m_workingPopulation = 0;
        m_moneyGeneration = 0;

        if (OLDWorldHolder.Instance() == null)
            return;

        var bounds = OLDWorldHolder.Instance().GetBounds();

        for(int i = bounds.x; i < bounds.x + bounds.width; i++)
        {
            for(int j = bounds.y; j < bounds.y + bounds.height; j++)
            {
                int level = 0;
                var type = OLDWorldHolder.Instance().GetBuilding(i, j, out level);

                if(type == OldBuildingType.house)
                {
                    if(m_levelInfos.Count > level)
                    {
                        m_population += m_levelInfos[level].populationPerHouse;
                        m_moneyGeneration += m_levelInfos[level].moneyPerHouse;
                    }
                }
                else if(type == OldBuildingType.factory)
                {
                    if(m_levelInfos.Count > level)
                    {
                        m_workingPopulation += m_levelInfos[level].populationPerFactory;
                        m_moneyGeneration += m_levelInfos[level].moneyPerFactory; 
                    }
                }   
                else if(type == OldBuildingType.scienceLab)
                {
                    m_workingPopulation += m_populationPerLab;
                }
            }
        }
    }

    Vector2Int GetNearestBuilding(Vector3 pos, out bool found)
    {
        found = false;

        if (OLDWorldHolder.Instance() == null)
            return Vector2Int.zero;

        Vector2Int currentPos = OLDWorldHolder.Instance().ProjectPos(pos);

        var bounds = OLDWorldHolder.Instance().GetBounds();

        Vector2Int bestPos = Vector2Int.zero;
        float bestDist = -1;

        for (int i = bounds.x; i < bounds.x + bounds.width; i++)
        {
            for (int j = bounds.y; j < bounds.y + bounds.height; j++)
            {
                var type = OLDWorldHolder.Instance().GetBuilding(i, j);
                if(type == OldBuildingType.factory || type == OldBuildingType.house || type == OldBuildingType.scienceLab)
                {
                    Vector2Int posItem = new Vector2Int(i, j);
                    var dir = currentPos - posItem;
                    float dist = dir.x * dir.x + dir.y * dir.y;

                    if(dist < bestDist || bestDist < 0)
                    {
                        bestDist = dist;
                        bestPos = posItem;
                    }
                }
            }
        }

        if (bestDist >= 0)
            found = true;

        return bestPos;
    }

    void GetPopulationAndMoney(GetPopulationAndMoneyEvent e)
    {
        e.population = m_population;
        e.money = m_money;
    }

    void GetNearestBuilding(GetNearestBuildingEvent e)
    {
        e.buildingPos = GetNearestBuilding(e.currentPos, out e.buildingFound);
    }

    void GetMax(GetMaxPopulationMoneyWaveEvent e)
    {
        e.money = m_bestMoney;
        e.population = m_bestPopulation;
    }

    public void PlaceTower(int x, int y, OldBuildingType type, int level)
    {
        if (OLDElementHolder.Instance() == null)
            return;

        if (OLDWorldHolder.Instance() == null)
            return;

        int cost = OLDElementHolder.Instance().GetBuildingCost(type, level);
        if (cost > m_money)
            return;

        var ground = OLDWorldHolder.Instance().GetGround(x, y);
        if (ground == OLDGroundType.empty)
            return;

        var building = OLDWorldHolder.Instance().GetBuilding(x, y);
        if(building != type && building != OldBuildingType.empty)
        {
            if (building == OldBuildingType.tower0 || building == OldBuildingType.tower1 || building == OldBuildingType.tower2)
                return;
        }

        m_money -= cost;

        OLDWorldHolder.Instance().SetBuilding(type, level, x, y);

        UpdateCorruption(x, y);

        CountBuildings();
    }

    public void RemoveTower(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        var building = OLDWorldHolder.Instance().GetBuilding(x, y);

        if (building != OldBuildingType.tower0 || building != OldBuildingType.tower1 || building != OldBuildingType.tower2)
            return;

        OLDWorldHolder.Instance().SetBuilding(OldBuildingType.empty, 0, x, y);

        UpdateCorruption(x, y);

        CountBuildings();
    }

    public void PlaceGround(int x, int y)
    {
        if (OLDElementHolder.Instance() == null)
            return;

        if (OLDWorldHolder.Instance() == null)
            return;

        int cost = OLDElementHolder.Instance().GetGroundCost(OLDGroundType.normal);
        if (cost > m_money)
            return;

        var groundType = OLDWorldHolder.Instance().GetGround(x, y);
        if (groundType != OLDGroundType.empty)
            return;

        m_money -= cost;

        OLDWorldHolder.Instance().SetGround(GetGroundType(x, y), x, y);

        CountBuildings();

    }

    public void RemoveGround(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        OLDWorldHolder.Instance().SetBuilding(OldBuildingType.empty, 0, x, y);
        OLDWorldHolder.Instance().SetGround(OLDGroundType.empty, x, y);

        CountBuildings();
    }

    public void DestroyBuilding(int x, int y)
    {
        if(OLDWorldHolder.Instance() != null)
            OLDWorldHolder.Instance().SetBuilding(OldBuildingType.empty, 0, x, y);

        CountBuildings();
    }

    OLDGroundType GetGroundType(int x, int y)
    {
        int level = GetCorruptionLevel(x, y);

        if (level <= 0)
            return OLDGroundType.normal;
        if (level == 1)
            return OLDGroundType.low;
        return OLDGroundType.deadly;
    }

    void UpdateCorruption(int x, int y)
    {
        if (OLDElementHolder.Instance() == null)
            return;

        if (OLDWorldHolder.Instance() == null)
            return;

        int currentCorruption = GetCorruptionFromPos(x, y);

        int level = 0;
        var type = OLDWorldHolder.Instance().GetBuilding(x, y, out level);

        int corruption = OLDElementHolder.Instance().GetBuildingCorruption(type, level);

        if (corruption != currentCorruption)
            CreateCorruptionFromPos(x, y, corruption);
    }

    int GetCorruptionLevel(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        int nb = 0;
        foreach(var c in m_corruption)
        {
            if (pos == c.pos)
                nb++;
        }

        return nb;
    }

    int GetCorruptionFromPos(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        int nb = 0;
        foreach (var c in m_corruption)
        {
            if (pos == c.buildingPos)
                nb++;
        }

        return nb;
    }

    void RemoveCorruptionFromPos(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);
        m_corruption.RemoveAll(v => { return v.buildingPos == pos; });
    }

    void CreateCorruptionFromPos(int x, int y, int nb)
    {
        int currentCorruption = GetCorruptionFromPos(x, y);

        if(nb < currentCorruption)
        {
            int currentCount = 0;
            Vector2Int pos = new Vector2Int(x, y);
            for(int i = 0; i < m_corruption.Count; i++)
            {
                if(m_corruption[i].buildingPos == pos)
                {
                    currentCount++;
                    if(currentCount > nb)
                    {
                        m_corruption.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        else if(nb > currentCorruption)
        {
            for(int i = currentCorruption; i < nb; i++)
            {
                var corruptablePos = GetCorruptablePosFrom(x, y);
                if (corruptablePos.Count == 0)
                    break;

                int index = UnityEngine.Random.Range(0, corruptablePos.Count);

                m_corruption.Add(new OLDCorruptionInfo(corruptablePos[index], new Vector2Int(x, y)));
            }
        }

        UpdateCorruptionAround(x, y, 2);
    }

    void UpdateCorruptionAround(int x, int y, int distance)
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        if (OLDElementHolder.Instance() == null)
            return;

        for(int i = -distance; i <= distance; i++)
        {
            for(int j = -distance; j <= distance; j++)
            {
                var currentGround = OLDWorldHolder.Instance().GetGround(x + i, y + j);
                if (currentGround == OLDGroundType.empty)
                    continue;

                var newGround = GetGroundType(x + i, y + j);

                if (currentGround == newGround)
                    continue;

                OLDWorldHolder.Instance().SetGround(newGround, x + i, y + j);

                int level = 0;
                var buildingType = OLDWorldHolder.Instance().GetBuilding(x + i, y + j, out level);

                if(buildingType == OldBuildingType.house || buildingType == OldBuildingType.factory)
                {
                    int newLevel = 1;
                    if (newGround == OLDGroundType.low)
                        newLevel = 0;
                    else if (newGround == OLDGroundType.deadly)
                        newLevel = -1;

                    if (newLevel < 0)
                        OLDWorldHolder.Instance().SetBuilding(OldBuildingType.empty, 0, x + i, y + j);
                    else OLDWorldHolder.Instance().SetBuilding(buildingType, newLevel, x + i, y + j);
                }
            }
        }
    }

    List<Vector2Int> GetCorruptablePosFrom(int x, int y)
    {
        Vector2Int center = new Vector2Int(x, y);
        List<Vector2Int> currentCorruptedPos = new List<Vector2Int>();
        List<Vector2Int> newCorruptedPos = new List<Vector2Int>();

        foreach(var c in m_corruption)
        {
            if (c.buildingPos == center)
                currentCorruptedPos.Add(c.pos);
        }

        for(int i = -2; i <= 2; i++)
        {
            for(int j = -2; j <= 2; j++)
            {
                if (Math.Abs(i) == 2 && Math.Abs(j) == 2)
                    continue; //remove corners

                if (i == 0 && j == 0)
                    continue;

                Vector2Int pos = center + new Vector2Int(i, j);

                if (currentCorruptedPos.Contains(pos))
                    continue;

                newCorruptedPos.Add(pos);
            }
        }

        return newCorruptedPos;
    }
}
