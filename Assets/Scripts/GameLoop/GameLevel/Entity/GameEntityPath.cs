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

    }
}
