using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class GUIEx
{
    public static Color White = new Color(0.7f, 0.7f, 0.7f); 

    public static void DrawHorizontalLine(Color color, int tickness = 1)
    {
        GUIStyle horizontalLine;
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        horizontalLine.fixedHeight = tickness;

        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine, GUILayout.ExpandWidth(true));
        GUI.color = c;
    }

    public static void DrawVerticalLine(Color color, int tickness = 1)
    {
        GUIStyle verticalLine;
        verticalLine = new GUIStyle();
        verticalLine.normal.background = EditorGUIUtility.whiteTexture;
        verticalLine.margin = new RectOffset(0, 0, 4, 4);
        verticalLine.fixedWidth = tickness;

        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, verticalLine, GUILayout.ExpandHeight(true));
        GUI.color = c;
    }
}
