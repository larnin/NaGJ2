using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BlockType
{
    air,
    ground,
}

[Serializable]
public class BlockLayerData
{
    public GameObject solo;
    public GameObject oneSide;
    public GameObject corner;
    public GameObject line;
    public GameObject treeSide;
    public GameObject full;
}

[Serializable]
public class BlockConnexionData
{

}

[Serializable]
public class BlockFullData
{
    public BlockLayerData solo;
    public BlockLayerData top;
    public BlockLayerData middle;
    public BlockLayerData bottom;

    public BlockConnexionData connexion;
}


[Serializable]
public class AllBlocks
{
    public Vector3 blockSize = Vector3.one;
    public BlockFullData ground;
}
