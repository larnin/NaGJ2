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
        public Vector3Int spawnPoint;
        public float spawnRadius;
        public int startIndex;
    }

    List<GamemodeWavesInfos> m_points = new List<GamemodeWavesInfos>();

    public GamemodeWaves(GameLevel level) : base(level)
    {
        m_type = GamemodeType.waves;
    }

    public override void Process(float deltaTime)
    {

    }

    public override void Reset()
    {
        m_points.Clear();
    }

    public override void Start()
    {
        
    }

    protected override void Load(JsonObject obj)
    {
        base.Load(obj);

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
    }

    public override void Save(JsonObject obj)
    {
        base.Save(obj);

        var dataArray = new JsonArray();
        obj.AddElement("Points", dataArray);

        foreach(var point in m_points)
        {
            var pointObject = new JsonObject();
            dataArray.Add(pointObject);

            pointObject.AddElement("Pos", Json.FromVector3Int(point.spawnPoint));
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
    }
}
