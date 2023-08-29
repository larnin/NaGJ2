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
    groundSlope,
    lake,
    river,
    waterfall,
    road,
    grass,
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
public class SlopeBlockData
{
    public GameObject single;
    public GameObject left;
    public GameObject right;
    public GameObject center;
}

[Serializable]
public class LakeBlockData
{
    public GameObject single;
    public GameObject solo;
    public GameObject oneSide;
    public GameObject corner;
    public GameObject line;
    public GameObject treeSide;
    public GameObject full;
    public GameObject treeCorners;
    public GameObject twoCornersLine;
    public GameObject twoCornersOpposite;
    public GameObject oneCorner;
    public GameObject center;
    public GameObject cornerNoOpposite;
    public GameObject lineOneCornerLeft;
    public GameObject lineOneCornerRight;
    public GameObject lineOnly;
    public GameObject lakeIn;
    public GameObject lakeOut;
}

[Serializable]
public class RiverBlockData
{
    public GameObject line;
    public GameObject corner;
    public GameObject meetSide;
    public GameObject meetLineLeft;
    public GameObject meetLineRight;
    public GameObject waterfallStartTop;
    public GameObject waterfallStartSingle;
    public GameObject waterfallUp;
    public GameObject waterfallMiddle;
    public GameObject waterfallDown;
}

[Serializable]
public class RoadBlockData
{
    public GameObject single;
    public GameObject start;
    public GameObject line;
    public GameObject corner;
    public GameObject tShape;
    public GameObject cross;
    public GameObject slope;
    public GameObject bridge;
}

[Serializable]
public class SimpleBlockData
{
    public List<GameObject> variants = new List<GameObject>();
}

[Serializable]
public class AllBlocks
{
    public Vector3 blockSize = Vector3.one;
    public BlockFullData ground;
    public SlopeBlockData groundSlope;
    public LakeBlockData lake;
    public RiverBlockData river;
    public RoadBlockData road;
    public SimpleBlockData grass;
}
