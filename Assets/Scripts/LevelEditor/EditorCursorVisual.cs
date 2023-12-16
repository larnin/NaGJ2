using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorCursorVisual : MonoBehaviour
{
    enum CursorType
    {
        None,
        Block,
        Building,
    }

    [SerializeField] GameObject m_cursorPrefab = null;

    BlockType m_blockType = BlockType.air;
    BuildingType m_buildingType = BuildingType.Belt;
    int m_blockData = 0;
    Rotation m_rotation = Rotation.rot_0;

    SubscriberList m_subscriberList = new SubscriberList();

    GameObject m_blockCursor;
    GameObject m_buildingCursor;

    GameObject m_cursor = null;

    CursorType m_cursorType = CursorType.None;

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorCursorClickEvent>.Subscriber(OnClick));
        m_subscriberList.Add(new Event<EditorSetCursorBlockEvent>.Subscriber(SetType));
        m_subscriberList.Add(new Event<EditorGetCursorBlockEvent>.Subscriber(GetType));
        m_subscriberList.Add(new Event<EditorSetCursorBuildingEvent>.Subscriber(SetBuildingType));
        m_subscriberList.Add(new Event<EditorGetCursorBuildingEvent>.Subscriber(GetBuildingType));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        InitCursor();
    }

    private void Update()
    {
        EditorCurstorGetPosEvent posData = new EditorCurstorGetPosEvent();
        Event<EditorCurstorGetPosEvent>.Broadcast(posData);

        if (m_cursorType == CursorType.Block)
            UpdateForBlock(posData.pos, posData.blockPos);
        else if (m_cursorType == CursorType.Building)
            UpdateForBuilding(posData.pos, posData.blockPos);
    }

    void UpdateForBlock(Vector3Int pos, Vector3Int blockPos)
    {
        BlockType type = m_blockType;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            type = BlockType.air;

        Vector3Int outPos = Vector3Int.zero;
        bool valid = BlockDataEx.GetValidPos(type, blockPos, pos, out outPos);
        EditorGetBuildingAtEvent e = new EditorGetBuildingAtEvent(outPos);
        Event<EditorGetBuildingAtEvent>.Broadcast(e);
        if (e.ID != 0)
            valid = false;
        if (valid)
            SetCursorPosition(outPos, true);
        else SetCursorPosition(outPos, false);

        if (Input.GetKeyDown(KeyCode.R))
            m_rotation = RotationEx.Add(m_rotation, Rotation.rot_90);
    }

    void UpdateForBuilding(Vector3Int pos, Vector3Int blockPos)
    {
        if (Input.GetKeyDown(KeyCode.R))
            m_rotation = RotationEx.Add(m_rotation, Rotation.rot_90);

        SetCursorPosition(pos, true);

        UpdateBuildingCursorColor(pos, m_buildingType);
    }

    void OnClick(EditorCursorClickEvent e)
    {
        EditorCurstorGetPosEvent posData = new EditorCurstorGetPosEvent();
        Event<EditorCurstorGetPosEvent>.Broadcast(posData);

        if (m_cursorType == CursorType.Block)
            OnClickForBlock(e.clickType, posData.pos, posData.blockPos);
        else if (m_cursorType == CursorType.Building)
            OnClickForBuilding(e.clickType, posData.pos, posData.blockPos);
    }

    void OnClickForBlock(EditorCursorClickType click, Vector3Int pos, Vector3Int blockPos)
    {
        Vector3Int outPos = Vector3Int.zero;

        if (click == EditorCursorClickType.leftClick)
        {
            bool valid = BlockDataEx.GetValidPos(m_blockType, blockPos, pos, out outPos);
            if (!valid)
                return;

            EditorGetBuildingAtEvent e = new EditorGetBuildingAtEvent(outPos);
            Event<EditorGetBuildingAtEvent>.Broadcast(e);
            if (e.ID != 0)
                return;

            BlockDataEx.SetBlock(m_blockType, m_rotation, m_blockData, outPos);
        }
        else if (click == EditorCursorClickType.rightClick)
        {
            if (!DeleteBuilding(pos, blockPos))
            {
                bool valid = BlockDataEx.GetValidPos(BlockType.air, blockPos, pos, out outPos);
                if (!valid)
                    return;
                BlockDataEx.SetBlock(BlockType.air, outPos);
            }
        }
    }

    void OnClickForBuilding(EditorCursorClickType click, Vector3Int pos, Vector3Int blockPos)
    {
        if (click == EditorCursorClickType.leftClick)
        {
            if (!CanPlaceBuilding(pos, m_buildingType))
                return;

            Event<EditorPlaceBuildingEvent>.Broadcast(new EditorPlaceBuildingEvent(pos, m_buildingType, m_rotation, Team.player));
        }
        else if (click == EditorCursorClickType.rightClick)
            DeleteBuilding(pos, blockPos);
    }

    bool DeleteBuilding(Vector3Int pos, Vector3Int blockPos)
    {
        return false;
    }

    void SetType(EditorSetCursorBlockEvent e)
    {
        m_cursorType = CursorType.Block;

        m_blockType = e.type;
        m_blockData = e.blockData;

        SetCursorPosition(Vector3Int.zero, false);
    }

    void GetType(EditorGetCursorBlockEvent e)
    {
        e.type = m_blockType;
        e.blockData = m_blockData;
        e.rotation = m_rotation;
    }

    void SetBuildingType(EditorSetCursorBuildingEvent e)
    {
        m_cursorType = CursorType.Building;

        m_buildingType = e.type;

        UpdateBuildingCursor();

        SetCursorPosition(Vector3Int.zero, false);
    }

    void GetBuildingType(EditorGetCursorBuildingEvent e)
    {
        e.type = m_buildingType;
        e.rotation = m_rotation;
    }

    void InitCursor()
    {
        if (m_cursorPrefab == null)
            return;

        m_blockCursor = Instantiate(m_cursorPrefab);
        m_blockCursor.transform.parent = transform;
        m_blockCursor.transform.localPosition = Vector3.zero;
        m_blockCursor.SetActive(false);
    }

    void SetCursorPosition(Vector3Int pos, bool visible)
    {
        bool blockCursorVisible = false;
        bool buildingCursorVisible = false;

        switch(m_cursorType)
        {
            case CursorType.None:
                m_cursor = null;
                break;
            case CursorType.Block:
                m_cursor = m_blockCursor;
                blockCursorVisible = visible;
                break;
            case CursorType.Building:
                m_cursor = m_buildingCursor;
                buildingCursorVisible = visible;
                break;
            default:
                Debug.LogWarning("Unknow cursor type " + m_cursorType);
                break;
        }

        if (m_blockCursor != null)
            m_blockCursor.SetActive(blockCursorVisible);
        if (m_buildingCursor != null)
            m_buildingCursor.SetActive(buildingCursorVisible);

        if (m_cursor != null)
        {
            Vector3 scale = Global.instance.allBlocks.blockSize;
            m_cursor.transform.localPosition = new Vector3(pos.x * scale.x, pos.y * scale.y, pos.z * scale.z);
        }
    }

    void UpdateBuildingCursor()
    {
        if (m_buildingCursor != null)
            Destroy(m_buildingCursor);

        var prefab = BuildingDataEx.GetBaseBuildingPrefab(m_buildingType);

        m_buildingCursor = Instantiate(prefab);
        m_buildingCursor.transform.parent = transform;
        m_buildingCursor.transform.localPosition = Vector3.zero;
        m_buildingCursor.SetActive(false);

        var colliders = m_buildingCursor.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;
    }

    void UpdateBuildingCursorColor(Vector3Int pos, BuildingType type)
    {
        if (m_buildingCursor == null)
            return;

        bool canPlace = CanPlaceBuilding(pos, type);
        Color c = canPlace ? new Color(0, 1, 0) : new Color(1, 0, 0);

        var renderers = m_buildingCursor.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            var mats = r.materials;
            foreach (var mat in mats)
                mat.color = c;
            r.materials = mats;
        }

        m_buildingCursor.transform.localRotation = RotationEx.ToQuaternion(m_rotation);
    }

    bool CanPlaceBuilding(Vector3Int pos, BuildingType type)
    {
        var bounds = BuildingDataEx.GetBuildingBounds(m_buildingType, pos, m_rotation);

        for(int i = bounds.min.x; i < bounds.max.x; i++)
        {
            for (int k = bounds.min.z; k < bounds.max.z; k++)
            {
                for (int j = bounds.min.y; j < bounds.max.y; j++)
                {
                    EditorGetBlockEvent eBlock = new EditorGetBlockEvent(new Vector3Int(i, j, k));
                    Event<EditorGetBlockEvent>.Broadcast(eBlock);

                    if (eBlock.type != BlockType.air)
                        return false;

                    EditorGetBuildingAtEvent eBuilding = new EditorGetBuildingAtEvent(new Vector3Int(i, j, k));
                    Event<EditorGetBuildingAtEvent>.Broadcast(eBuilding);
                    if (eBuilding.ID != 0)
                        return false;
                }

                EditorGetBlockEvent eGround = new EditorGetBlockEvent(new Vector3Int(i, bounds.min.y - 1, k));
                Event<EditorGetBlockEvent>.Broadcast(eGround);

                bool groundValid = BlockDataEx.CanPlaceBuildingOnBlock(eGround.type);

                if (type == BuildingType.Belt && eGround.type == BlockType.groundSlope)
                    groundValid = true;

                if (!groundValid)
                    return false;
            }
        }
        return true;
    }
}
