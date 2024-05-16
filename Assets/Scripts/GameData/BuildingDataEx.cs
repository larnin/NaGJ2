using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public struct SimpleBeltInfos
{
    public bool haveBelt;
    public Rotation rotation;
}

public static class BuildingDataEx
{
    public static Vector3Int GetBuildingSize(BuildingType type, int level = 0)
    {
        var buildings = Global.instance.allBuildings;
        Vector3Int defaultSize = new Vector3Int(1, 1, 1);

        switch(type)
        {
            case BuildingType.Tower0:
                if (level >= 0 && level < buildings.tower0.levels.Count)
                    return buildings.tower0.levels[level].size;
                return defaultSize;
            case BuildingType.Tower1:
                if (level >= 0 && level < buildings.tower1.levels.Count)
                    return buildings.tower1.levels[level].size;
                return defaultSize;
            case BuildingType.Tower2:
                if (level >= 0 && level < buildings.tower2.levels.Count)
                    return buildings.tower2.levels[level].size;
                return defaultSize;
            case BuildingType.OperationCenter:
                return buildings.operationCenter.size;
            case BuildingType.BigMiningCenter:
                return buildings.bigMiningCenter.size;
            case BuildingType.MediumMiningCenter:
                return buildings.mediumMiningCenter.size;
            case BuildingType.SmallMiningCenter:
                return buildings.smallMiningCenter.size;
            case BuildingType.Drill:
                return buildings.drill.size;
            default:
                return defaultSize;
        }
    }

    public static BoundsInt GetBuildingBounds(BuildingType type, Vector3Int pos, Rotation rot, int level = 0)
    {
        var size = GetBuildingSize(type, level);

        Vector3Int min = new Vector3Int((size.x - 1) / 2, 0, (size.z - 1) / 2);
        Vector3Int max = new Vector3Int((size.x - 1) - min.x, size.y - 1, (size.z - 1) - min.z);

        int count = (int)rot;
        for (int i = 0; i < count; i++)
        {
            int temp = min.x;
            min.x = max.z;
            max.z = max.x;
            max.x = min.z;
            min.z = temp;
        }

        return new BoundsInt(pos - min, max + min + Vector3Int.one);
    }

    public static GameObject GetBaseBuildingPrefab(BuildingType type, int level = 0)
    {
        var buildings = Global.instance.allBuildings;

        switch (type)
        {
            case BuildingType.Tower0:
                if (level >= 0 && level < buildings.tower0.levels.Count)
                    return buildings.tower0.levels[level].prefab;
                return null;
            case BuildingType.Tower1:
                if (level >= 0 && level < buildings.tower1.levels.Count)
                    return buildings.tower1.levels[level].prefab;
                return null;
            case BuildingType.Tower2:
                if (level >= 0 && level < buildings.tower2.levels.Count)
                    return buildings.tower2.levels[level].prefab;
                return null;
            case BuildingType.OperationCenter:
                return buildings.operationCenter.prefab;
            case BuildingType.BigMiningCenter:
                return buildings.bigMiningCenter.prefab;
            case BuildingType.MediumMiningCenter:
                return buildings.mediumMiningCenter.prefab;
            case BuildingType.SmallMiningCenter:
                return buildings.smallMiningCenter.prefab;
            case BuildingType.Drill:
                return buildings.drill.prefab;
            case BuildingType.Belt:
                return buildings.belt.forwardPrefab;
            case BuildingType.Pipe:
                return buildings.pipe.ground.pipe_I;
            default:
                return null;
        }
    }

    public static ResourceCostData GetCost(BuildingType type, int level = 0)
    {
        var buildings = Global.instance.allBuildings;

        switch (type)
        {
            case BuildingType.Tower0:
                if (level >= 0 && level < buildings.tower0.levels.Count)
                    return buildings.tower0.levels[level].cost;
                return null;
            case BuildingType.Tower1:
                if (level >= 0 && level < buildings.tower1.levels.Count)
                    return buildings.tower1.levels[level].cost;
                return null;
            case BuildingType.Tower2:
                if (level >= 0 && level < buildings.tower2.levels.Count)
                    return buildings.tower2.levels[level].cost;
                return null;
            case BuildingType.BigMiningCenter:
                return buildings.bigMiningCenter.cost;
            case BuildingType.MediumMiningCenter:
                return buildings.mediumMiningCenter.cost;
            case BuildingType.SmallMiningCenter:
                return buildings.smallMiningCenter.cost;
            case BuildingType.Belt:
                return buildings.belt.cost;
            case BuildingType.Pipe:
                return buildings.pipe.cost;
            default:
                return null;
        }
    }

