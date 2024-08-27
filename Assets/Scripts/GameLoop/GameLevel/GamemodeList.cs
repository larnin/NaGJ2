using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GamemodeList
{
    GameLevel m_level;
    List<GamemodeBase> m_modes = new List<GamemodeBase>();

    public GamemodeList(GameLevel level)
    {
        m_level = level;
    }

    public void Reset()
    {
        m_modes.Clear();
    }

    public void Load(JsonDocument doc)
    {
        Reset();

        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var modesObject = rootObject.GetElement("Gamemode")?.JsonObject();
            if (modesObject != null)
            {
                var dataArray = modesObject.GetElement("List")?.JsonArray();
                if (dataArray != null)
                {
                    foreach (var elem in dataArray)
                    {
                        var modeObject = elem.JsonObject();
                        if (modeObject != null)
                        {
                            var mode = GamemodeBase.CreateAndLoad(modeObject, m_level);
                            if (mode != null)
                                m_modes.Add(mode);
                        }
                    }
                }
            }
        }
    }

    public void Save(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var modesObject = new JsonObject();
            rootObject.AddElement("Gamemode", modesObject);

            var modesArray = new JsonArray();
            modesObject.AddElement("List", modesArray);

            foreach (var mode in m_modes)
            {
                var modeObject = new JsonObject();
                modesArray.Add(modeObject);
                mode.Save(modeObject);
            }
        }
    }

    public void Process(float deltaTime)
    {
        foreach(var mode in m_modes)
        {
            if (mode.GetStatus() == GamemodeStatus.playing)
                mode.Process(deltaTime);
        }
    }

    public int GetGamemodeNb()
    {
        return m_modes.Count();
    }

    public GamemodeBase GetGamemode(int index)
    {
        return m_modes[index];
    }

    public void AddGamemode(GamemodeBase gamemode)
    {
        m_modes.Add(gamemode);
    }

    public void RemoveGamemode(GamemodeBase gamemode)
    {
        m_modes.Remove(gamemode);
    }

    public void RemoveGamemodeAt(int index)
    {
        m_modes.RemoveAt(index);
    }
}
