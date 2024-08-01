using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEditorWindowBlocks : GameEditorWindowBase
{
    string filter = "";

    public override void OnGUI()
    {
        if (Global.instance == null)
            return;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Filter ", GUILayout.MaxWidth(30));
        GUILayout.TextField(filter);
        GUILayout.EndHorizontal();

        var blocks = Global.instance.editor.blocks;
        //todo
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
}
