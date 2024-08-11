using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum EntityType
{
    TestGround,
    TestAir,
    TestWater,
}

public enum EntityBehaviourType
{
    Ground,
    Air,
    Water,
}

[Serializable]
public class OneEntityData
{
    public EntityType type;
    public EntityBehaviourType behaviour;
    public GameObject prefab;
    public float maxLife;
    public float moveSpeed;
    public float rotationSpeed;
    public float radius;
    public float height;
}


[Serializable]
public class AllEntities
{
    public List<OneEntityData> entities;
}

