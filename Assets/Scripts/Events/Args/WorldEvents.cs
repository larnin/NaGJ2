using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShowLoadingScreenEvent
{
    public bool start;

    public ShowLoadingScreenEvent(bool _start)
    {
        start = _start;
    }
}

public class SetBlockEvent
{
    public BlockType type;
    public byte data;
    public Vector3Int pos;

    public SetBlockEvent(Vector3Int _pos, BlockType _type, byte _data = 0)
    {
        type = _type;
        data = _data;
        pos = _pos;
    }
}

public class GetBlockEvent
{
    public BlockType type;
    public byte data;
    public Vector3Int pos;

    public GetBlockEvent(Vector3Int _pos)
    {
        type = BlockType.air;
        data = 0;
        pos = _pos;
    }
}

public class GetNearBlocsEvent
{
    public Vector3Int pos;
    public NearMatrix3<SimpleBlock> matrix = new NearMatrix3<SimpleBlock>();

    public GetNearBlocsEvent(Vector3Int _pos)
    {
        pos = _pos;
    }
}
