using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEntityPath
{
    GameEntity m_entity;
    GameTarget m_target;

    GamePath m_path;

    Vector3 m_pos;
    Vector3 m_velocity;
    float m_seeDir;

    bool m_moving = false;

    public GameEntityPath(GameEntity entity)
    {
        m_entity = entity;
    }

    public void Process(float deltaTime)
    {

    }

    public void SetTarget(GameTarget target)
    {
        m_target = target;

        RebuildPath();
    }

    public Vector3 GetPos()
    {
        return m_pos;
    }

    public Vector3 GetVelocity()
    {
        return m_velocity;
    }

    public float GetSeeDir()
    {
        return m_seeDir;
    }

    void RebuildPath()
    {
        var type = m_entity.GetEntityType();
        var entityData = EntityDataEx.Get(type);

        if(entityData == null || !m_target.IsValid())
        {
            m_path.Reset();
            return;
        }    

        switch(entityData.behaviour)
        {
            case EntityBehaviourType.Ground:
                m_path.SetPath(GamePathMaker.MakeGroundPath(m_entity.GetLevel(), m_pos, m_target.GetTargetPos(), Mathf.CeilToInt(entityData.radius + 0.5f), Mathf.CeilToInt(entityData.height)));
                break;
            case EntityBehaviourType.Water:
                m_path.SetPath(GamePathMaker.MakeWaterPath(m_entity.GetLevel(), m_pos, m_target.GetTargetPos(), Mathf.CeilToInt(entityData.radius + 0.5f), Mathf.CeilToInt(entityData.height)));
                break;
            case EntityBehaviourType.Air:
                //todo !
                m_path.Reset();
                break;
        }
    }
}
