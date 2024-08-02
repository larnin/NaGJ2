using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public  class GameEditorInterface : MonoBehaviour
{
    [SerializeField] List<GameObject> m_itemLists = new List<GameObject>();
    [SerializeField] Image m_currentElementImage = null;
    [SerializeField] TMP_Text m_filename;
    [SerializeField] string menuName;

    GameLevel m_level = new GameLevel();

    SubscriberList m_subscriberList = new SubscriberList();

    bool m_clicked = false;
    int m_currentList = -1;
    string m_currentPath = "";

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorSelectBlockTypeEvent>.Subscriber(OnClickBlock));
        m_subscriberList.Add(new Event<EditorSelectBuildingTypeEvent>.Subscriber(OnClickBuilding));
        m_subscriberList.Add(new Event<GameGetCurrentLevelEvent>.Subscriber(GetLevel));

        m_subscriberList.Subscribe();

        m_level.active = true;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        Event<GameSetCurrentLevelEvent>.Broadcast(new GameSetCurrentLevelEvent(m_level));

        Event<GameResetEvent>.Broadcast(new GameResetEvent());
        Event<GameLoadEvent>.Broadcast(new GameLoadEvent());

        OnOpenListClick(-1);
        UpdateFilename();
    }

    private void Update()
    {
        UpdateCurrentElement();

        //m_level.Process(Time.deltaTime);
    }

    void UpdateCurrentElement()
    {
        if (m_currentElementImage == null)
            return;

        EditorGetCursorTypeEvent cursorType = new EditorGetCursorTypeEvent();
        Event<EditorGetCursorTypeEvent>.Broadcast(cursorType);

        Sprite s = null;
        if(cursorType.cursorType == EditorCursorType.Block)
        {
            EditorGetCursorBlockEvent b = new EditorGetCursorBlockEvent();
            Event<EditorGetCursorBlockEvent>.Broadcast(b);
            s = BlockDataEx.GetSprite(b.type, b.blockData);
        }
        else if(cursorType.cursorType == EditorCursorType.Building)
        {
            EditorGetCursorBuildingEvent b = new EditorGetCursorBuildingEvent();
            Event<EditorGetCursorBuildingEvent>.Broadcast(b);
            s = BuildingDataEx.GetSprite(b.type, b.level);
        }

        if (s == null)
            m_currentElementImage.enabled = false;
        else
        {
            m_currentElementImage.enabled = true;
            m_currentElementImage.sprite = s;
        }
    }

    void UpdateFilename()
    {
        if (m_currentPath.Length == 0)
        {
            m_filename.text = "New Level";
            return;
        }

        string filename = "";
        int posSlash = m_currentPath.LastIndexOfAny(new char[] { '/', '\\' });
        if (posSlash >= 0)
            filename = m_currentPath.Substring(posSlash + 1);
        else String.Copy(m_currentPath);

        int posDot = filename.LastIndexOf('.');
        if (posDot > 0)
            filename = filename.Substring(0, posDot);

        m_filename.text = filename;
    }

    public void OnOpenListClick(int listIndex)
    {
        foreach (var g in m_itemLists)
            g.SetActive(false);

        if (listIndex == m_currentList || listIndex < 0 || listIndex >= m_itemLists.Count)
        {
            m_currentList = -1;
            return;
        }

        m_itemLists[listIndex].SetActive(true);
        m_currentList = listIndex;
    }

    void OnClickBlock(EditorSelectBlockTypeEvent e)
    {
        Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(e.type, e.value));
    }

    void OnClickBuilding(EditorSelectBuildingTypeEvent e)
    {
        Event<EditorSetCursorBuildingEvent>.Broadcast(new EditorSetCursorBuildingEvent(e.type, e.level, Team.player));
    }

    public void OnEmptyClick()
    {
        Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(BlockType.air, 0));
    }

    void GetLevel(GameGetCurrentLevelEvent e)
    {
        e.level = m_level;
    }

    public void OnNew()
    {
        m_currentPath = "";
        UpdateFilename();

        m_level.Reset();

        Event<GameResetEvent>.Broadcast(new GameResetEvent());
        Event<GameLoadEvent>.Broadcast(new GameLoadEvent());
    }

    public void OnSave()
    {
        if (m_currentPath.Length == 0)
        {
            OnSaveAs();
            return;
        }

        JsonDocument doc = new JsonDocument();
        doc.SetRoot(new JsonObject());

        m_level.Save(doc);

#if UNITY_EDITOR
        string relativePath = SaveEx.GetRelativeAssetPath(m_currentPath);
        if (relativePath != m_currentPath)
        {
            SaveEx.SaveLevelFromEditor(relativePath, doc);
        }
        else
#endif
        {
            Json.WriteToFile(m_currentPath, doc);
        }
    }

    public void OnSaveAs()
    {
        string path = SaveEx.GetSaveFilePath("Save world", m_currentPath, "asset");
        if (path.Length == 0)
            return;

        m_currentPath = path;

        UpdateFilename();

        OnSave();
    }

    public void OnLoad()
    {
        string path = SaveEx.GetLoadFiltPath("Load world", m_currentPath, "asset");
        if (path.Length == 0)
            return;

        m_currentPath = path;

        UpdateFilename();

        JsonDocument doc = null;

#if UNITY_EDITOR
        string relativePath = SaveEx.GetRelativeAssetPath(m_currentPath);
        if (relativePath != m_currentPath)
        {
            doc = SaveEx.LoadLevelFromEditor(relativePath);
        }
        else
#endif
        {
            doc = Json.ReadFromFile(m_currentPath);
        }

        m_level.Load(doc);
        Event<GameLoadEvent>.Broadcast(new GameLoadEvent());
    }

    public void OnQuit()
    {
        if (m_clicked)
            return;

        m_clicked = true;
        SceneSystem.changeScene(menuName);
    }
}
