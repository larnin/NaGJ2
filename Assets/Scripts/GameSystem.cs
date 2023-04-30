using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class OneLevelInfo
{
    public int populationPerHouse = 1;
    public int populationPerFactory = 1;
    public float moneyPerHouse = 1;
    public float moneyPerFactory = 1;
}

public class CorruptionInfo
{
    public Vector2Int pos;
    public Vector2Int buildingPos;

    public CorruptionInfo(Vector2Int _pos, Vector2Int _buildingPos)
    {
        pos = _pos;
        buildingPos = _buildingPos;
    }
}

class GameSystem : MonoBehaviour
{
    [SerializeField] float m_growTimer = 5;
    [SerializeField] int m_populationPerLab = 10;
    [SerializeField] List<OneLevelInfo> m_levelInfos;
    [SerializeField] float m_minElemTimer = 30;
    [SerializeField] float m_labWeight = 0.1f;
    [SerializeField] int m_initialMoney = 10;

    List<CorruptionInfo> m_corruption = new List<CorruptionInfo>();

    bool m_initialized = false;

    int m_population = 0;
    int m_workingPopulation = 0;
    float m_moneyGeneration = 0;
    int m_money;
    float m_moneyToAdd; //range[0;1]
    BuildingType m_nextWantedBuilding = BuildingType.empty;

    float m_timer = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    static GameSystem m_instance = null;

    public static GameSystem Instance()
    {
        return m_instance;
    }

    private void Awake()
    {
        m_instance = this;

        m_subscriberList.Add(new Event<GetPopulationAndMoneyEvent>.Subscriber(GetPopulationAndMoney));
        m_subscriberList.Add(new Event<GetNearestBuildingEvent>.Subscriber(GetNearestBuilding));
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
    }   
    
    void SetInitialState()
    {
        if (WorldHolder.Instance() == null)
            return;

        m_initialized = true;

        WorldHolder.Instance().SetGround(GroundType.normal, 0, 0);
        WorldHolder.Instance().SetBuilding(BuildingType.house, 1, 0, 0);
        CountBuildings();
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

        if(m_nextWantedBuilding == BuildingType.empty)
        {
            if (UnityEngine.Random.value < m_labWeight)
                m_nextWantedBuilding = BuildingType.scienceLab;
            else m_nextWantedBuilding = BuildingType.factory;
        }
        if (m_workingPopulation <= 4)
            m_nextWantedBuilding = BuildingType.factory;


        int validIndex = UnityEngine.Random.Range(0, validPos.Count);

        Vector2Int pos = validPos[validIndex];

        var groundType = GetGroundType(pos.x, pos.y);
        if (groundType == GroundType.empty || groundType == GroundType.deadly)
            return;

        int level = 1;
        if (groundType == GroundType.low)
            level = 0;
        else if (groundType == GroundType.hight)
            level = 2;

        int nbWantedWorker = 0;
        if (m_nextWantedBuilding == BuildingType.factory && m_levelInfos.Count > level)
            nbWantedWorker = m_levelInfos[level].populationPerFactory;
        else if (m_nextWantedBuilding == BuildingType.scienceLab)
            nbWantedWorker = m_populationPerLab;

        int nbFreeWorker = m_population - m_workingPopulation;

        if (nbFreeWorker >= nbWantedWorker)
        {
            if (m_nextWantedBuilding == BuildingType.scienceLab)
                level = 0;
            WorldHolder.Instance().SetBuilding(m_nextWantedBuilding, level, pos.x, pos.y);
            m_nextWantedBuilding = BuildingType.empty;
        }
        else WorldHolder.Instance().SetBuilding(BuildingType.house, level, pos.x, pos.y);

        CountBuildings();
    }

    void CountBuildings()
    {
        m_population = 0;
        m_workingPopulation = 0;
        m_moneyGeneration = 0;

        if (WorldHolder.Instance() == null)
            return;

        var bounds = WorldHolder.Instance().GetBounds();

        for(int i = bounds.x; i < bounds.x + bounds.width; i++)
        {
            for(int j = bounds.y; j < bounds.y + bounds.height; j++)
            {
                int level = 0;
                var type = WorldHolder.Instance().GetBuilding(i, j, out level);

                if(type == BuildingType.house)
                {
                    if(m_levelInfos.Count > level)
                    {
                        m_population += m_levelInfos[level].populationPerHouse;
                        m_moneyGeneration += m_levelInfos[level].moneyPerHouse;
                    }
                }
                else if(type == BuildingType.factory)
                {
                    if(m_levelInfos.Count > level)
                    {
                        m_workingPopulation += m_levelInfos[level].populationPerFactory;
                        m_moneyGeneration += m_levelInfos[level].moneyPerFactory; 
                    }
                }   
                else if(type == BuildingType.scienceLab)
                {
                    m_workingPopulation += m_populationPerLab;
                }
            }
        }
    }

