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
}

[Serializable]
public class BuildingOnePortData
{
    public Vector3Int pos;
    public Rotation rotation;
    public BuildingPortDirection direction;
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
    public List<OneTowerData> levels;
}

[Serializable]
public class OperationCenterData
{
    public GameObject prefab;
    public Vector3Int size = new Vector3Int(1, 1, 1);
}

[Serializable]
public class MiningCenterData
{
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
    public GameObject forwardPrefab;
    public GameObject leftPrefab;
    public GameObject rightPrefab;
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
