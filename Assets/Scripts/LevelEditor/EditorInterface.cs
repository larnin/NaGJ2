using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorInterface : MonoBehaviour
{
    [Serializable]
    struct EditorBlockData
    {
        public BlockType block;
        public int additionalData;
        public Sprite texture;
    }

    [Serializable]
    struct EditorBuildingData
    {
        public BuildingType building;
        public Sprite texture;
    }

    [SerializeField] List<GameObject> m_itemLists = new List<GameObject>();
    [SerializeField] List<EditorBlockData> m_blocks = new List<EditorBlockData>();
    [SerializeField] List<EditorBuildingData> m_buildings = new List<EditorBuildingData>();
    [SerializeField] Image m_currentElementImage = null;
    [SerializeField] TMP_Text m_filename;
    [SerializeField] string menuName;

    bool m_clicked = false;
    int m_currentList = -1;
    string m_currentPath = "";

    private void Start()
    {
        OnOpenListClick(-1);
        UpdateFilename();
    }

    private void Update()
    {
        UpdateCurrentElement();
    }

    void UpdateCurrentElement()
    {
        if (m_currentElementImage == null)
            return;

        EditorGetCursorBlockEvent b = new EditorGetCursorBlockEvent();
        Event<EditorGetCursorBlockEvent>.Broadcast(b);

        Sprite s = null;
        foreach(var block in m_blocks)
        {
            if(block.block == b.type && block.additionalData == b.blockData)
            {
                s = block.texture;
                break;
            }
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

        if(listIndex == m_currentList || listIndex < 0 || listIndex >= m_itemLists.Count)
        {
            m_currentList = -1;
            return;
        }

        m_itemLists[listIndex].SetActive(true);
        m_currentList = listIndex;
    }

    public void OnBlockClick(int blockIndex)
    {
        if (blockIndex < 0 || blockIndex >= m_blocks.Count)
            Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(BlockType.air, 0));
        else
        {
            var b = m_blocks[blockIndex];

            Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(b.block, b.additionalData));
        }
    }

    public void OnBuildingClick(int buildingIndex)
    {
        if(buildingIndex < 0 || buildingIndex >= m_buildings.Count)
            Event<EditorSetCursorBlockEvent>.Broadcast(new EditorSetCursorBlockEvent(BlockType.air, 0));
        else
        {
            var b = m_buildings[buildingIndex];

            Event<EditorSetCursorBuildingEvent>.Broadcast(new EditorSetCursorBuildingEvent(b.building, 0));
        }
    }

    public void OnNew()
    {
        m_currentPath = "";
        UpdateFilename();

        Event<NewLevelEvent>.Broadcast(new NewLevelEvent());
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

        SaveEvent e = new SaveEvent(doc);
        Event<SaveEvent>.Broadcast(e);

#if UNITY_EDITOR
        string relativePath = SaveEx.GetRelativeAssetPath(m_currentPath);
        if(relativePath != m_currentPath)
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

        LoadEvent e = new LoadEvent(doc);
        Event<LoadEvent>.Broadcast(e);
    }

    public void OnQuit()
    {
        if (m_clicked)
            return;

        m_clicked = true;
        SceneSystem.changeScene(menuName);
    }
}
