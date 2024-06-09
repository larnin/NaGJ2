using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NLocalization;

public enum BuildingUpdateType
{
    added,
    removed,
    updated,
}

public enum BuildingPortDirection
{
    input,
    output,
    inout,
}

public enum ResourceFilterType
{
    everything,
    isOneOf,
    notOneOf,
}

[Serializable]
public class ResourceFilter
{
    public ResourceFilterType type;
    public bool liquid;
    [HideIf("type", ResourceFilterType.everything)]
    public List<ResourceType> resources = new List<ResourceType>();

    public ResourceFilter Copy()
    {
        var r = new ResourceFilter();
        r.type = type;
        r.resources = resources.ToList();
        return r;
    }

    public bool IsValid(ResourceType resource)
    {
        if (ResourceDataEx.IsLiquid(resource) != liquid)
            return false;

        if (type == ResourceFilterType.everything)
            return true;

        bool isInList = resources != null && resources.Contains(resource);
        if (isInList)
            return type == ResourceFilterType.isOneOf;
        else return type == ResourceFilterType.notOneOf;
    }
}

[Serializable]
public class BuildingContainerData
{
    public ResourceFilter filter;
    public float count;
    public List<BuildingPortData> ports;
}

[Serializable]
public class BuildingPortData
{
    public Vector3Int pos;
    public Rotation rotation;
    public BuildingPortDirection direction;
}

public enum Team
{
    player,
    neutral,
    ennemy1,
    ennemy2,
    ennemy3,
}

public enum BuildingType
{
    Tower0,
    Tower1,
    Tower2,
    OperationCenter,
    BigMiningCenter,
    MediumMiningCenter,
    SmallMiningCenter,
    Drill,
    Belt,
    Pipe,
}

[Serializable]
public class OneTowerData
{
    public ResourceCostData cost;
    public GameObject prefab;
    public Vector3Int size = new Vector3Int(1, 1, 1);
}

[Serializable]
public class TowerData
{
    public LocText name;
    public LocText description;
    public List<OneTowerData> levels;
}

[Serializable]
public class OperationCenterData
{
    public LocText name;
    public LocText description;
    public GameObject prefab;
    public Vector3Int size = new Vector3Int(1, 1, 1);
    public List<BuildingContainerData> containers;
}

[Serializable]
public class MiningCenterData
{
    public LocText name;
    public LocText description;
    public ResourceCostData cost;
    public GameObject prefab;
    public Vector3Int size = new Vector3Int(1, 1, 1);
    public int mineDistance = 1;
    public List<BuildingContainerData> containers;
}

[Serializable]
public class DrillData
{
    public GameObject prefab;
    public Vector3Int size = new Vector3Int(1, 1, 1);
}

[Serializable]
public class BeltData
{
    public LocText name;
    public LocText description;
    public ResourceCostData cost;
    public GameObject forwardPrefab;
    public GameObject leftPrefab;
    public GameObject rightPrefab;
    public GameObject upPrefab;
    public GameObject downPrefab;
}

[Serializable]
public class OnePipeData
{
    public GameObject pipe_I;
    public GameObject pipe_L;
    public GameObject pipe_T;
    public GameObject pipe_X;
}

[Serializable]
public class PipeData
{
    public LocText name;
    public LocText description;
    public ResourceCostData cost;
    public OnePipeData ground;
    public OnePipeData groundUp;
    public OnePipeData air;
    public OnePipeData airUp;
    public OnePipeData airDown;
    public OnePipeData airVertical;
    public GameObject vertical;
    public GameObject verticalSide;
    public GameObject goUp;
    public GameObject airGoUp;
    public GameObject airGoDown;
    public GameObject end;
}

[Serializable]
public class AllBuildings
{
    public TowerData tower0;
    public TowerData tower1;
    public TowerData tower2;

    public OperationCenterData operationCenter;

    public MiningCenterData bigMiningCenter;
    public MiningCenterData mediumMiningCenter;
    public MiningCenterData smallMiningCenter;
    public DrillData drill;

    public BeltData belt;

    public PipeData pipe;
}
