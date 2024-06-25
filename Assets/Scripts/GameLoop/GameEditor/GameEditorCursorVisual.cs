using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum EditorCursorType
{
    None,
    Block,
    Building,
}

public class GameEditorCursorVisual : MonoBehaviour
{
    [SerializeField] GameObject m_cursorPrefab = null;

    BlockType m_blockType = BlockType.air;
    int m_blockData = 0;
    BuildingType m_buildingType = BuildingType.Belt;
    int m_buildingLevel = 0;
    Rotation m_rotation = Rotation.rot_0;

    SubscriberList m_subscriberList = new SubscriberList();

    GameObject m_blockCursor;
    GameObject m_buildingCursor;

    GameObject m_cursor = null;

    EditorCursorType m_cursorType = EditorCursorType.None;

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorCursorClickEvent>.Subscriber(OnClick));
        m_subscriberList.Add(new Event<EditorSetCursorBlockEvent>.Subscriber(SetType));
        m_subscriberList.Add(new Event<EditorGetCursorBlockEvent>.Subscriber(GetType));
        m_subscriberList.Add(new Event<EditorSetCursorBuildingEvent>.Subscriber(SetBuildingType));
        m_subscriberList.Add(new Event<EditorGetCursorBuildingEvent>.Subscriber(GetBuildingType));
        m_subscriberList.Add(new Event<EditorGetCursorTypeEvent>.Subscriber(GetCursorType));

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

        if (m_cursorType == EditorCursorType.Block)
            UpdateForBlock(posData.pos, posData.blockPos);
        else if (m_cursorType == EditorCursorType.Building)
            UpdateForBuilding(posData.pos, posData.blockPos);
    }

    void UpdateForBlock(Vector3Int pos, Vector3Int blockPos)
    {
        BlockType type = m_blockType;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            type = BlockType.air;

        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return;

        Vector3Int outPos = Vector3Int.zero;
        bool valid = BlockDataEx.GetValidPos(getLevel.level, type, blockPos, pos, out outPos);

        var building = getLevel.level.buildingList.GetBuildingAt(outPos);
        if (building != null)
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

        if (m_cursorType == EditorCursorType.Block)
            OnClickForBlock(e.clickType, posData.pos, posData.blockPos);
        else if (m_cursorType == EditorCursorType.Building)
            OnClickForBuilding(e.clickType, posData.pos, posData.blockPos);
    }

    void OnClickForBlock(EditorCursorClickType click, Vector3Int pos, Vector3Int blockPos)
    {
        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return;

        Vector3Int outPos = Vector3Int.zero;

        if (click == EditorCursorClickType.leftClick)
        {
            bool valid = BlockDataEx.GetValidPos(getLevel.level, m_blockType, blockPos, pos, out outPos);
            if (!valid)
                return;

            if (m_blockType == BlockType.air)
                RemoveBlock(outPos);
            else
            {
                var building = getLevel.level.buildingList.GetBuildingAt(outPos);
                if (building != null)
                    return;

                BlockDataEx.SetBlock(getLevel.level, m_blockType, m_rotation, m_blockData, outPos);
            }
        }
        else if (click == EditorCursorClickType.rightClick)
        {
            if (!DeleteBuilding(pos, blockPos))
            {
                bool valid = BlockDataEx.GetValidPos(getLevel.level, BlockType.air, blockPos, pos, out outPos);
                if (!valid)
                    return;
                RemoveBlock(outPos);
            }
        }
    }

    void RemoveBlock(Vector3Int pos)
    {
        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return;

        var building = getLevel.level.buildingList.GetBuildingAt(new Vector3Int(pos.x, pos.y + 1, pos.z));
        if (building != null)
            return;

        BlockDataEx.SetBlock(BlockType.air, pos);
    }

    void OnClickForBuilding(EditorCursorClickType click, Vector3Int pos, Vector3Int blockPos)
    {
        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return;

        if (click == EditorCursorClickType.leftClick)
        {
            if (!CanPlaceBuilding(pos, m_buildingType, m_rotation))
                return;

            var infos = new BuildingInfos();
            infos.pos = pos;
            infos.buildingType = m_buildingType;
            infos.rotation = m_rotation;
            infos.team = Team.player;

            var building = BuildingBase.Create(infos, getLevel.level); ;

            getLevel.level.buildingList.Add(building);
        }
        else if (click == EditorCursorClickType.rightClick)
            DeleteBuilding(pos, blockPos);
    }

    bool DeleteBuilding(Vector3Int pos, Vector3Int blockPos)
    {
        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return false;

        var building = getLevel.level.buildingList.GetBuildingAt(pos);
        if (building == null)
        {
            building = getLevel.level.buildingList.GetBuildingAt(pos);
            if (building == null)
                return false;
        }

        getLevel.level.buildingList.Remove(building.GetID());

        return true;
    }

    void SetType(EditorSetCursorBlockEvent e)
    {
        m_cursorType = EditorCursorType.Block;

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
        m_cursorType = EditorCursorType.Building;

        m_buildingType = e.type;

        UpdateBuildingCursor();

        SetCursorPosition(Vector3Int.zero, false);
    }

    void GetBuildingType(EditorGetCursorBuildingEvent e)
    {
        e.type = m_buildingType;
        e.rotation = m_rotation;
        e.level = m_buildingLevel;
    }

    void GetCursorType(EditorGetCursorTypeEvent e)
    {
        e.cursorType = m_cursorType;
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

        switch (m_cursorType)
        {
            case EditorCursorType.None:
                m_cursor = null;
                break;
            case EditorCursorType.Block:
                m_cursor = m_blockCursor;
                blockCursorVisible = visible;
                break;
            case EditorCursorType.Building:
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
        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return false;

        if (type == BuildingType.Belt)
            return CanPlaceBelt(pos, rotation);
        else if (type == BuildingType.Pipe)
            return CanPlacePipe(pos, rotation);
        else
        {
            var bounds = BuildingDataEx.GetBuildingBounds(m_buildingType, pos, m_rotation);

            for (int i = bounds.min.x; i < bounds.max.x; i++)
            {
                for (int k = bounds.min.z; k < bounds.max.z; k++)
                {
                    for (int j = bounds.min.y; j < bounds.max.y; j++)
                    {
                        var eBlock = getLevel.level.grid.GetBlock(new Vector3Int(i, j, k));
                        if (eBlock.id != BlockType.air)
                            return false;

                        var eBuilding = getLevel.level.buildingList.GetBuildingAt(new Vector3Int(i, j, k));
                        if (eBuilding == null)
                            return false;
                    }

                    var eGround = getLevel.level.grid.GetBlock(new Vector3Int(i, bounds.min.y - 1, k));

                    bool groundValid = BlockDataEx.CanPlaceBuildingOnBlock(eGround.id);

                    if (type == BuildingType.Belt && eGround.id == BlockType.groundSlope)
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
        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return false;

        var currentBlock = getLevel.level.grid.GetBlock(pos);
        if (currentBlock.id != BlockType.air)
            return false;

        var downBlock = getLevel.level.grid.GetBlock(new Vector3Int(pos.x, pos.y - 1, pos.z));

        if (BlockDataEx.CanPlaceBuildingOnBlock(downBlock.id))
            return true;

        if (downBlock.id == BlockType.groundSlope)
        {
            var blockRot = BlockDataEx.ExtractDataRotation(downBlock.data);
            return blockRot == rot || blockRot == RotationEx.Add(rot, Rotation.rot_180);
        }

        return false;
    }

    bool CanPlacePipe(Vector3Int pos, Rotation rot)
    {
        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return false;

        var currentBlock = getLevel.level.grid.GetBlock(pos);
        if (currentBlock.id != BlockType.air)
            return false;

        var downBlock = getLevel.level.grid.GetBlock(new Vector3Int(pos.x, pos.y - 1, pos.z));
        if (BlockDataEx.CanPlaceBuildingOnBlock(downBlock.id))
            return true;

        NearMatrix3<bool> pipes = new NearMatrix3<bool>();
        getLevel.level.buildingList.GetNearPipes(pos, pipes);

        if (pipes.Get(0, 1, 0))
            return true;
        if (pipes.Get(0, -1, 0))
            return true;
        if (pipes.Get(1, 0, 0))
            return true;
        if (pipes.Get(-1, 0, 0))
            return true;
        if (pipes.Get(0, 0, 1))
            return true;
        if (pipes.Get(0, 0, -1))
            return true;

        return false;
    }
}
