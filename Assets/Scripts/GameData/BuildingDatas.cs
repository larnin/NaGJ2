using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        if (type == ResourceFilterType.everything)
            return true;

        bool isInList = resources != null && resources.Contains(resource);
        if (isInList)
            return type == ResourceFilterType.isOneOf;
        else return type == ResourceFilterType.notOneOf;
    }
}

[Serializable]
public class BuildingContainer
{
    [HideInInspector] public int id; //building ID
    [HideInInspector] public int index; //container index in building

    public BuildingPortDirection direction;
    public ResourceFilter filter;
    public int count;
}

[Serializable]
public class BuildingOnePortData
{
    public Vector3Int pos;
    public Rotation rotation;
    public BuildingPortDirection direction;
    public int containerIndex;
}

[Serializable]
public class BuildingOneBeltData
{
    public Vector3Int pos;
    public Rotation rotation;
    public int verticalOffset;
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
    public string name;
    [Multiline]
    public string description;
    public List<OneTowerData> levels;
}

[Serializable]
public class OperationCenterData
{
    public string name;
    [Multiline]
    public string description;
    public GameObject prefab;
    public Vector3Int size = new Vector3Int(1, 1, 1);
}

[Serializable]
public class MiningCenterData
{
    public string name;
    [Multiline]
    public string description;
    public ResourceCostData cost;
    public GameObject prefab;
    public Vector3Int size = new Vector3Int(1, 1, 1);
    public int mineDistance = 1;
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
    public string name;
    [Multiline]
    public string description;
    public ResourceCostData cost;
    public GameObject forwardPrefab;
    public GameObject leftPrefab;
    public GameObject rightPrefab;
    public GameObject upPrefab;
    public GameObject downPrefab;
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
}
