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
    ironOre,
    copperOre,
    crystal,
    tree
}

public struct SimpleBlock
{
    public static readonly SimpleBlock defaultValue = new SimpleBlock(BlockType.air, 0);

    public BlockType id;
    public byte data;

    public SimpleBlock(BlockType _id, byte _data)
    {
        id = _id;
        data = _data;
    }
}

[Serializable]
public class BlockCollectResourceData
{
    public ResourceType resource;
    public float speed;
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
public class BlockFullData
{
    public BlockLayerData solo;
    public BlockLayerData top;
    public BlockLayerData middle;
    public BlockLayerData bottom;
    public Sprite sprite;
}

[Serializable]
public class SlopeBlockData
{
    public GameObject single;
    public GameObject left;
    public GameObject right;
    public GameObject center;
    public Sprite sprite;
}

[Serializable]
public class LakeBlockData
{
    public GameObject single;
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
    public Sprite sprite;
    public BlockCollectResourceData collect;
}

[Serializable]
public class RiverBlockData
{
    public GameObject line;
    public GameObject cornerRight;
    public GameObject cornerLeft;
    public GameObject meetSide;
    public GameObject meetLineLeft;
    public GameObject meetLineRight;
    public GameObject waterfallStartTop;
    public GameObject waterfallStartSingle;
    public GameObject waterfallUp;
    public GameObject waterfallMiddle;
    public GameObject waterfallDown;
    public GameObject waterfallEnd;
    public int waterfallSize = 5;
    public Sprite riverSprite;
    public Sprite waterfallSprite;
    public BlockCollectResourceData collect;
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
    public Sprite roadSprite;
    public Sprite bridgeSprite;
}

[Serializable]
public class SimpleBlockVariant
{
    public GameObject instance;
    public Sprite sprite;
}

[Serializable]
public class SimpleBlockData
{
    public List<SimpleBlockVariant> variants = new List<SimpleBlockVariant>();
}

[Serializable]
public class ResourceBlockData
{
    public List<SimpleBlockVariant> variants = new List<SimpleBlockVariant>();
    public BlockCollectResourceData collect;
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
    public SimpleBlockData tree;
    public ResourceBlockData ironOre;
    public ResourceBlockData copperOre;
    public ResourceBlockData crystal;
}
