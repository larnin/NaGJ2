using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public enum EnemyType
{
    normal,
    fast,
    heavy,
    kamikaze,
    sniper,
}

[Serializable]
public class EnemyData
{
    public EnemyType type;
    public GameObject prefab;
}

public class WaveSystem : MonoBehaviour
{
    [SerializeField] List<EnemyData> m_enemies = new List<EnemyData>();
    [SerializeField] GameObject m_spawnerPrefab;
    [SerializeField] float m_initialDelay = 120;
    [SerializeField] float m_waveDelay = 60;
    [SerializeField] int m_initialEnemyCount = 2;
    [SerializeField] int m_moreEnemyEachWave = 1;
    [SerializeField] int m_waveToMoreType = 10;
    [SerializeField] int m_maxTypeNb = 3;
    [SerializeField] float m_powerMultiplier = 1.3f;
    [SerializeField] float m_durationBetweenSpawn = 1;
    [SerializeField] int m_spawnDistance = 5;

    SubscriberList m_subscriberList = new SubscriberList();

    int m_currentWave = 0;
    float m_timer = 0;
    float m_initialTimer = 0;
    List<GameObject> m_spawnPos = new List<GameObject>();
    List<int> m_remainingEnemies = new List<int>();
    int m_spawnedEnemies = 0;
    Status m_status = Status.Cooldown;

    List<int> m_nextWave = new List<int>();