    Vector2Int GetNearestBuilding(Vector3 pos, out bool found)
    {
        found = false;

        if (WorldHolder.Instance() == null)
            return Vector2Int.zero;

        Vector2Int currentPos = WorldHolder.Instance().ProjectPos(pos);

        var bounds = WorldHolder.Instance().GetBounds();

        Vector2Int bestPos = Vector2Int.zero;
        float bestDist = -1;

        for (int i = bounds.x; i < bounds.x + bounds.width; i++)
        {
            for (int j = bounds.y; j < bounds.y + bounds.height; j++)
            {
                var type = WorldHolder.Instance().GetBuilding(i, j);
                if(type == BuildingType.factory || type == BuildingType.house || type == BuildingType.scienceLab)
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

    public void PlaceTower(int x, int y, BuildingType type, int level)
    {
        if (ElementHolder.Instance() == null)
            return;

        if (WorldHolder.Instance() == null)
            return;

        int cost = ElementHolder.Instance().GetBuildingCost(type, level);
        if (cost > m_money)
            return;

        var ground = WorldHolder.Instance().GetGround(x, y);
        if (ground == GroundType.empty)
            return;

        var building = WorldHolder.Instance().GetBuilding(x, y);
        if(building != type && building != BuildingType.empty)
        {
            if (building == BuildingType.tower0 || building == BuildingType.tower1 || building == BuildingType.tower2)
                return;
        }

        m_money -= cost;

        WorldHolder.Instance().SetBuilding(type, level, x, y);

        UpdateCorruption(x, y);

        CountBuildings();
    }

    public void RemoveTower(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return;

        var building = WorldHolder.Instance().GetBuilding(x, y);

        if (building != BuildingType.tower0 || building != BuildingType.tower1 || building != BuildingType.tower2)
            return;

        WorldHolder.Instance().SetBuilding(BuildingType.empty, 0, x, y);

        UpdateCorruption(x, y);

        CountBuildings();
    }

    public void PlaceGround(int x, int y)
    {
        if (ElementHolder.Instance() == null)
            return;

        if (WorldHolder.Instance() == null)
            return;

        int cost = ElementHolder.Instance().GetGroundCost(GroundType.normal);
        if (cost > m_money)
            return;

        var groundType = WorldHolder.Instance().GetGround(x, y);
        if (groundType != GroundType.empty)
            return;

        m_money -= cost;

        WorldHolder.Instance().SetGround(GetGroundType(x, y), x, y);

    }

    public void RemoveGround(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return;

        WorldHolder.Instance().SetBuilding(BuildingType.empty, 0, x, y);
        WorldHolder.Instance().SetGround(GroundType.empty, x, y);

        CountBuildings();
    }

    public void DestroyBuilding(int x, int y)
    {
        if(WorldHolder.Instance() != null)
            WorldHolder.Instance().SetBuilding(BuildingType.empty, 0, x, y);
    }

    GroundType GetGroundType(int x, int y)
    {
        int level = GetCorruptionLevel(x, y);

        if (level <= 0)
            return GroundType.normal;
        if (level == 1)
            return GroundType.low;
        return GroundType.deadly;
    }

    void UpdateCorruption(int x, int y)
    {
        if (ElementHolder.Instance() == null)
            return;

        if (WorldHolder.Instance() == null)
            return;

        int currentCorruption = GetCorruptionFromPos(x, y);

        int level = 0;
        var type = WorldHolder.Instance().GetBuilding(x, y, out level);

        int corruption = ElementHolder.Instance().GetBuildingCorruption(type, level);

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

                m_corruption.Add(new CorruptionInfo(corruptablePos[index], new Vector2Int(x, y)));
            }
        }

        UpdateCorruptionAround(x, y, 2);
    }

    void UpdateCorruptionAround(int x, int y, int distance)
    {
        if (WorldHolder.Instance() == null)
            return;

        if (ElementHolder.Instance() == null)
            return;

        for(int i = -distance; i <= distance; i++)
        {
            for(int j = -distance; j <= distance; j++)
            {
                var currentGround = WorldHolder.Instance().GetGround(x + i, y + j);
                if (currentGround == GroundType.empty)
                    continue;

                var newGround = GetGroundType(x + i, y + j);

                if (currentGround == newGround)
                    continue;

                WorldHolder.Instance().SetGround(newGround, x + i, y + j);

                int level = 0;
                var buildingType = WorldHolder.Instance().GetBuilding(x + i, y + j, out level);

                if(buildingType == BuildingType.house || buildingType == BuildingType.factory)
                {
                    int newLevel = 1;
                    if (newGround == GroundType.low)
                        newLevel = 0;
                    else if (newGround == GroundType.deadly)
                        newLevel = -1;

                    if (newLevel < 0)
                        WorldHolder.Instance().SetBuilding(BuildingType.empty, 0, x + i, y + j);
                    else WorldHolder.Instance().SetBuilding(buildingType, newLevel, x + i, y + j);
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
