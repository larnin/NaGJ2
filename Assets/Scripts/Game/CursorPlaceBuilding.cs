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

        var colliders = m_buildingInstance.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;
    }

    bool CanPlaceAt(Vector3Int pos)
    {
        if (m_buildingType == BuildingType.Belt)
            return CanPlaceBelt(pos);
        else
        {
            var bounds = BuildingDataEx.GetBuildingBounds(m_buildingType, pos, m_rotation);

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

                        GetBuildingAtEvent eBuilding = new GetBuildingAtEvent(new Vector3Int(i, j, k));
                        Event<GetBuildingAtEvent>.Broadcast(eBuilding);
                        if (eBuilding.element != null)
                            return false;
                    }

                    GetBlockEvent eGround = new GetBlockEvent(new Vector3Int(i, bounds.min.y - 1, k));
                    Event<GetBlockEvent>.Broadcast(eGround);

                    if (!BlockDataEx.CanPlaceBuildingOnBlock(eGround.type))
                        return false;
                }
            }
        }
        return true;
    }

    bool CanPlaceBelt(Vector3Int pos)
    {
        var currentBlock = new GetBlockEvent(pos);
        Event<GetBlockEvent>.Broadcast(currentBlock);
        if (currentBlock.type != BlockType.air)
            return false;

        var downBlock = new GetBlockEvent(new Vector3Int(pos.x, pos.y - 1, pos.z));
        Event<GetBlockEvent>.Broadcast(downBlock);

        if (BlockDataEx.CanPlaceBuildingOnBlock(downBlock.type))
            return true;

        if (downBlock.type == BlockType.groundSlope)
        {
            var blockRot = BlockDataEx.ExtractDataRotation(downBlock.data);
            return blockRot == m_rotation || blockRot == RotationEx.Add(m_rotation, Rotation.rot_180);
        }

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
        var pos = GetCursorPosition();
        if (!CanPlaceAt(pos))
            return;
        if (!HaveResources())
            return;

        PlaceBuildingEvent buildingData = new PlaceBuildingEvent(pos, m_buildingType, m_level, m_rotation, Team.player);
        Event<PlaceBuildingEvent>.Broadcast(buildingData);
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
