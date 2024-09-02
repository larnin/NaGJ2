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

    public static float FloatField(float value, params GUILayoutOption[] options)
    {
        string newValue = GUILayout.TextField(value.ToString(), options);
        float newFloat = 0;

        if (!Single.TryParse(newValue, out newFloat))
            return 0;
        return newFloat;
    }

    public static Vector3 Vector3Field(Vector3 value, params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal();

        for (int i = 0; i < 3; i++)
            value[i] = FloatField(value[i], options);

        GUILayout.EndHorizontal();

        return value;
    }

    public static Vector3Int Vector3IntField(Vector3Int value, params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal();

        for (int i = 0; i < 3; i++)
            value[i] = IntField(value[i], options);

        GUILayout.EndHorizontal();

        return value;
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

    public static void DrawHorizontalLine(Color color, int tickness = 1, bool expand = true)
    {
        GUIStyle horizontalLine;
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = Texture2D.whiteTexture;
        horizontalLine.margin = new RectOffset(4, 4, 4, 4);
        horizontalLine.fixedHeight = tickness;

        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine, GUILayout.ExpandWidth(expand));
        GUI.color = c;
    }

    public static void DrawVerticalLine(Color color, int tickness = 1, bool expand = true)
    {
        GUIStyle verticalLine;
        verticalLine = new GUIStyle();
        verticalLine.normal.background = Texture2D.whiteTexture;
        verticalLine.margin = new RectOffset(4, 4, 4, 4);
        verticalLine.fixedWidth = tickness;

        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, verticalLine, GUILayout.ExpandHeight(expand));
        GUI.color = c;
    }
}
