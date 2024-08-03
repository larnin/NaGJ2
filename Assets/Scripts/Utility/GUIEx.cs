using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GUIEx
{
    static int m_nextWindowID;
    public static int GetNextWindowID()
    {
        int nextID = m_nextWindowID;
        m_nextWindowID++;
        return nextID;
    }

    public static int IntField(int value, params GUILayoutOption[] options)
    {
        string newValue = GUILayout.TextField(value.ToString(), options);
        int newInt = 0;

        if (!Int32.TryParse(newValue, out newInt))
            return 0;
        return newInt;
    }

    //return new selected state
    public static int DrawDropdown(Rect rect, ref bool selected, string label, GUIDropdownData[] datas)
    {
        if (GUI.Button(rect, label))
            selected = !selected;

        return DrawDropdownData(rect, ref selected, datas);
    }

    public static int DrawDropdown(ref bool selected, string label, GUIDropdownData[] datas)
    {
        if (GUILayout.Button(label))
            selected = !selected;

        var rect = GUILayoutUtility.GetLastRect();

        return DrawDropdownData(rect, ref selected, datas);
    }

    static int DrawDropdownData(Rect rect, ref bool selected, GUIDropdownData[] datas)
    {
        int returnValue = -1;

        const float buttonSpacing = 2;
        const float buttonHeight = 20;

        if (selected)
        {
            float height = datas.Length * buttonHeight + buttonSpacing + buttonSpacing;
            GUI.Box(new Rect(rect.x, rect.y + rect.height, rect.width, height), "");

            for (int i = 0; i < datas.Length; i++)
            {
                float y = buttonSpacing + i * (buttonSpacing + buttonHeight);
                if (GUI.Button(new Rect(rect.x + buttonSpacing, rect.y + rect.height + y, rect.width - 2 * buttonSpacing, buttonHeight), datas[i].name))
                {
                    selected = false;
                    returnValue = i;
                }

                if (datas[i].selected)
                {
                    GUI.Label(new Rect(rect.x + rect.width - buttonSpacing - buttonHeight, rect.y + rect.height + y, buttonHeight, buttonHeight), "✓");
                }
            }
        }

        return returnValue;
    }
}
