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
        public GameEntityWave wave;
        public Vector3Int spawnPoint;
        public float spawnRadius;
    }

    List<GamemodeWavesInfos> m_waves = new List<GamemodeWavesInfos>();

    public GamemodeWaves(GameLevel level) : base(level)
    {
        m_type = GamemodeType.waves;
    }

    public override void Process(float deltaTime)
    {

    }

    public override void Reset()
    {
        
    }

    public override void Start()
    {
        
    }

    protected override void Load(JsonObject obj)
    {
        base.Load(obj);
    }

    public override void Save(JsonObject obj)
    {
        base.Save(obj);
    }
}
