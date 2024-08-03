using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GUIDropdownData
{
    public string name;
    public bool selected;

    public GUIDropdownData(string _name, bool _selected = false)
    {
        name = _name;
        selected = _selected;
    }
}

public class GUIDropdown
{
    const float buttonSpacing = 2;
    const float buttonHeight = 20;

    int m_windowID;
    Rect m_rect;
    Rect m_displayRect;
    bool m_useRect = false;
    bool m_selected = false;
    string m_label = "";
    List<GUIDropdownData> m_datas = new List<GUIDropdownData>();
    int m_selectedIndex = -1;

    public GUIDropdown()
    {
        m_windowID = GUIEx.GetNextWindowID();
        m_useRect = false;
    }

    public GUIDropdown(Rect rect)
    {
        m_windowID = GUIEx.GetNextWindowID();
        m_useRect = true;
        m_rect = rect;
    }

    public void SetDatas(List<GUIDropdownData> datas)
    {
        m_datas = datas;
    }

    public void SetDatas(List<string> data)
    {
        m_datas = new List<GUIDropdownData>();
        foreach (var d in data)
            m_datas.Add(new GUIDropdownData(d));
    }

    public void SetLabel(string label)
    {
        m_label = label;
    }

    public int OnGUI()
    {
        return OnGUI(Vector2.zero);
    }

    public int OnGUI(Vector2 offset)
    {
        if (m_useRect)
        {
            m_displayRect = m_rect;

            if (GUI.Button(m_displayRect, m_label))
                m_selected = !m_selected;
        }
        else
        {
            if (GUILayout.Button(m_label))
                m_selected = !m_selected;

            if(Event.current.type == EventType.Repaint)
                m_displayRect = GUILayoutUtility.GetLastRect();
        }

        if (m_selected && Event.current.type == EventType.Repaint)
        {
            float height = m_datas.Count * (buttonHeight + buttonSpacing) + buttonSpacing;
            m_displayRect.y = m_displayRect.y + m_displayRect.height;
            m_displayRect.height = height;
            m_displayRect.x += offset.x;
            m_displayRect.y += offset.y;

            Event<EditorDrawWindowNextFrameEvent>.Broadcast(new EditorDrawWindowNextFrameEvent(m_windowID, m_displayRect, DrawWindow, "", true));
        }

        int returnValue = m_selectedIndex;
        m_selectedIndex = -1;
        return returnValue;
    }

    void DrawWindow(int windowID)
    {
        for (int i = 0; i < m_datas.Count; i++)
        {
            float y = buttonSpacing + i * (buttonSpacing + buttonHeight);
            if (GUI.Button(new Rect(buttonSpacing, y, m_displayRect.width - 2 * buttonSpacing, buttonHeight), m_datas[i].name))
                m_selectedIndex = i;

            if (m_datas[i].selected)
                GUI.Label(new Rect(m_displayRect.width - buttonSpacing - buttonHeight, m_displayRect.height + y, buttonHeight, buttonHeight), "✓");
        }

        if (m_selectedIndex >= 0)
            m_selected = false;
    }
}

