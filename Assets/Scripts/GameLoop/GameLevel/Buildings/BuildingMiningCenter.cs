using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class BuildingMiningCenter : BuildingBase
{
    class DrillInfo
    {
        public int drillID;
        public BlockType block;
    }

    List<DrillInfo> m_drillsID = new List<DrillInfo>();

    float m_timer = 0;

    public BuildingMiningCenter(BuildingInfos infos, GameLevel level) : base(infos, level)
    {

    }

    public override void Load(JsonObject obj)
    {
        base.Load(obj);

        m_drillsID.Clear();

        var array = obj.GetElement("Drills")?.JsonArray();
        if(array != null)
        {
            foreach(var elt in array)
            {
                if (!elt.IsJsonNumber())
                    continue;

                int value = elt.Int();
                if (value > 0)
                {
                    DrillInfo d = new DrillInfo();
                    d.drillID = value;
                    m_drillsID.Add(d);
                }
            }
        }
    }

    public override void Save(JsonObject obj)
    {
        base.Save(obj);

        var array = new JsonArray();
        obj.AddElement("Drills", array);

        foreach (var d in m_drillsID)
            array.Add(d.drillID);
    }

    public override void Start()
    {
        base.Start();

        for(int i = 0; i < m_drillsID.Count; i++)
        {
            var d = m_drillsID[i];
            var b = m_level.buildingList.GetBuilding(d.drillID);
            if(b != null)
                d.block = m_level.grid.GetBlockID(b.GetInfos().pos);
            else
            {
                m_drillsID.RemoveAt(i);
                i--;
            }
        }

        var points = GetDrillsPos();

        foreach (var p in points)
        {
            Vector3Int pos = new Vector3Int(p.x, m_infos.pos.y, p.y);
            TryCreateDrillAt(pos);
        }
    }

    List<Vector2Int> GetDrillsPos()
    {
        int distance = 0;
        if (m_infos.buildingType == BuildingType.SmallMiningCenter)
            distance = Global.instance.allBuildings.smallMiningCenter.mineDistance;
        else if (m_infos.buildingType == BuildingType.MediumMiningCenter)
            distance = Global.instance.allBuildings.mediumMiningCenter.mineDistance;
        else if (m_infos.buildingType == BuildingType.BigMiningCenter)
            distance = Global.instance.allBuildings.bigMiningCenter.mineDistance;

        var bounds = BuildingDataEx.GetBuildingBounds(m_infos.buildingType, m_infos.pos, m_infos.rotation, m_infos.level);
        var boundsPos = bounds.position;
        var boundsSize = bounds.size;
        return GetDrillsPos(new RectInt(new Vector2Int(boundsPos.x, boundsPos.z), new Vector2Int(boundsSize.x, boundsSize.z)), distance);
    }

    List<Vector2Int> GetDrillsPos(RectInt bounds, int level)
    {
        List<Vector2Int> drills = new List<Vector2Int>();

        Vector2Int size = bounds.size;
        Vector2Int min = bounds.min;


        for(int i = -3; i < size.x + 3; i++)
        {
            for(int j = -3; j < size.y + 3; j++)
            {
                if (i > 0 && j > 0 && i < size.x && j < size.y)
                    continue;

                int x = i;
                if (x > 0 && x < size.x)
                    x = 0;
                else if (x >= size.x)
                    x -= size.x - 1;

                int y = j;
                if (j > 0 && j < size.y)
                    j = 0;
                else if (j >= size.y)
                    j -= size.y - 1;

                float diag = Mathf.Abs(x) + Mathf.Abs(y);

                if (level >= 1 && diag == 1)
                    drills.Add(new Vector2Int(i, j));
                if(diag == 2)
                {
                    if (level >= 2 && x != 0 && y != 0)
                        drills.Add(new Vector2Int(i, j));
                    if (level >= 3 && (x == 0 || y == 0))
                        drills.Add(new Vector2Int(i, j));
                }
                if (diag == 3 && x != 0 && y != 0 && level >= 4)
                    drills.Add(new Vector2Int(i, j));
                if (Mathf.Abs(x) == 2 && Mathf.Abs(y) == 2 && level >= 5)
                    drills.Add(new Vector2Int(i, j));
                if (Mathf.Abs(x) == 3 && Mathf.Abs(y) < 2 && level >= 6)
                    drills.Add(new Vector2Int(i, j));
                if (Mathf.Abs(y) == 3 && Mathf.Abs(x) < 2 && level >= 6)
                    drills.Add(new Vector2Int(i, j));
                if (diag == 5 && Mathf.Abs(x) <= 3 && Mathf.Abs(y) <= 3 && level >= 7)
                    drills.Add(new Vector2Int(i, j));
            }
        }

        for (int i = 0; i < drills.Count; i++)
            drills[i] = drills[i] + min;

        return drills;
    }

    BuildingInfos GenerateDrill(Vector3Int pos)
    {
        BuildingInfos drill = new BuildingInfos();

        drill.buildingType = BuildingType.Drill;
        drill.level = m_infos.level;
        drill.pos = pos;
        drill.rotation = RotationEx.RandomRotation();
        drill.team = m_infos.team;

        return drill;
    }

    public override void Destroy(bool fromReset)
    {
        base.Destroy(fromReset);

        if (fromReset)
            return;

        List<Vector3Int> removedDrills = new List<Vector3Int>();

        foreach(var d in m_drillsID)
        {
            var drill = m_level.buildingList.GetBuilding(d.drillID);
            if (drill == null)
                continue;

            removedDrills.Add(drill.GetInfos().pos);

            m_level.buildingList.Remove(d.drillID);
        }

        int nbBuildings = m_level.buildingList.GetBuildingNb();
        foreach(var p in removedDrills)
        {
            for (int i = 0; i < nbBuildings; i++)
            {
                var b = m_level.buildingList.GetBuildingFromIndex(i);

                if (b == this)
                    continue;

                var miningCenter = b as BuildingMiningCenter;
                if (miningCenter == null)
                    continue;

                if (miningCenter.RetrieveDrill(p))
                    break;
            }
        }
    }

    public bool RetrieveDrill(Vector3Int pos)
    {
        var points = GetDrillsPos();

        foreach (var p in points)
        {
            Vector3Int validPos = new Vector3Int(p.x, m_infos.pos.y, p.y);

            if (validPos != pos)
                continue;

            return TryCreateDrillAt(pos);
        }

        return false;
    }

    bool TryCreateDrillAt(Vector3Int pos)
    {
        if (m_level.buildingList.GetBuildingAt(pos) != null)
            return false;

        var b = m_level.grid.GetBlock(pos);
        if (!BlockDataEx.CanDrill(b.id))
            return false;

        var drill = BuildingBase.Create(GenerateDrill(pos), m_level);
        m_level.buildingList.Add(drill);

        var infos = new DrillInfo();
        infos.drillID = drill.GetID();
        infos.block = b.id;

        m_drillsID.Add(infos);

        return true;
    }

    public override void Process(float deltaTime)
    {
        base.Process(deltaTime);

        if (m_drillsID.Count == 0)
            return;

        float efficiency = 1;
        if (m_infos.buildingType == BuildingType.SmallMiningCenter)
            efficiency = Global.instance.allBuildings.smallMiningCenter.efficiency;
        else if (m_infos.buildingType == BuildingType.MediumMiningCenter)
            efficiency = Global.instance.allBuildings.mediumMiningCenter.efficiency;
        else if (m_infos.buildingType == BuildingType.BigMiningCenter)
            efficiency = Global.instance.allBuildings.bigMiningCenter.efficiency;

        float speed = 0;
        bool allSames = true;
        foreach(var d in m_drillsID)
        {
            var collect = BlockDataEx.GetCollectData(d.block);
            if (collect != null)
                speed += collect.speed;
            if (d.block != m_drillsID[0].block)
                allSames = false;
        }

        m_timer += deltaTime * speed * efficiency;

        if (allSames)
        {
            while(m_timer >= 1)
            {
                m_timer--;
                AddResourceFromBlock(m_drillsID[0].block);
            }
        }
        else
        {
            var distrib = new UniformFloatDistribution(0, speed);
            var rand = new StaticRandomGenerator<MT19937>();

            while (m_timer >= 1)
            {
                m_timer--;

                float weight = distrib.Next(rand);

                foreach (var d in m_drillsID)
                {
                    var collect = BlockDataEx.GetCollectData(d.block);
                    if (collect != null)
                        weight -= collect.speed;

                    if (weight <= 0)
                    {
                        AddResourceFromBlock(d.block);
                        break;
                    }
                }
            }
        }
    }
    
    void AddResourceFromBlock(BlockType type)
    {
        var collect = BlockDataEx.GetCollectData(type);
        if (collect == null)
            return;

        m_level.beltSystem.ContainerAddResource(collect.resource, 1, m_ID, 0);
    }
}