    enum Status
    {
        waitingSpawner,
        Spawning,
        DestroyingSpawner,
        Cooldown,
    }

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetWaveTextEvent>.Subscriber(GetWaveText));
        m_subscriberList.Add(new Event<GetMaxPopulationMoneyWaveEvent>.Subscriber(GetMax));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void GetWaveText(GetWaveTextEvent e)
    {
        e.label = "";
        e.description = "";

        if(m_status == Status.waitingSpawner || m_status == Status.Spawning)
        {
            e.label = "Spawning ...";
            e.description = SetDescriptionFromWaves(m_remainingEnemies);
        }
        else
        {
            e.label = "Prochaine vague : ";
            e.label += TimerToText(m_timer);
            e.description = SetDescriptionFromWaves(m_nextWave);
        }
    }

    string SetDescriptionFromWaves(List<int> wave)
    {
        string description = "";

        for(int i = 0; i < wave.Count; i++)
        {
            int w = wave[i];
            if (w == 0)
                continue;

            description += GetName((EnemyType)i);
            description += " : ";
            description += w.ToString();
            description += " - ";
        }

        if (description.Length > 3)
            description = description.Remove(description.Length - 3);

        return description;
    }

    void GetMax(GetMaxPopulationMoneyWaveEvent e)
    {
        e.wave = m_currentWave;
    }

    private void Start()
    {
        int nbType = Enum.GetValues(typeof(EnemyType)).Length;
        for(int i = 0; i < nbType; i++)
        {
            m_remainingEnemies.Add(0);
            m_nextWave.Add(0);
        }

        m_timer = m_initialDelay;
        m_initialTimer = m_initialDelay;
        GenerateNextWave();
    }

    private void Update()
    {
        if (Gamestate.instance.paused)
            return;

        switch(m_status)
        {
            case Status.Cooldown:
                {
                    m_timer -= Time.deltaTime;
                    
                    if (m_timer <= 0)
                        StartNextWave();
                    break;
                }
            case Status.waitingSpawner:
                {
                    if (WaitSpawners())
                        m_status = Status.Spawning;
                    break;
                }
            case Status.Spawning:
                {
                    m_timer -= Time.deltaTime;
                    int nbSpawn = Mathf.CeilToInt((m_initialTimer - m_timer) / m_durationBetweenSpawn);
                    if (nbSpawn > m_spawnedEnemies)
                        SpawnOneEnemy();
                    break;
                }
            case Status.DestroyingSpawner:
                {
                    m_timer -= Time.deltaTime;
                    if (DestroySpawners())
                        m_status = Status.Cooldown;
                    break;
                }
        }
    }

    EnemyData GetEnemy(EnemyType type)
    {
        foreach (var e in m_enemies)
            if (e.type == type)
                return e;

        return null;
    }

    string GetName(EnemyType type)
    {
        switch(type)
        {
            case EnemyType.fast:
                return "Rapide";
            case EnemyType.heavy:
                return "Lourd";
            case EnemyType.kamikaze:
                return "Kamikaze";
            case EnemyType.normal:
                return "Normal";
            case EnemyType.sniper:
                return "Sniper";
        }

        return "";
    }

    string TimerToText(float timer)
    {
        int time = Mathf.FloorToInt(timer);

        int min = time / 60;
        int sec = time % 60;

        string str = "";
        str += min.ToString();
        str += ":";
        if (sec < 10)
            str += "0";
        str += sec.ToString();
        return str;
    }

    void StartNextWave()
    {
        int nbEnemies = 0;
        for (int i = 0; i < m_remainingEnemies.Count; i++)
        {
            m_remainingEnemies[i] = m_nextWave[i];
            nbEnemies += m_remainingEnemies[i];
        }

        float minDuration = nbEnemies * m_durationBetweenSpawn;
        if (minDuration > m_waveDelay)
            m_timer = minDuration;
        else m_timer = m_waveDelay;
        m_initialTimer = m_timer;
        m_spawnedEnemies = 0;

        GenerateSpawnPos();

        m_status = Status.waitingSpawner;

        m_currentWave++;

        GenerateNextWave();
    }

    void GenerateNextWave()
    {
        for (int i = 0; i < m_nextWave.Count; i++)
            m_nextWave[i] = 0;

        int nbType = GetNbType();
        int nbEnemy = m_initialEnemyCount + m_moreEnemyEachWave * m_currentWave;

        int nbMean = nbEnemy / nbType;
        int nbMin = Mathf.RoundToInt(nbMean * 0.8f);
        int nbMax = Mathf.RoundToInt(nbMean * 1.2f);

        int remaining = nbEnemy;

        for(int i = 0; i < nbType; i++)
        {
            int index = UnityEngine.Random.Range(0, m_nextWave.Count);
            if (m_currentWave <= 1)
                index = (int)EnemyType.normal;
            if (i == nbType - 1)
                m_nextWave[index] += remaining;
            else
            {
                int count = UnityEngine.Random.Range(nbMin, nbMax);
                m_nextWave[index] += count;
                remaining -= count;
            }
        }
    }

    bool WaitSpawners()
    {
        foreach(var s in m_spawnPos)
        {
            var status = new SpawnerGetStatusEvent();
            Event<SpawnerGetStatusEvent>.Broadcast(status, s);
            if (!status.canSpawn)
                return false;
        }

        return true;
    }

    void StartDestroySpawner()
    {
        m_status = Status.DestroyingSpawner;

        foreach(var s in m_spawnPos)
        {
            Event<SpawnerStopEvent>.Broadcast(new SpawnerStopEvent(), s);
        }

    }

    bool DestroySpawners()
    {
        foreach (var s in m_spawnPos)
        {
            var status = new SpawnerGetStatusEvent();
            Event<SpawnerGetStatusEvent>.Broadcast(status, s);
            if (!status.stopped)
                return false;
        }

        foreach(var s in m_spawnPos)
        {
            Destroy(s);
        }

        m_spawnPos.Clear();

        return true;
    }

    void SpawnOneEnemy()
    {
        int nbRemaining = 0;
        foreach (var e in m_remainingEnemies)
            nbRemaining += e;
        if(nbRemaining <= 0)
        {
            StartDestroySpawner();
            return;
        }

        float multiplier = Mathf.Pow(m_powerMultiplier, m_currentWave - 1);

        if (m_spawnPos.Count() == 0)
            return;

        int spawnIndex = m_spawnedEnemies % m_spawnPos.Count();

        int nbEnemies = 0;
        foreach (var e in m_remainingEnemies)
            nbEnemies += e;
        StaticRandomGenerator<MT19937> rand = new StaticRandomGenerator<MT19937>();
        UniformIntDistribution d = new UniformIntDistribution(0, nbEnemies);
        int currentEnemy = d.Next(rand);
        EnemyType type = EnemyType.normal;
        for (int i = 0; i < m_remainingEnemies.Count; i++)
        {
            currentEnemy -= m_remainingEnemies[i];
            if (i > 0)
            {
                m_remainingEnemies[i]--;
                type = (EnemyType)i;
                break;
            }
        }

        m_spawnedEnemies++;

        EnemyData data = GetEnemy(type);
        if (data == null)
            return;

        Event<SpawnEntityEvent>.Broadcast(new SpawnEntityEvent(data.prefab, multiplier), m_spawnPos[spawnIndex]);
    }

    void GenerateSpawnPos()
    {
        if (WorldHolder.Instance() == null)
            return;

        m_spawnPos.Clear();

        int nbPos = GetNbType();

        var bounds = WorldHolder.Instance().GetBounds();
        bounds.x -= m_spawnDistance;
        bounds.width += m_spawnDistance * 2;
        bounds.y -= m_spawnDistance;
        bounds.height += m_spawnDistance * 2;
        int boundsLen = 2 * (bounds.width + bounds.height);

        for(int i = 0; i < nbPos; i++)
        {
            int x = 0;
            int y = 0;

            int value = UnityEngine.Random.Range(0, boundsLen);
            if(value < 2 * bounds.width)
            {
                if(value < bounds.width)
                {
                    x = value;
                    y = bounds.y;
                }
                else
                {
                    x = value - bounds.width;
                    y = bounds.y + bounds.height;
                }
            }
            else
            {
                value -= 2 * bounds.width;
                if(value < bounds.height)
                {
                    x = bounds.x;
                    y = value;
                }
                else
                {
                    x = bounds.x + bounds.width;
                    y = value - bounds.height;
                }
            }

            Vector3 pos = WorldHolder.Instance().GetElemPos(x, y);
            pos.y += 1.2f;

            var spawner = Instantiate(m_spawnerPrefab);
            spawner.transform.position = pos;

            m_spawnPos.Add(spawner);
        }
    }

    int GetNbType()
    {
        int nbType = 1 + m_currentWave / m_waveToMoreType;
        if (nbType > m_maxTypeNb)
            return m_maxTypeNb;
        return nbType;
    }
}