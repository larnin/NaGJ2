using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorPlaceBuilding : CursorBase
{
    GameObject m_buildingInstance; 

    BuildingType m_buildingType;
    int m_level;

    Rotation m_rotation = Rotation.rot_0;

    public void SetBuilding(BuildingType type, int level)
    {
        m_buildingType = type;
        m_level = level;

        LoadInstance();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                m_rotation = RotationEx.Sub(m_rotation, Rotation.rot_90);
            else m_rotation = RotationEx.Add(m_rotation, Rotation.rot_90);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Event<EnableSelectCursorEvent>.Broadcast(new EnableSelectCursorEvent());
            return;
        }

        if (Input.GetMouseButtonDown(0))
            PlaceBuilding();

        UpdateBuildingState();
    }

    void LoadInstance()
    {
        if (m_buildingInstance != null)
            Destroy(m_buildingInstance);

        var prefab = BuildingDataEx.GetBaseBuildingPrefab(m_buildingType, m_level);
        if (prefab == null)
            return;

        m_buildingInstance = Instantiate(prefab);
        m_buildingInstance.transform.parent = transform;
    }

    bool CanPlaceAt(Vector3Int pos)
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);

        if (level.level == null)
            return false;

        if (m_buildingType == BuildingType.Belt)
            return CanPlaceBelt(pos);
        else if (m_buildingType == BuildingType.Pipe)
            return CanPlacePipe(pos);
        else
        {
            var bounds = BuildingDataEx.GetBuildingBounds(m_buildingType, pos, m_rotation);

            for (int i = bounds.min.x; i < bounds.max.x; i++)
            {
                for (int k = bounds.min.z; k < bounds.max.z; k++)
                {
                    for (int j = bounds.min.y; j < bounds.max.y; j++)
                    {
                        var block = level.level.grid.GetBlock(new Vector3Int(i, j, k));

                        if (block.id != BlockType.air)
                            return false;

                        var building = level.level.buildingList.GetBuildingAt(new Vector3Int(i, j, k));
                        if (building != null)
                            return false;
                    }

                    var ground = level.level.grid.GetBlock(new Vector3Int(i, bounds.min.y - 1, k));

                    if (!BlockDataEx.CanPlaceBuildingOnBlock(ground.id))
                        return false;
                }
            }
        }
        return true;
    }

    bool CanPlaceBelt(Vector3Int pos)
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return false;

        var currentBlock = level.level.grid.GetBlock(pos);
        if (currentBlock.id != BlockType.air)
            return false;

        var downBlock = level.level.grid.GetBlock(new Vector3Int(pos.x, pos.y - 1, pos.z));
        if (BlockDataEx.CanPlaceBuildingOnBlock(downBlock.id))
            return true;

        if (downBlock.id == BlockType.groundSlope)
        {
            var blockRot = BlockDataEx.ExtractDataRotation(downBlock.data);
            return blockRot == m_rotation || blockRot == RotationEx.Add(m_rotation, Rotation.rot_180);
        }

        return false;
    }

    bool CanPlacePipe(Vector3Int pos)
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return false;

        var currentBlock = level.level.grid.GetBlock(pos);
        if (currentBlock.id != BlockType.air)
            return false;

        var downBlock = level.level.grid.GetBlock(new Vector3Int(pos.x, pos.y - 1, pos.z));
        if (BlockDataEx.CanPlaceBuildingOnBlock(downBlock.id))
            return true;

        NearMatrix3<bool> pipes = new NearMatrix3<bool>();
        level.level.buildingList.GetNearPipes(pos, pipes);

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

    void UpdateBuildingState()
    {
        var size = Global.instance.allBlocks.blockSize;

        var pos = GetCursorPosition();
        var realPos = new Vector3(pos.x * size.x, pos.y * size.y, pos.z * size.z);

        m_buildingInstance.transform.position = realPos;
        m_buildingInstance.transform.rotation = RotationEx.ToQuaternion(m_rotation);

        Color c = Color.green;
        if (!CanPlaceAt(pos))
            c = Color.red;
        else if (!HaveResources())
            c = Color.yellow;

        var renderers = m_buildingInstance.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            var mats = r.materials;
            foreach (var mat in mats)
                mat.color = c;
            r.materials = mats;
        }
    }

    void PlaceBuilding()
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);
        if (level.level == null)
            return;

        var pos = GetCursorPosition();
        if (!CanPlaceAt(pos))
            return;
        if (!HaveResources())
            return;

        BuildingInfos infos = new BuildingInfos();
        infos.team = Team.player;
        infos.level = m_level;
        infos.pos = pos;
        infos.rotation = m_rotation;
        var building = BuildingBase.Create(infos, level.level);
        level.level.buildingList.Add(building);
    }

    bool HaveResources()
    {
        return true;
    }

    private void OnDisable()
    {
        Destroy(m_buildingInstance);
    }
}
