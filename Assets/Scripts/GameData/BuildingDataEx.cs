using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BuildingDataEx
{
    public static Vector2Int GetBuildingSize(BuildingType type, int level = 0)
    {
        var buildings = Global.instance.allBuildings;
        Vector2Int defaultSize = new Vector2Int(1, 1);

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
            default:
                return defaultSize;
        }
    }
}

