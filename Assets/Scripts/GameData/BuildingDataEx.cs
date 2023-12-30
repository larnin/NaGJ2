using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        Vector3Int max = new Vector3Int((size.x - 1) - min.x, size.y, (size.z - 1) - min.z);

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
            default:
                return null;
        }
    }
}

