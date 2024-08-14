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
}

public class GameTarget
{
    GameTargetType m_targetType;
    int m_ID;
    Vector3 m_pos;
    GameLevel m_level;

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
}
