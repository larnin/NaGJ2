using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingInfos
{
    public BuildingType buildingType;
    public BeltDirection beltDirection;
    public int level;
    public Vector3Int pos;
    public Rotation rotation;
    public Team team;
}

public class BuildingBase
{
    protected int m_ID;
    protected BuildingInfos m_infos;
    protected GameLevel m_level;

    public BuildingBase(BuildingInfos infos, GameLevel level)
    {
        m_infos = infos;
        m_level = level;
    }

    public virtual void Start() { }
    public virtual void Destroy(bool fromReset) { }
    public virtual void Process(float deltaTime) { }

    public virtual void Load(JsonObject obj) 
    {
        m_ID = obj.GetElement("ID")?.Int() ?? 0;
    }

    public virtual void Save(JsonObject obj)
    {
        obj.AddElement("Type", m_infos.buildingType.ToString());
        obj.AddElement("ID", m_ID);
        obj.AddElement("Level", m_infos.level);
        obj.AddElement("Pos", Json.FromVector3Int(m_infos.pos));
        obj.AddElement("Rot", m_infos.rotation.ToString());
        obj.AddElement("Team", m_infos.team.ToString());
    }

    public static BuildingBase Create(JsonObject obj, GameLevel level)
    {
        BuildingInfos infos = new BuildingInfos();

        string str = obj.GetElement("Type")?.String();
        if (str == null)
            return null;
        if (!Enum.TryParse(str, out infos.buildingType))
            return null;

        infos.level = obj.GetElement("Level")?.Int() ?? 0;
        infos.pos = Json.ToVector3Int(obj.GetElement("Pos")?.JsonArray(), Vector3Int.zero);
        str = obj.GetElement("Rot")?.String();
        if (str == null)
            return null;
        Enum.TryParse(str, out infos.rotation);
        str = obj.GetElement("Team")?.String();
        if (str == null)
            return null;
        Enum.TryParse(str, out infos.team);

        var building = Create(infos, level);
        building.Load(obj);

        return building;
    }

    public static BuildingBase Create(BuildingInfos infos, GameLevel level)
    {
        switch(infos.buildingType)
        {
            case BuildingType.SmallMiningCenter:
            case BuildingType.MediumMiningCenter:
            case BuildingType.BigMiningCenter:
                return new BuildingMiningCenter(infos, level);
            default:
                return new BuildingBase(infos, level);
        }
    }

    public BuildingInfos GetInfos()
    {
        return m_infos;
    }

    public void SetID(int id)
    {
        m_ID = id;
    }

    public int GetID()
    {
        return m_ID;
    }
}
