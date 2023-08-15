using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorCursorVisual : MonoBehaviour
{
    [SerializeField] GameObject m_cursorPrefab = null;

    BlockType m_blockType = BlockType.air;
    Rotation m_rotation = Rotation.rot_0;

    SubscriberList m_subscriberList = new SubscriberList();

    GameObject m_cursor = null;

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorCursorClickEvent>.Subscriber(OnClick));
        m_subscriberList.Add(new Event<EditorSetCursorBlockEvent>.Subscriber(SetType));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        InitCursor();

        SetType(new EditorSetCursorBlockEvent(BlockType.ground));
    }

    private void Update()
    {
        EditorCurstorGetPosEvent posData = new EditorCurstorGetPosEvent();
        Event<EditorCurstorGetPosEvent>.Broadcast(posData);

        Vector3Int pos = Vector3Int.zero;
        bool valid = BlockDataEx.GetValidPos(m_blockType, posData.blockPos, posData.pos, out pos);
        if (valid)
            SetCursorPosition(pos, true);
        else SetCursorPosition(pos, false);
    }

    void OnClick(EditorCursorClickEvent e)
    {
        EditorCurstorGetPosEvent posData = new EditorCurstorGetPosEvent();
        Event<EditorCurstorGetPosEvent>.Broadcast(posData);

        Vector3Int pos = Vector3Int.zero;

        if(e.clickType == EditorCursorClickType.leftClick)
        {
            bool valid = BlockDataEx.GetValidPos(m_blockType, posData.blockPos, posData.pos, out pos);
            if (!valid)
                return;

            BlockDataEx.SetBlock(m_blockType, m_rotation, pos);
        }
        else if(e.clickType == EditorCursorClickType.rightClick)
        {
            bool valid = BlockDataEx.GetValidPos(BlockType.air, posData.blockPos, posData.pos, out pos);
            if (!valid)
                return;
            BlockDataEx.SetBlock(BlockType.air, Rotation.rot_0, pos);
        }
    }

    void SetType(EditorSetCursorBlockEvent e)
    {
        m_blockType = e.type;

        SetCursorPosition(Vector3Int.zero, false);
    }

    void InitCursor()
    {
        if (m_cursorPrefab == null)
            return;

        m_cursor = Instantiate(m_cursorPrefab);
        m_cursor.transform.parent = transform;
        m_cursor.transform.localPosition = Vector3.zero;
        m_cursor.SetActive(false);
    }

    void SetCursorPosition(Vector3Int pos, bool visible)
    {
        if (!m_cursor)
            return;

        m_cursor.SetActive(visible);

        Vector3 scale = Global.instance.allBlocks.blockSize;
        m_cursor.transform.localPosition = new Vector3(pos.x * scale.x, pos.y * scale.y, pos.z * scale.z);
    }
}
