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
    Team m_buildingTeam = Team.player;
    int m_blockData = 0;
    int m_buildingLevel = 0;
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
        if (e.building != null)
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

        UpdateBuildingCursorColor(pos, m_buildingType, m_rotation);
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

            if (m_blockType == BlockType.air)
                RemoveBlock(outPos);
            else
            {
                EditorGetBuildingAtEvent e = new EditorGetBuildingAtEvent(outPos);
                Event<EditorGetBuildingAtEvent>.Broadcast(e);
                if (e.building != null)
                    return;

                BlockDataEx.SetBlock(m_blockType, m_rotation, m_blockData, outPos);
            }
        }
        else if (click == EditorCursorClickType.rightClick)
        {
            if (!DeleteBuilding(pos, blockPos))
            {
                bool valid = BlockDataEx.GetValidPos(BlockType.air, blockPos, pos, out outPos);
                if (!valid)
                    return;
                RemoveBlock(outPos);
            }
        }
    }

    void RemoveBlock(Vector3Int pos)
    {
        EditorGetBuildingAtEvent e = new EditorGetBuildingAtEvent(new Vector3Int(pos.x, pos.y + 1, pos.z));
        Event<EditorGetBuildingAtEvent>.Broadcast(e);
        if (e.building != null)
            return;

        BlockDataEx.SetBlock(BlockType.air, pos);
    }

    void OnClickForBuilding(EditorCursorClickType click, Vector3Int pos, Vector3Int blockPos)
    {
        if (click == EditorCursorClickType.leftClick)
        {
            if (!CanPlaceBuilding(pos, m_buildingType, m_rotation))
                return;

            Event<EditorPlaceBuildingEvent>.Broadcast(new EditorPlaceBuildingEvent(pos, m_buildingType, m_rotation, m_buildingTeam, m_buildingLevel));
        }
        else if (click == EditorCursorClickType.rightClick)
            DeleteBuilding(pos, blockPos);
    }

    bool DeleteBuilding(Vector3Int pos, Vector3Int blockPos)
    {
        EditorGetBuildingAtEvent data = new EditorGetBuildingAtEvent(pos);
        Event<EditorGetBuildingAtEvent>.Broadcast(data);
        if(data.building == null)
        {
            data.pos = blockPos;
            Event<EditorGetBuildingAtEvent>.Broadcast(data);
            if (data.building == null)
                return false;
        }

        EditorRemoveBuildingEvent removeData = new EditorRemoveBuildingEvent(data.building.ID);
        Event<EditorRemoveBuildingEvent>.Broadcast(removeData);
        return true;
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
        m_buildingLevel = e.level;
        m_buildingTeam = e.team;

        UpdateBuildingCursor();

        SetCursorPosition(Vector3Int.zero, false);
    }

    void GetBuildingType(EditorGetCursorBuildingEvent e)
    {
        e.type = m_buildingType;
        e.level = m_buildingLevel;
        e.team = m_buildingTeam;
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

    void UpdateBuildingCursorColor(Vector3Int pos, BuildingType type, Rotation rot)
    {
        if (m_buildingCursor == null)
            return;

        bool canPlace = CanPlaceBuilding(pos, type, rot);
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

    bool CanPlaceBuilding(Vector3Int pos, BuildingType type, Rotation rotation)
    {
        if (type == BuildingType.Belt)
            return CanPlaceBelt(pos, rotation);
        else if (type == BuildingType.Pipe)
            return CanPlacePipe(pos, rotation);
        else
        {
            var bounds = BuildingDataEx.GetBuildingBounds(m_buildingType, pos, m_rotation, m_buildingLevel);

            for (int i = bounds.min.x; i < bounds.max.x; i++)
            {
                for (int k = bounds.min.z; k < bounds.max.z; k++)
                {
                    for (int j = bounds.min.y; j < bounds.max.y; j++)
                    {
                        GetBlockEvent eBlock = new GetBlockEvent(new Vector3Int(i, j, k));
                        Event<GetBlockEvent>.Broadcast(eBlock);

                        if (eBlock.type != BlockType.air)
                            return false;

                        EditorGetBuildingAtEvent eBuilding = new EditorGetBuildingAtEvent(new Vector3Int(i, j, k));
                        Event<EditorGetBuildingAtEvent>.Broadcast(eBuilding);
                        if (eBuilding.building != null)
                            return false;
                    }

                    GetBlockEvent eGround = new GetBlockEvent(new Vector3Int(i, bounds.min.y - 1, k));
                    Event<GetBlockEvent>.Broadcast(eGround);

                    bool groundValid = BlockDataEx.CanPlaceBuildingOnBlock(eGround.type);

                    if (type == BuildingType.Belt && eGround.type == BlockType.groundSlope)
                        groundValid = true;

                    if (!groundValid)
                        return false;
                }
            }
        }
        return true;
    }

    bool CanPlaceBelt(Vector3Int pos, Rotation rot)
    {
        var currentBlock = new GetBlockEvent(pos);
        Event<GetBlockEvent>.Broadcast(currentBlock);
        if (currentBlock.type != BlockType.air)
            return false;

        var downBlock = new GetBlockEvent(new Vector3Int(pos.x, pos.y - 1, pos.z));
        Event<GetBlockEvent>.Broadcast(downBlock);

        if (BlockDataEx.CanPlaceBuildingOnBlock(downBlock.type))
            return true;

        if(downBlock.type == BlockType.groundSlope)
        {
            var blockRot = BlockDataEx.ExtractDataRotation(downBlock.data);
            return blockRot == rot || blockRot == RotationEx.Add(rot, Rotation.rot_180);
        }

        return false;
    }

    bool CanPlacePipe(Vector3Int pos, Rotation rot)
    {
        var currentBlock = new GetBlockEvent(pos);
        Event<GetBlockEvent>.Broadcast(currentBlock);
        if (currentBlock.type != BlockType.air)
            return false;

        var downBlock = new GetBlockEvent(new Vector3Int(pos.x, pos.y - 1, pos.z));
        Event<GetBlockEvent>.Broadcast(downBlock);

        if (BlockDataEx.CanPlaceBuildingOnBlock(downBlock.type))
            return true;

        GetNearPipesEvent pipes = new GetNearPipesEvent(pos);
        Event<GetNearPipesEvent>.Broadcast(pipes);

        if (pipes.matrix.Get(0, 1, 0))
            return true;
        if (pipes.matrix.Get(0, -1, 0))
            return true;
        if (pipes.matrix.Get(1, 0, 0))
            return true;
        if (pipes.matrix.Get(-1, 0, 0))
            return true;
        if (pipes.matrix.Get(0, 0, 1))
            return true;
        if (pipes.matrix.Get(0, 0, -1))
            return true;

        return false;
    }
}
