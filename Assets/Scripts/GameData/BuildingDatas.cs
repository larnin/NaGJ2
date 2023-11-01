using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
    public Vector2Int size = new Vector2Int(1, 1);
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
    public Vector2Int size = new Vector2Int(1, 1);
}

[Serializable]
public class MiningCenterData
{
    public ResourceCostData cost;
    public GameObject prefab;
    public Vector2Int size = new Vector2Int(1, 1);
    public int mineDistance = 1;
}

[Serializable]
public class DrillData
{
    public GameObject prefab;
    public Vector2Int size = new Vector2Int(1, 1);
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

    public BeltData belt;
}
