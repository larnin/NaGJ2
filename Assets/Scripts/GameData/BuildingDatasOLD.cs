using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class BuildingContainerOLD
{
    [HideInInspector] public int id; //building ID
    [HideInInspector] public int index; //container index in building

    public BuildingPortDirection direction;
    public ResourceFilter filter;
    public int count;
}

[Serializable]
public class BuildingOnePortDataOLD
{
    public Vector3Int pos;
    public Rotation rotation;
    public BuildingPortDirection direction;
    public int containerIndex;
}

[Serializable]
public class BuildingOneBeltDataOLD
{
    public Vector3Int pos;
    public Rotation rotation;
    public BeltDirection direction;
}
