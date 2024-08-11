using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameEntity
{
    int m_ID = 0;
    EntityType m_type = default;

    GameLevel m_level;

    public GameEntity(EntityType type, GameLevel level)
    {
        m_type = type;
        m_level = level;
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
}
