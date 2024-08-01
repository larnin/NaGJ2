using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEditorWindowBlocks : GameEditorWindowBase
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

        var blocks = Global.instance.editor.blocks;

        m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);

        if (GUILayout.Button("Empty"))
            OnClickEmpty();

        foreach (var b in blocks)
        {
            bool valid = CheckFilter(m_filter, b.type.ToString());
            valid |= CheckFilter(m_filter, b.name);

            if (!valid)
                continue;

            if (GUILayout.Button(b.name + " (" + b.type + " - " + b.data + ")"))
                OnClick(b);

        }
        GUILayout.EndScrollView();
    }

    bool CheckFilter(string filter, string name)
    {
        string filterData = filter;
        bool contains = true;
        if(filter.StartsWith('-'))
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

    void OnClick(EditorBlock b)
    {
        Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(b.type, b.data));
    }
}
