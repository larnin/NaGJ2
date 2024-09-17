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
    Team m_team = Team.player;

    GameLevel m_level;
    GameEntityPath m_path;

    public GameEntity(EntityType type, GameLevel level)
    {
        m_type = type;
        m_level = level;

        m_path = new GameEntityPath(this, Vector3.zero);
    }

    public GameEntity(EntityType type, Team team, Vector3 pos, GameLevel level)
    {
        m_type = type;
        m_level = level;
        m_team = team;

        m_path = new GameEntityPath(this, pos);
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

    public Team GetTeam()
    {
        return m_team;
    }

    public void Process(float deltaTime)
    {
        m_path.Process(deltaTime);
    }

    public void Load(JsonObject obj)
    {
        m_ID = obj.GetElement("ID")?.Int() ?? 0;

        string str = obj.GetElement("Type")?.String();
        if (str != null)
        {
            EntityType type;
            if (!Enum.TryParse(str, out type))
                m_type = type;
        }

        str = obj.GetElement("Team")?.String();
        if (str != null)
        {
            Team team;
            if (!Enum.TryParse(str, out team))
                m_team = team;
        }

        var pathObj = obj.GetElement("Path")?.JsonObject();
        if(pathObj != null)
            m_path.Load(pathObj);
    }

    public void Save(JsonObject obj)
    {
        obj.AddElement("ID", m_ID);
        obj.AddElement("Type", m_type.ToString());
        obj.AddElement("Team", m_team.ToString());

        var pathObj = new JsonObject();
        obj.AddElement("Path", pathObj);
        m_path.Save(pathObj);
    }

    public GameEntityPath GetPath()
    {
        return m_path;
    }

    public Vector3 GetPos()
    {
        return m_path.GetPos();
    }

    public Vector3 GetViewPos()
    {
        var size = Global.instance.allBlocks.blockSize;
        var pos = m_path.GetPos();

        return new Vector3(size.x * pos.x, size.y * pos.y, size.z * pos.z);
    }

    public Quaternion GetViewMoveRotation()
    {
        return Quaternion.Euler(0, m_path.GetMoveDir() * Mathf.Rad2Deg, 0);
    }

    public GameLevel GetLevel()
    {
        return m_level;
    }
}
