﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameOneWorld
{
    OneLevelDatas m_levelInitialData;

    GameLevel m_level;

    bool m_visited = false;
    float m_time = 0;

    public GameOneWorld(OneLevelDatas level)
    {
        m_levelInitialData = level;
    }

    public void Init()
    {
        Reset();


    }

    public void Start()
    {

    }

    public void Reset()
    {
        m_visited = false;
        m_time = 0;
    }

    public void Update(float deltaTime)
    {

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
        if (slot < 0)
            return null;

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

            m_level.Save(doc);

            Json.WriteToFile(path, doc);
        }
    }
}