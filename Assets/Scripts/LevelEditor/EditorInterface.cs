using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    [SerializeField] List<GameObject> m_itemLists = new List<GameObject>();
    [SerializeField] List<EditorBlockData> m_blocks = new List<EditorBlockData>();
    [SerializeField] Image m_currentElementImage = null;

    int m_currentList = -1;

    private void Start()
    {
        OnOpenListClick(-1);
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
}
