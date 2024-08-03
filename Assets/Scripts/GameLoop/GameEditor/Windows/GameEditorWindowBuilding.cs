using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEditorWindowBuilding : GameEditorWindowBase
{
    string m_filter = "";
    Vector2 m_scrollPos = Vector2.zero;

    Team m_team = Team.player;
    int m_level = 0;

    GUIDropdown m_teamDropdown;

    public GameEditorWindowBuilding()
    {
        m_teamDropdown = new GUIDropdown();
        List<string> names = new List<string>();
        int nbNames = Enum.GetValues(typeof(Team)).Length;
        for (int i = 0; i < nbNames; i++)
            names.Add(((Team)i).ToString());
        m_teamDropdown.SetDatas(names);
        m_teamDropdown.SetLabel(m_team.ToString());
    }

    public override void OnGUI(Vector2 position)
    {
        if (Global.instance == null)
            return;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Team");
        int teamSelect = m_teamDropdown.OnGUI(position);
        if(teamSelect >= 0)
        {
            m_team = (Team)teamSelect;
            m_teamDropdown.SetLabel(m_team.ToString());
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Level");
        m_level = GUIEx.IntField(m_level);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Filter ", GUILayout.MaxWidth(30));
        m_filter = GUILayout.TextField(m_filter);
        GUILayout.EndHorizontal();

        m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);

        if (GUILayout.Button("Empty"))
            OnClickEmpty();

        int nbTypes = Enum.GetValues(typeof(BuildingType)).Length;

        for(int i = 0; i < nbTypes; i++)
        {
            var type = (BuildingType)i;

            if (!CheckFilter(m_filter, type.ToString()))
                continue;

            if (GUILayout.Button(type.ToString()))
                OnClick(type);
        }

        GUILayout.EndScrollView();
    }

    bool CheckFilter(string filter, string name)
    {
        string filterData = filter;
        bool contains = true;
        if (filter.StartsWith('-'))
        {
            contains = false;
            filterData = filter.Substring(1);
        }

        if (filterData.Length == 0)
            return true;

        return name.Contains(filterData) == contains;
    }

    void OnClickEmpty()
    {
        Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(BlockType.air, 0));
    }

    void OnClick(BuildingType b)
    {
        Event<EditorSetCursorBuildingEvent>.Broadcast(new EditorSetCursorBuildingEvent(b, m_level, m_team));
    }
}
