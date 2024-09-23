using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GameTargetType
{
    position,
    entity,
    building,
    invalid,
}

public class GameTarget
{
    GameTargetType m_targetType;
    int m_ID;
    Vector3 m_pos;
    GameLevel m_level;

    private GameTarget()
    {
        m_targetType = GameTargetType.invalid;
        m_pos = Vector3.zero;
        m_ID = 0;
        m_level = null;
    }

    private GameTarget(Vector3 pos)
    {
        m_targetType = GameTargetType.position;
        m_pos = pos;
        m_ID = 0;
        m_level = null;
    }

    private GameTarget(GameLevel level, GameTargetType type, int id) 
    {
        m_targetType = type;
        m_ID = id;
        m_pos = Vector3.zero;
        m_level = level;
    }

    public static GameTarget Invalid()
    {
        return new GameTarget();
    }

    public static GameTarget FromPos(Vector3 pos)
    {
        return new GameTarget(pos);
    }

    public static GameTarget FromEntity(GameLevel level, int id)
    {
        return new GameTarget(level, GameTargetType.entity, id);
    }

    public static GameTarget FromBuilding(GameLevel level, int id)
    {
        return new GameTarget(level, GameTargetType.building, id);
    }

    public bool IsValid()
    {
        if (m_targetType == GameTargetType.invalid)
            return false;

        if (m_targetType == GameTargetType.position)
            return true;

        if (m_level == null)
            return false;

        if(m_targetType == GameTargetType.entity)
            return m_level.entityList.GetEntity(m_ID) != null;

        if (m_targetType == GameTargetType.building)
            return m_level.buildingList.GetBuilding(m_ID) != null;

        return false;
    }

    public Vector3 GetTargetPos()
    {
        if (!IsValid())
            return Vector3.zero;

        if (m_targetType == GameTargetType.position)
            return m_pos;

        if (m_targetType == GameTargetType.entity)
            return m_level.entityList.GetEntity(m_ID).GetPos();

        if (m_targetType == GameTargetType.building)
            return m_level.buildingList.GetBuilding(m_ID).GetInfos().pos;

        return Vector3.zero;
    }

    public bool IsPos()
    {
        return m_targetType == GameTargetType.position;
    }

    public bool IsBuilding()
    {
        return m_targetType == GameTargetType.building;
    }

    public BuildingBase GetBuilding()
    {
        if (m_level == null)
            return null;

        return m_level.buildingList.GetBuilding(m_ID);
    }

    public bool IsEntity()
    {
        return m_targetType == GameTargetType.entity;
    }

    public GameEntity GetEntity()
    {
        if (m_level == null)
            return null;

        return m_level.entityList.GetEntity(m_ID);
    }

    public void Load(JsonObject obj, GameLevel level)
    {
        string typeStr = obj.GetElement("Type")?.String();
        if(typeStr != null)
        {
            GameTargetType targetType;
            if (Enum.TryParse(typeStr, out targetType))
            {
                m_targetType = targetType;
                if(m_targetType == GameTargetType.position)
                {
                    var posObj = obj.GetElement("Pos")?.JsonArray();
                    if (posObj != null)
                        m_pos = Json.ToVector3(posObj);
                    else m_pos = Vector3.zero;
                }
                else
                {
                    m_ID = obj.GetElement("ID")?.Int() ?? 0;
                    m_level = level;
                }
            }
            else
            {
                m_targetType = GameTargetType.invalid;
                m_pos = Vector3.zero;
            }
        }
    }

    public void Save(JsonObject obj)
    {
        obj.AddElement("Type", m_targetType.ToString());
        if (m_targetType == GameTargetType.position)
            obj.AddElement("Pos", Json.FromVector3(m_pos));
        else obj.AddElement("ID", m_ID);

        //GameTargetType m_targetType;
        //int m_ID;
        //Vector3 m_pos;
        //GameLevel m_level;
    }
}
