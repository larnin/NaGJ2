using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GameEditorWindowType
{
    Window_File,
    Window_Blocks,
    Window_Buildings,
    Window_Entities,
}

public class GameEditorInterface2 : MonoBehaviour
{
    const float minWindowSize = 100;
    const float handleSize = 25;
    const float toolbarHeight = 30;

    class ResizeInfo
    {
        public int id;
        public bool resizeHandleClicked;
        public Vector2 clickedPos;
        public Rect originalRect;

        public ResizeInfo()
        {
            id = -1;

            resizeHandleClicked = false;
            clickedPos = Vector2.zero;
            originalRect = Rect.zero;
        }
    }

    class WindowInfo
    {
        public string name;
        public int id;
        public Rect rect;
        public bool enabled;
        public GameEditorWindowBase instance;

        public WindowInfo(int _id, string _name, GameEditorWindowBase _instance)
        {
            name = _name;
            id = _id;
            rect = new Rect(0, toolbarHeight, 200, 200);
            enabled = false;
            instance = _instance;
        }
    }

    List<WindowInfo> m_windows = new List<WindowInfo>();
    ResizeInfo m_resize = new ResizeInfo();
    List<bool> m_popupOpen = new List<bool>();

    bool[] m_debugDraws = new bool[Enum.GetValues(typeof(GameEditorWindowType)).Length];

    private void Awake()
    {
        int nbWindow = Enum.GetValues(typeof(GameEditorWindowType)).Length;
        m_windows.Clear();
        for (int i = 0; i < nbWindow; i++)
        {
            GameEditorWindowType type = (GameEditorWindowType)i;
            m_windows.Add(new WindowInfo(i, type.ToString(), CreateInstance(type)));
        }
    }

    GameEditorWindowBase CreateInstance(GameEditorWindowType type)
    {
        switch(type)
        {
            case GameEditorWindowType.Window_File:
                return new GameEditorWindowFile();
            default:
                return null;
        }
    }

    private void Start()
    {
        foreach (var w in m_windows)
        {
            if (w.instance != null)
                w.instance.Start();
        }
    }

    private void OnDestroy()
    {
        foreach (var w in m_windows)
        {
            if (w.instance != null)
                w.instance.OnDestroy();
        }
    }

    private void Update()
    {
        foreach(var w in m_windows)
        {
            if (w.instance != null)
            {
                if(w.enabled)
                    w.instance.Update();
                w.instance.AlwaysUpdate();
            }
        }
    }

    private void OnGUI()
    {
        DrawToolbar();

        ResizeWindow();

        foreach (var w in m_windows)
        {
            if (w.enabled)
            {
                w.rect = GUI.Window(w.id, w.rect, DrawWindow, w.name);
            }
        }
    }

    void DrawWindow(int id)
    {
        var w = GetWindow(id);
        if (w == null)
            return;

        GUI.Box(new Rect(w.rect.width - handleSize, w.rect.height - handleSize, handleSize, handleSize), "");

        if (w.instance != null)
            w.instance.OnGUI();

        if (m_resize.id != id)
            GUI.DragWindow();
    }

    WindowInfo GetWindow(int id)
    {
        foreach (var w in m_windows)
        {
            if (w.id == id)
                return w;
        }

        return null;
    }

    void ResizeWindow()
    {
        var mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y;

        var w = GetWindow(m_resize.id);

        if (w == null && Input.GetMouseButtonDown(0) && !m_resize.resizeHandleClicked)
        {
            foreach (var window in m_windows)
            {
                var windowHandle = new Rect(window.rect.x + window.rect.width - handleSize, window.rect.y + window.rect.height - handleSize, handleSize, handleSize);

                if (windowHandle.Contains(mousePos))
                {
                    m_resize.resizeHandleClicked = true;
                    m_resize.clickedPos = mousePos;
                    m_resize.originalRect = window.rect;
                    m_resize.id = window.id;
                    break;
                }
            }
        }

        w = GetWindow(m_resize.id);
        if (w != null)
        {
            var windowHandle = new Rect(w.rect.x + w.rect.width - handleSize, w.rect.y + w.rect.height - handleSize, handleSize, handleSize);

            if (m_resize.resizeHandleClicked)
            {
                if (Input.GetMouseButton(0))
                {
                    w.rect.width = Mathf.Max(m_resize.originalRect.width + (mousePos.x - m_resize.clickedPos.x), minWindowSize);
                    w.rect.height = Mathf.Max(m_resize.originalRect.height + (mousePos.y - m_resize.clickedPos.y), minWindowSize);
                }
                else
                {
                    m_resize.resizeHandleClicked = false;
                    m_resize.id = -1;
                }
            }
            else w.id = -1;
        }
    }


    const int popupID_Debug = 0;
    const int popupID_Draw = 1;

    void DrawToolbar()
    {
        const float labelWidth = 150;
        const float labelspacing = 10;

        GUI.Box(new Rect(0, 0, Screen.width, toolbarHeight), "");

        // window
        {
            DebugPopupData[] windowsDatas = new DebugPopupData[Enum.GetValues(typeof(GameEditorWindowType)).Length];
            for (int i = 0; i < windowsDatas.Length; i++)
                windowsDatas[i] = new DebugPopupData(((GameEditorWindowType)i).ToString(), m_windows[i].enabled);

            bool selected = GetPopupOpen(popupID_Debug);
            int clicked = DebugLayout.DrawPopup(new Rect(labelspacing + (labelspacing + labelWidth) * popupID_Debug, 2, labelWidth, toolbarHeight - 4), ref selected, "Windows", windowsDatas);
            if (selected)
                CloseOthersPopup(popupID_Debug);
            SetPopupOpen(popupID_Debug, selected);

            if (clicked >= 0)
            {
                m_windows[clicked].enabled = !m_windows[clicked].enabled;

                if (m_windows[clicked].instance != null)
                {
                    if (m_windows[clicked].enabled)
                        m_windows[clicked].instance.OnEnable();
                    else m_windows[clicked].instance.OnDisable();
                }
            }
        }
    }

    bool GetPopupOpen(int index)
    {
        if (m_popupOpen.Count <= index)
            return false;
        return m_popupOpen[index];
    }

    void SetPopupOpen(int index, bool value)
    {
        while (m_popupOpen.Count <= index)
            m_popupOpen.Add(false);
        m_popupOpen[index] = value;
    }

    void CloseOthersPopup(int index)
    {
        for (int i = 0; i < m_popupOpen.Count; i++)
            if (i != index)
                m_popupOpen[i] = false;
    }

}