    public static string GetName(BuildingType type)
    {
        var buildings = Global.instance.allBuildings;

        switch (type)
        {
            case BuildingType.Tower0:
                return buildings.tower0.name;
            case BuildingType.Tower1:
                return buildings.tower1.name;
            case BuildingType.Tower2:
                return buildings.tower2.name;
            case BuildingType.OperationCenter:
                return buildings.operationCenter.name;
            case BuildingType.BigMiningCenter:
                return buildings.bigMiningCenter.name;
            case BuildingType.MediumMiningCenter:
                return buildings.mediumMiningCenter.name;
            case BuildingType.SmallMiningCenter:
                return buildings.smallMiningCenter.name;
            case BuildingType.Belt:
                return buildings.belt.name;
            case BuildingType.Pipe:
                return buildings.pipe.name;
            default:
                return "";
        }
    }

    public static string GetDescription(BuildingType type)
    {
        var buildings = Global.instance.allBuildings;

        switch (type)
        {
            case BuildingType.Tower0:
                return buildings.tower0.description;
            case BuildingType.Tower1:
                return buildings.tower1.description;
            case BuildingType.Tower2:
                return buildings.tower2.description;
            case BuildingType.OperationCenter:
                return buildings.operationCenter.description;
            case BuildingType.BigMiningCenter:
                return buildings.bigMiningCenter.description;
            case BuildingType.MediumMiningCenter:
                return buildings.mediumMiningCenter.description;
            case BuildingType.SmallMiningCenter:
                return buildings.smallMiningCenter.description;
            case BuildingType.Belt:
                return buildings.belt.description;
            case BuildingType.Pipe:
                return buildings.pipe.description;
            default:
                return "";
        }
    }

    public static GameObject InstantiateBelt(Vector3Int pos, NearMatrix3<SimpleBlock> blocks, NearMatrix3<SimpleBeltInfos> belts, out BeltDirection direction, Transform parent = null)
    {
        GameObject obj = null;
        GameObject prefab = null;

        direction = BeltDirection.Horizontal;

        var rot = belts.Get(0, 0, 0).rotation;
        bool isSlope = blocks.Get(0, -1, 0).id == BlockType.groundSlope;

        var beltsData = Global.instance.allBuildings.belt;

        if(isSlope)
        {
            var slopeRot = BlockDataEx.ExtractDataRotation(blocks.Get(0, -1, 0).data);

            if (rot == slopeRot)
            {
                prefab = beltsData.upPrefab;
                direction = BeltDirection.Up;
            }
            else
            {
                prefab = beltsData.downPrefab;
                direction = BeltDirection.Down;
            }
        }
        else
        {
            bool isBack = IsBelt(blocks, belts, rot);
            bool isLeft = IsBelt(blocks, belts, RotationEx.Add(rot, Rotation.rot_90));
            bool isRight = IsBelt(blocks, belts, RotationEx.Add(rot, Rotation.rot_270));

            if (isBack)
                prefab = beltsData.forwardPrefab;
            else if (isLeft && !isRight)
                prefab = beltsData.rightPrefab;
            else if (isRight && !isLeft)
                prefab = beltsData.leftPrefab;
            else prefab = beltsData.forwardPrefab;
        }

        if (prefab == null)
            prefab = beltsData.forwardPrefab;

        if (prefab != null)
        {
            obj = GameObject.Instantiate(prefab);

            var size = Global.instance.allBlocks.blockSize;
            var realPos = new Vector3(size.x * pos.x, size.y * pos.y, size.z * pos.z);

            if(parent != null)
            {
                obj.transform.parent = parent;
                obj.transform.localPosition = realPos;
                obj.transform.localRotation = RotationEx.ToQuaternion(rot);
            }
            else
            {
                obj.transform.position = realPos;
                obj.transform.rotation = RotationEx.ToQuaternion(rot);
            }
        }

        return obj;
    }

    static bool IsBelt(NearMatrix3<SimpleBlock> blocks, NearMatrix3<SimpleBeltInfos> belts, Rotation rot)
    {
        var dir = RotationEx.ToVector3Int(rot);

        var belt = belts.Get(-dir.x, 0, -dir.z);
        if(!belt.haveBelt)
        {
            belt = belts.Get(-dir.x, 1, -dir.z);
            if (!belt.haveBelt)
                return false;

            var block = blocks.Get(-dir.x, 0, -dir.z);
            if (block.id != BlockType.groundSlope)
                return false;

            var blockRot = BlockDataEx.ExtractDataRotation(block.data);
            if (RotationEx.Add(blockRot, Rotation.rot_180) != rot)
                return false;

            return belt.rotation == rot;
        }
        else
        {
            var block = blocks.Get(-dir.x, -1, -dir.z); 
            var blockRot = BlockDataEx.ExtractDataRotation(block.data);
            if (block.id == BlockType.groundSlope)
            {
                if (blockRot != rot)
                    return false;

                return belt.rotation == rot;
            }
            else return belt.rotation == rot; 
        }
    }
}

