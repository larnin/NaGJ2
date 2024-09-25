using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRand;

public class GameOneWorld
{
    const float saveTimeActive = 5;
    const float saveTimeInactive = 60;

    OneLevelDatas m_levelInitialData;

    GameLevel m_level = new GameLevel();
    public GameLevel level { get { return m_level; } }

    bool m_visited = false;
    float m_time = 0;
    float m_saveTimer = 0;

    bool m_active = false;

    public GameOneWorld(OneLevelDatas level)
    {
        m_levelInitialData = level;
    }

    public void Init()
    {
        Reset();

        //LoadSave();
    }

    public void Start()
    {

    }

    public void Reset()
    {
        m_visited = false;
        m_time = 0;

        m_saveTimer = new UniformFloatDistribution(saveTimeInactive).Next(new StaticRandomGenerator<MT19937>());

        var doc = Json.ReadFromString(m_levelInitialData.level.data);

        if (doc != null)
            m_level.Load(doc);
    }

    public void Update(float deltaTime)
    {
        if (m_visited)
        {
            m_time += deltaTime;
            m_level.Process(deltaTime);
        }

        m_saveTimer += deltaTime;
        float maxTimer = m_active ? saveTimeActive : saveTimeInactive;
        if(m_saveTimer >= maxTimer)
        {
            m_saveTimer = 0;
            WriteSave();
        }
    }

    public void Destroy()
    {

    }

    public OneLevelDatas GetLevelData()
    {
        return m_levelInitialData;
    }

    string GetSaveNameWithoutExt()
    {
        int slot = Save.instance.GetCurrentSlot();

        string path = Save.instance.GetSavePath(slot);

        path += "World\\" + m_levelInitialData.ID;

        return path;
    }

    void LoadSave()
    {
        string path = GetSaveNameWithoutExt();

        LoadHeader(path + ".data");
        LoadLevel(path + ".lvl");
    }

    void WriteSave()
    {
        string path = GetSaveNameWithoutExt();

        WriteHeader(path + ".data");
        WriteLevel(path + ".lvl");
    }

    void LoadHeader(string path)
    {
        var doc = Json.ReadFromFile(path);

        if (doc == null)
            return;

        var headerElt = doc.GetRoot();
        if (headerElt.IsJsonObject())
        {
            var root = headerElt.JsonObject();

            var visitedElt = root.GetElement("Visited");
            if (visitedElt != null && visitedElt.IsJsonNumber())
                m_visited = visitedElt.Int() == 0 ? false : true;

            var timeElt = root.GetElement("Time");
            if (timeElt != null && timeElt.IsJsonNumber())
                m_time = timeElt.Float();
        }
    }

    void WriteHeader(string path)
    {
        if (!m_visited)
        {
            SaveEx.DeleteFile(path);
        }
        else
        {
            var doc = new JsonDocument();
            var root = new JsonObject();
            doc.SetRoot(root);

            root.AddElement("Visited", m_visited ? 1 : 0);
            root.AddElement("Time", m_time);

            Json.WriteToFile(path, doc);
        }
    }

    void LoadLevel(string path)
    {
        var doc = Json.ReadFromFile(path); 
        
        if (doc == null)
            return;

        m_level.Load(doc);
    }

    void WriteLevel(string path)
    {
        if (!m_visited)
        {
            SaveEx.DeleteFile(path);
        }
        else
        {
            var doc = new JsonDocument();
            doc.SetRoot(new JsonObject());

            m_level.Save(doc);

            Json.WriteToFile(path, doc);
        }
    }

    public void SetActive(bool active)
    {
        if (active)
            m_visited = true;

        m_level.active = active;
    }

    public bool IsActive()
    {
        return m_active;
    }
}