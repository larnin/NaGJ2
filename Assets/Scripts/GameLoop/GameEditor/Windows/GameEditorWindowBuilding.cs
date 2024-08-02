using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEditorWindowBuilding : GameEditorWindowBase
{
    string m_filter = "";
    Vector2 m_scrollPos = Vector2.zero;

    public override void OnGUI()
    {
        if (Global.instance == null)
            return;

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
        //Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(b.type, b.data));
    }
}
