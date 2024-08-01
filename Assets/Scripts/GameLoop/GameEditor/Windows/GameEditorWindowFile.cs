using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEditorWindowFile : GameEditorWindowBase
{
    string m_currentPath = "";

    bool m_clicked = false;

    public override void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(GetFileName());
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("New"))
            OnNew();

        if (GUILayout.Button("Save"))
            OnSave();

        if (GUILayout.Button("Save As ..."))
            OnSaveAs();

        if (GUILayout.Button("Load"))
            OnLoad();

        if (GUILayout.Button("Quit"))
            OnQuit();
    }

    string GetFileName()
    {
        if (m_currentPath.Length == 0)
            return "New Level";

        string filename = "";
        int posSlash = m_currentPath.LastIndexOfAny(new char[] { '/', '\\' });
        if (posSlash >= 0)
            filename = m_currentPath.Substring(posSlash + 1);
        else String.Copy(m_currentPath);

        int posDot = filename.LastIndexOf('.');
        if (posDot > 0)
            filename = filename.Substring(0, posDot);

        return filename;
    }

    void OnNew()
    {
        m_currentPath = "";

        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return;

        level.level.Reset();

        Event<GameResetEvent>.Broadcast(new GameResetEvent());
        Event<GameLoadEvent>.Broadcast(new GameLoadEvent());
    }

    void OnSave()
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return;

        if (m_currentPath.Length == 0)
        {
            OnSaveAs();
            return;
        }

        JsonDocument doc = new JsonDocument();
        doc.SetRoot(new JsonObject());

        level.level.Save(doc);

#if UNITY_EDITOR
        string relativePath = SaveEx.GetRelativeAssetPath(m_currentPath);
        if (relativePath != m_currentPath)
        {
            SaveEx.SaveLevelFromEditor(relativePath, doc);
        }
        else
#endif
        {
            Json.WriteToFile(m_currentPath, doc);
        }
    }

    void OnSaveAs()
    {
        string path = SaveEx.GetSaveFilePath("Save world", m_currentPath, "asset");
        if (path.Length == 0)
            return;

        m_currentPath = path;

        OnSave();
    }

    void OnLoad()
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return;

        string path = SaveEx.GetLoadFiltPath("Load world", m_currentPath, "asset");
        if (path.Length == 0)
            return;

        m_currentPath = path;

        JsonDocument doc = null;

#if UNITY_EDITOR
        string relativePath = SaveEx.GetRelativeAssetPath(m_currentPath);
        if (relativePath != m_currentPath)
        {
            doc = SaveEx.LoadLevelFromEditor(relativePath);
        }
        else
#endif
        {
            doc = Json.ReadFromFile(m_currentPath);
        }

        level.level.Load(doc);
        Event<GameLoadEvent>.Broadcast(new GameLoadEvent());
    }

    void OnQuit()
    {
        if (m_clicked)
            return;

        m_clicked = true;
        SceneSystem.changeScene("MainMenu");
    }
}
