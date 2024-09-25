using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GamemodeWaves : GamemodeBase
{
    public class GamemodeWavesInfos
    {
        public List<GameEntityWave> waves = new List<GameEntityWave>();
        public Vector3Int spawnPoint = Vector3Int.zero;
        public float spawnPointRot = 0;
        public float spawnRadius = 1;
        public int startIndex = 0;
    }

    enum WaveState
    {
        starting,
        spawning,
        delay,
    }

    List<GamemodeWavesInfos> m_points = new List<GamemodeWavesInfos>();
    float m_delayBeforeFirstWave = 0;
    public float delayBeforeFirstWave { get { return m_delayBeforeFirstWave; } set { m_delayBeforeFirstWave = value; } }
    float m_delayBetweenWaves = 0;
    public float delayBetweenWaves { get { return m_delayBetweenWaves; } set { m_delayBetweenWaves = value; } }
    Team m_team = Team.ennemy1;
    public Team team { get { return m_team; } set { m_team = value; } }

    float m_timer = 0;
    int m_currentWave = 0;
    WaveState m_waveState = WaveState.starting;

    List<int> m_spawnedEntities = new List<int>();

    public GamemodeWaves(GameLevel level) : base(level)
    {
        m_type = GamemodeType.waves;
    }

    public override void Process(float deltaTime)
    {
        ProcessWaves(deltaTime);
        ProcessEntities();
    }

    void ProcessWaves(float deltaTime)
    { 
        if (m_status != GamemodeStatus.playing)
            return;

        float previousTime = m_timer;
        m_timer += deltaTime;

        switch(m_waveState)
        {
            case WaveState.starting:
                if(m_timer >= m_delayBeforeFirstWave)
                {
                    m_timer = 0;
                    m_currentWave = 0;
                    m_waveState = WaveState.spawning;
                }
                break;
            case WaveState.spawning:
                bool goNext = true;
                for(int i = 0; i < m_points.Count; i++)
                {
                    var point = m_points[i];
                    if (point.startIndex > m_currentWave)
                        continue;

                    int waveIndex = m_currentWave - point.startIndex;
                    if (waveIndex >= point.waves.Count)
                        continue;

                    foreach(var group in point.waves[waveIndex].groups)
                    {
                        int oldCount = group.GetNbSpawn(previousTime);
                        int newCount = group.GetNbSpawn(m_timer);

                        for(int j = oldCount; j < newCount; j++)
                            SpawnEntity(group.type, i);

                        if (newCount < group.count)
                            goNext = false;
                    }
                }

                if(goNext)
                {
                    m_timer = 0;
                    m_waveState = WaveState.delay;
                    if (IsEnded(m_currentWave + 1))
                    {
                        m_currentWave++;
                        m_status = GamemodeStatus.completed;
                    }
                }
                break;
            case WaveState.delay:
                if(m_timer >= m_delayBetweenWaves)
                {
                    m_timer = 0;
                    m_currentWave++;

                    if (IsEnded(m_currentWave))
                        m_status = GamemodeStatus.completed;
                    else m_waveState = WaveState.spawning;
                }
                break;
        }
    }

    void ProcessEntities()
    {
        List<int> nextEntities = new List<int>();
        foreach(var e in m_spawnedEntities)
        {
            var entity = m_level.entityList.GetEntity(e);
            if (entity == null)
                continue;

            nextEntities.Add(e);

            if(!entity.GetPath().GetTarget().IsValid())
            {
                var pos = entity.GetPos();
                var posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.RoundToInt(pos.z));

                var target = m_level.buildingList.GetNearestTargetBuilding(posInt, BuildingType.OperationCenter, entity.GetTeam());
                if (target != null)
                    entity.GetPath().SetTarget(GameTarget.FromBuilding(m_level, target.GetID()));
            }
        }

        m_spawnedEntities = nextEntities;
    }

    bool IsEnded(int wave)
    {
        foreach(var point in m_points)
        {
            int lastWave = point.waves.Count + point.startIndex;
            if (lastWave >= wave)
                return false;
        }

        return true;
    }

    public override void Reset()
    {
        m_points.Clear();
    }

    public override void Start()
    {
        m_timer = 0;
        m_currentWave = 0;

        m_status = GamemodeStatus.playing;
        m_waveState = WaveState.starting;
    }

    protected override void Load(JsonObject obj)
    {
        base.Load(obj);

        m_delayBeforeFirstWave = obj.GetElement("DelayBeforeFirst")?.Float() ?? 0;
        m_delayBetweenWaves = obj.GetElement("DelayBetween")?.Float() ?? 0;

        string teamStr = obj.GetElement("Team")?.String();
        if(teamStr != null)
        {
            if (!Enum.TryParse(teamStr, out m_team))
                m_team = Team.ennemy1;
        }

        var dataArray = obj.GetElement("Points")?.JsonArray();
        if (dataArray != null)
        {
            foreach (var elem in dataArray)
            {
                var pointObject = elem.JsonObject();
                if (pointObject != null)
                {
                    GamemodeWavesInfos point = new GamemodeWavesInfos();
                    point.spawnPoint = Json.ToVector3Int(pointObject.GetElement("Pos")?.JsonArray());
                    point.spawnPointRot = pointObject.GetElement("Rot")?.Float() ?? 0;
                    point.spawnRadius = pointObject.GetElement("Radius")?.Float() ?? 0;
                    point.startIndex = pointObject.GetElement("Start")?.Int() ?? 0;

                    var wavesArray = pointObject.GetElement("Waves")?.JsonArray();
                    if(wavesArray != null)
                    {
                        foreach (var waveElem in wavesArray)
                        {
                            var waveObject = waveElem.JsonObject();
                            if (waveObject != null)
                            {
                                GameEntityWave wave = new GameEntityWave();
                                wave.Load(waveObject);
                                point.waves.Add(wave);
                            }
                        }
                    }

                    m_points.Add(point);
                }
            }
        }

        var entitiesArray = obj.GetElement("Entities")?.JsonArray();
        if(entitiesArray != null)
        {
            foreach(var elem in entitiesArray)
            {
                if (elem.IsJsonNumber())
                    m_spawnedEntities.Add(elem.Int());
            }
        }

        m_timer = obj.GetElement("Timer")?.Float() ?? 0;
        m_currentWave = obj.GetElement("Wave")?.Int() ?? 0;

        string stateStr = obj.GetElement("WaveState")?.String();
        if (stateStr == null)
            m_waveState = WaveState.starting;
        else
        {
            WaveState waveState;
            if (Enum.TryParse(stateStr, out waveState))
                m_waveState = waveState;
            else m_waveState = WaveState.starting;
        }
    }

    public override void Save(JsonObject obj)
    {
        base.Save(obj);

        obj.AddElement("DelayBeforeFirst", m_delayBeforeFirstWave);
        obj.AddElement("DelayBetween", m_delayBetweenWaves);
        obj.AddElement("Team", m_team.ToString());

        var dataArray = new JsonArray();
        obj.AddElement("Points", dataArray);

        foreach(var point in m_points)
        {
            var pointObject = new JsonObject();
            dataArray.Add(pointObject);

            pointObject.AddElement("Pos", Json.FromVector3Int(point.spawnPoint));
            pointObject.AddElement("Rot", point.spawnPointRot);
            pointObject.AddElement("Radius", point.spawnRadius);
            pointObject.AddElement("Start", point.startIndex);

            var wavesArray = new JsonArray();
            pointObject.AddElement("Waves", wavesArray);

            foreach(var w in point.waves)
            {
                var waveObject = new JsonObject();
                wavesArray.Add(waveObject);
                w.Save(waveObject);
            }
        }

        var entitiesArray = new JsonArray();
        obj.AddElement("Entities", entitiesArray);

        foreach (var e in m_spawnedEntities)
            entitiesArray.Add(e);

        obj.AddElement("Timer", m_timer);
        obj.AddElement("Wave", m_currentWave);
        obj.AddElement("WaveState", m_waveState.ToString());
    }

    public override EditorGamemodeViewBase CreateEditorView()
    {
        return new EditorGamemodeViewWaves(this);
    }

    public int GetPointNb()
    {
        return m_points.Count();
    }

    public GamemodeWavesInfos GetPointFromIndex(int index)
    {
        if (index < 0 || index >= m_points.Count())
            return null;

        return m_points[index];
    }

    public void AddPoint(GamemodeWavesInfos point)
    {
        m_points.Add(point);
    }

    public void RemovePoint(GamemodeWavesInfos point)
    {
        m_points.Remove(point);
    }

    public void RemovePointAt(int index)
    {
        m_points.RemoveAt(index);
    }

    void SpawnEntity(EntityType type, int point)
    {
        var spawnPos = GetSpawnPos(point);

        var entity = new GameEntity(type, m_team, spawnPos, m_level);

        m_level.entityList.Add(entity);
        m_spawnedEntities.Add(entity.GetID());

        var spawnPosInt = new Vector3Int(Mathf.RoundToInt(spawnPos.x), Mathf.FloorToInt(spawnPos.y), Mathf.RoundToInt(spawnPos.z));

        var target = m_level.buildingList.GetNearestTargetBuilding(spawnPosInt, BuildingType.OperationCenter, entity.GetTeam());
        if(target != null)
            entity.GetPath().SetTarget(GameTarget.FromBuilding(m_level, target.GetID()));
    }

    Vector3 GetSpawnPos(int point)
    {
        if (point < 0 || point >= m_points.Count)
            return Vector3.zero;

        return m_points[point].spawnPoint;
    }
}
