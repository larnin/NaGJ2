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
    float m_velocity;
    float m_moveDir;

    bool m_moving = false;

    public GameEntityPath(GameEntity entity, Vector3 pos)
    {
        m_entity = entity;
        m_pos = pos;

        m_target = GameTarget.FromPos(pos);
    }

    public void Process(float deltaTime)
    {
        var type = m_entity.GetEntityType();
        var data = EntityDataEx.Get(type);
        if (data == null)
            return;
        
        var dir = new Vector3(Mathf.Cos(m_moveDir), 0, Mathf.Sin(m_moveDir));

        if (m_moving)
        {
            var nextPos = m_path.GetNextPos(m_pos);

            dir = nextPos - m_pos;
            dir.y = 0;
            float dist = dir.magnitude;
            dir /= dist;

            if (dist < 0.2f)
                m_moving = false;

            float dirAngle = Mathf.Atan2(dir.z, dir.x);

            if (m_velocity < 0.001f)
                m_moveDir = dirAngle;
            else
            {
                float offsetAngle = dirAngle - m_moveDir;
                while (offsetAngle < Mathf.PI)
                    offsetAngle += 2 * Mathf.PI;
                while (offsetAngle > Mathf.PI)
                    offsetAngle -= 2 * Mathf.PI;

                float realOffset = data.rotationSpeed * deltaTime;
                if (realOffset > Mathf.Abs(offsetAngle))
                    realOffset = offsetAngle;
                else if (offsetAngle < 0)
                    realOffset *= -1;

                m_moveDir += realOffset;
            }

            m_velocity += data.acceleration * deltaTime;
            if (m_velocity > data.moveSpeed)
                m_velocity = data.moveSpeed;
        }
        else
        {
            m_velocity -= data.acceleration * deltaTime * 2;
            if (m_velocity < 0)
                m_velocity = 0;
        }

        if (m_velocity > 0)
        {
            var nextPos = m_pos + m_velocity * deltaTime * dir;
            //todo collisions & physics

            m_pos = nextPos;
        }
    }

    public void SetTarget(GameTarget target)
    {
        m_target = target;

        RebuildPath();
    }

    public GameTarget GetTarget()
    {
        return m_target;
    }

    public Vector3 GetPos()
    {
        return m_pos;
    }

    public float GetVelocity()
    {
        return m_velocity;
    }

    public float GetMoveDir()
    {
        return m_moveDir;
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

    public void Load(JsonObject obj)
    {
        
    }

    public void Save(JsonObject obj)
    {
        
    }
}
