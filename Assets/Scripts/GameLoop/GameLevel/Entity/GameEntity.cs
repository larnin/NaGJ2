using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEntity
{
    int m_ID = 0;
    EntityType m_type = default;

    GameLevel m_level;
    GameEntityPath m_path;

    public GameEntity(EntityType type, GameLevel level)
    {
        m_type = type;
        m_level = level;

        m_path = new GameEntityPath(this);
    }

    public void SetID(int id)
    {
        m_ID = id;
    }

    public int GetID()
    {
        return m_ID;
    }

    public EntityType GetEntityType()
    {
        return m_type;
    }

    public void Process(float deltaTime)
    {
        m_path.Process(deltaTime);
    }

    public virtual void Load(JsonObject obj)
    {
        m_ID = obj.GetElement("ID")?.Int() ?? 0;
    }

    public virtual void Save(JsonObject obj)
    {
        obj.AddElement("ID", m_ID);
        obj.AddElement("Type", m_type.ToString());
    }

    public Vector3 GetPos()
    {
        return m_path.GetPos();
    }

    public float GetSeeDir()
    {
        return m_path.GetSeeDir();
    }

    public GameLevel GetLevel()
    {
        return m_level;
    }
}
