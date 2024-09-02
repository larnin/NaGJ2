using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEditorWindowGamemodes : GameEditorWindowBase
{
    Vector2 m_scrollPos = Vector2.zero;
    List<bool> m_openList = new List<bool>();

    GamemodeType m_nextModeType = GamemodeType.waves;
    GUIDropdown m_nextModeDropdown;

    public GameEditorWindowGamemodes()
    {
        m_nextModeDropdown = new GUIDropdown();
        List<string> names = new List<string>();
        int nbNames = Enum.GetValues(typeof(GamemodeType)).Length;
        for (int i = 0; i < nbNames; i++)
            names.Add(((GamemodeType)i).ToString());
        m_nextModeDropdown.SetDatas(names);
        m_nextModeDropdown.SetLabel(m_nextModeType.ToString());
    }

    public override void OnEnable() 
    { 
    
    }

    public override void OnDisable() 
    { 
    
    }

    public override void OnGUI(Vector2 position)
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return;

        m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);

        for (int i = 0; i < level.level.gamemode.GetGamemodeNb(); i++)
            DrawOneGamemode(level.level.gamemode.GetGamemode(i), i, position);

        DrawNewGamemode(position);

        GUILayout.EndScrollView();
    }

    void DrawOneGamemode(GamemodeBase gamemode, int index, Vector2 position)
    {
        if(DrawOneGamemodeHeader(gamemode, index))
        {
            var editor = EditorGamemodeListView.GetCurrent();
            if(editor != null)
            {
                var view = editor.GetGamemodeView(gamemode);
                if (view != null)
                {
                    GUILayout.BeginHorizontal();
                    GUIEx.DrawVerticalLine(Color.white, 2, true);
                    GUILayout.BeginVertical();
                    view.OnGui(position);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
            }
        }
    }

    bool DrawOneGamemodeHeader(GamemodeBase gamemode, int index)
    {
        string label = index.ToString() + ": " + gamemode.GetModeType().ToString();

        while (m_openList.Count <= index)
            m_openList.Add(false);

        bool opened = m_openList[index];

        var style = new GUIStyle(GUI.skin.button);
        style.alignment = TextAnchor.MiddleLeft;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(label, style))
        {
            opened = !opened;
            m_openList[index] = opened;
        }

        if(GUILayout.Button("X", GUILayout.Width(25)))
        {
            GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
            Event<GameGetCurrentLevelEvent>.Broadcast(level);
            if (level.level != null)
            {
                level.level.gamemode.RemoveGamemodeAt(index);
                m_openList.RemoveAt(index);
                opened = false;
            }
        }

        GUILayout.EndHorizontal();

        return opened;
    }

    void DrawNewGamemode(Vector2 position)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("New gamemode:", GUILayout.Width(100)) ;

        int modeSelect = m_nextModeDropdown.OnGUI(position);
        if (modeSelect >= 0)
        {
            m_nextModeType = (GamemodeType)modeSelect;
            m_nextModeDropdown.SetLabel(m_nextModeType.ToString());
        }

        if (GUILayout.Button("Add", GUILayout.MaxWidth(50)))
        {
            GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
            Event<GameGetCurrentLevelEvent>.Broadcast(level);
            if (level.level != null)
            {
                var newMode = GamemodeBase.Create(m_nextModeType, level.level);
                level.level.gamemode.AddGamemode(newMode);
            }
        }

        GUILayout.EndHorizontal();
    }
}
