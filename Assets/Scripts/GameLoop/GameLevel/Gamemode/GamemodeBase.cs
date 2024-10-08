﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GamemodeType
{
    waves,
    camps,
    pylons
}

public enum GamemodeStatus
{
    notStarted,
    playing,
    completed,
    failed,
}

public abstract class GamemodeBase
{
    protected GameLevel m_level;

    protected GamemodeType m_type;
    protected GamemodeStatus m_status;

    protected string m_name;

    public GamemodeBase(GameLevel level)
    {
        m_level = level;
        m_status = GamemodeStatus.notStarted;
    }

    public static GamemodeBase Create(GamemodeType gamemode, GameLevel level)
    {
        switch(gamemode)
        {
            case GamemodeType.waves:
                return new GamemodeWaves(level);
            default:
                return null;
        }
    }

    public static GamemodeBase CreateAndLoad(JsonObject obj, GameLevel level)
    {
        string str = obj.GetElement("Type")?.String();
        if (str == null)
            return null;

        GamemodeType type;
        if (!Enum.TryParse(str, out type))
            return null;

        var mode = Create(type, level);
        if (mode == null)
            return null;

        mode.Load(obj);

        return mode;
    }

    public GamemodeType GetModeType()
    {
        return m_type;
    }

    public GamemodeStatus GetStatus()
    {
        return m_status;
    }

    public string GetName()
    {
        return m_name;
    }

    public void SetName(string name)
    {
        m_name = name;
    }

    public abstract void Reset();
    public abstract void Start();
    public abstract void Process(float deltaTime);

    protected virtual void Load(JsonObject obj)
    {
        Reset();

        m_name = obj.GetElement("Name")?.String();

        string str = obj.GetElement("Status")?.String();
        if (str != null)
        {
            if (!Enum.TryParse(str, out m_status))
                m_status = GamemodeStatus.notStarted;
        }

        m_status = GamemodeStatus.playing;
    }

    public virtual void Save(JsonObject obj)
    {
        obj.AddElement("Type", m_type.ToString());

        obj.AddElement("Name", m_name);
        obj.AddElement("Status", m_status.ToString());
    }

    public abstract EditorGamemodeViewBase CreateEditorView();
}
