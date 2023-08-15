using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BlockDataEx
{
    public static void GetBlockInfo(BlockType type, byte data, NearMatrix3<BlockType> mat, out GameObject prefab, out Quaternion outRot)
    {
        switch(type)
        {
            case BlockType.ground:
                GetBlockInfoGround(mat, out prefab, out outRot);
                break;
            default:
                prefab = null;
                outRot = Quaternion.identity;
                break;
        }
    }

    static void GetBlockInfoGround(NearMatrix3<BlockType> mat, out GameObject prefab, out Quaternion outRot)
    {
        var grounds = Global.instance.allBlocks?.ground;
        if(grounds == null)
        {
            prefab = null;
            outRot = Quaternion.identity;
        }

        //todo !
        prefab = grounds.solo.solo;
        outRot = Quaternion.identity;
    }

    // ---------------------------------------

    public static bool GetValidPos(BlockType type, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        switch(type)
        {
            case BlockType.ground:
                return GetValidPosGround(blockPos, pos, out outPos);
            case BlockType.air:
                return GetValidPosAir(blockPos, pos, out outPos);
            default:
                outPos = Vector3Int.zero;
                return false;
        }
    }

    static bool GetValidPosAir(Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        EditorGetBlockEvent b1 = new EditorGetBlockEvent(blockPos);
        EditorGetBlockEvent b2 = new EditorGetBlockEvent(pos);

        Event<EditorGetBlockEvent>.Broadcast(b1);
        Event<EditorGetBlockEvent>.Broadcast(b2);

        if(b2.type != BlockType.air)
        {
            outPos = pos;
            return true;
        }

        if(b1.type != BlockType.air)
        {
            outPos = blockPos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    static bool GetValidPosGround(Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        EditorGetBlockEvent b1 = new EditorGetBlockEvent(blockPos);
        EditorGetBlockEvent b2 = new EditorGetBlockEvent(pos);

        Event<EditorGetBlockEvent>.Broadcast(b1);
        Event<EditorGetBlockEvent>.Broadcast(b2);

        if(b1.type == BlockType.air)
        {
            outPos = blockPos;
            return true;
        }

        if(b2.type != BlockType.ground)
        {
            outPos = pos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    // ------------------------------------------

    public static void SetBlock(BlockType type, Rotation rot, Vector3Int pos)
    {
        switch (type)
        {
            case BlockType.ground:
            case BlockType.air:
                SetSimpleBlock(type, pos);
                break;
            default:
                break;
        }
    }

    static void SetSimpleBlock(BlockType type, Vector3Int pos)
    {
        Event<EditorSetBlockEvent>.Broadcast(new EditorSetBlockEvent(pos, type));
    }
}
