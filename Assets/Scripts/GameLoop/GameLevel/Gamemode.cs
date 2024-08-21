using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Gamemode
{
    GameLevel m_level;

    public Gamemode(GameLevel level)
    {
        m_level = level;
    }

    public void Reset()
    {

    }

    public void Load(JsonDocument doc)
    {
        Reset();

        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var modeObject = rootObject.GetElement("Gamemode")?.JsonObject();
            if (modeObject != null)
            {
                //todo
            }
        }
    }

    public void Save(JsonDocument doc)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var modeObject = new JsonObject();
            rootObject.AddElement("Gamemode", modeObject);

            //todo
        }
    }

    public void Process(float deltaTime)
    {

    }
}
