using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BlockDataEx
{
    public static void GetBlockInfo(BlockType type, byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        switch(type)
        {
            case BlockType.ground:
                GetBlockInfoGround(mat, out prefab, out outRot);
                break;
            case BlockType.groundSlope:
                GetBlockInfoSlope(data, mat, out prefab, out outRot);
                break;
            case BlockType.lake:
                GetBlockInfoLake(mat, out prefab, out outRot);
                break;
            case BlockType.river:
                GetBlockInfoRiver(data, mat, out prefab, out outRot);
                break;
            case BlockType.waterfall:
                GetBlockInfoWaterfall(data, mat, out prefab, out outRot);
                break;
            case BlockType.road:
                GetBlockInfoRoad(mat, out prefab, out outRot);
                break;
            case BlockType.grass:
                GetBlockInfoGrass(data, mat, out prefab, out outRot);
                break;
            default:
                prefab = null;
                outRot = Quaternion.identity;
                break;
        }
    }

    static void GetBlockInfoGround(NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        var grounds = Global.instance.allBlocks?.ground;
        if(grounds == null)
        {
            prefab = null;
            outRot = Quaternion.identity;
        }

        BlockLayerData layer = grounds.solo;

        if (IsBlockFull(mat.Get(0, 1, 0).id))
        {
            if (IsBlockFull(mat.Get(0, -1, 0).id))
                layer = grounds.middle;
            else layer = grounds.bottom;
        }
        else if (IsBlockFull(mat.Get(0, -1, 0).id))
            layer = grounds.top;

        var layerMat = mat.GetLayerMatrix(0);

        GetSolidBlockInfo(layerMat, layer, out prefab, out outRot);
    }

    static void GetSolidBlockInfo(NearMatrix<SimpleBlock> mat, BlockLayerData layer, out GameObject prefab, out Quaternion outRot)
    {
        prefab = layer.solo;
        outRot = Quaternion.identity;

        bool top = IsBlockFull(mat.Get(0, -1).id);
        bool down = IsBlockFull(mat.Get(0, 1).id);
        bool left = IsBlockFull(mat.Get(-1, 0).id);
        bool right = IsBlockFull(mat.Get(1, 0).id);
        int nb = (top ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        Rotation rot = Rotation.rot_0;

        if (nb == 4)
        {
            rot = RotationEx.RandomRotation();
            prefab = layer.full;
        }
        else if (nb == 3)
        {
            if (!top)
                rot = Rotation.rot_0;
            else if (!right)
                rot = Rotation.rot_270;
            else if (!down)
                rot = Rotation.rot_180;
            else rot = Rotation.rot_90;
            prefab = layer.treeSide;
        }
        else if (nb == 2)
        {
            if (!top && !down)
            {
                rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_0 : Rotation.rot_180;
                prefab = layer.line;
            }
            else if (!left && !right)
            {

                rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_90 : Rotation.rot_270;
                prefab = layer.line;
            }
            else
            {
                if (!left && !top)
                    rot = Rotation.rot_90;
                else if (!top && !right)
                    rot = Rotation.rot_0;
                else if (!right && !down)
                    rot = Rotation.rot_270;
                else rot = Rotation.rot_180;

                prefab = layer.corner;
            }
        }
        else if (nb == 1)
        {
            if (top)
                rot = Rotation.rot_270;
            else if (right)
                rot = Rotation.rot_180;
            else if (down)
                rot = Rotation.rot_90;
            else rot = Rotation.rot_0;
            prefab = layer.oneSide;
        }
        else
        {
            rot = RotationEx.RandomRotation();
            prefab = layer.solo;
        }

        outRot = RotationEx.ToQuaternion(rot);
    }

    static void GetBlockInfoSlope(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        var grounds = Global.instance.allBlocks?.groundSlope;
        if (grounds == null)
        {
            prefab = null;
            outRot = Quaternion.identity;
        }

        Rotation rot = ExtractDataRotation(data);

        var offset = RotationEx.ToVectorInt(rot);
        offset = RotationEx.Rotate(offset, Rotation.rot_90);

        bool right = IsBlockFull(mat.Get(offset.x, 0, offset.y).id);
        bool left = IsBlockFull(mat.Get(-offset.x, 0, -offset.y).id);

        outRot = RotationEx.ToQuaternion(rot);

        if (left && right)
            prefab = grounds.center;
        else if (left)
            prefab = grounds.right;
        else if (right)
            prefab = grounds.left;
        else prefab = grounds.single;

    }

    static void GetBlockInfoLake(NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {

        prefab = Global.instance.allBlocks.lake.single;
        outRot = Quaternion.identity;
    }

    static void GetBlockInfoRiver(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        prefab = Global.instance.allBlocks.river.line;
        outRot = Quaternion.identity;
    }

    static void GetBlockInfoRoad(NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        var roads = Global.instance.allBlocks.road;

        prefab = Global.instance.allBlocks.road.single;

        Rotation rot = Rotation.rot_0;

        var ground = mat.Get(0, -1, 0).id;
        if(ground == BlockType.lake || ground == BlockType.river)
        {
            var left = mat.Get(-1, 0, 0).id;
            var right = mat.Get(1, 0, 0).id;

            prefab = roads.bridge;

            if(left == BlockType.lake || left == BlockType.river || right == BlockType.river || right == BlockType.lake)
                rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_0 : Rotation.rot_180;
            else rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_90 : Rotation.rot_270;
        }
        else if(ground == BlockType.groundSlope)
        {
            prefab = roads.slope;

            var groundData = mat.Get(0, -1, 0).data;
            rot = ExtractDataRotation(groundData);
        }
        else
        {
            bool top = mat.Get(0, 0, -1).id == BlockType.road || mat.Get(0, 1, -1).id == BlockType.road || mat.Get(0, -1, -1).id == BlockType.road;
            bool down = mat.Get(0, 0, 1).id == BlockType.road || mat.Get(0, 1, 1).id == BlockType.road || mat.Get(0, -1, 1).id == BlockType.road;
            bool left = mat.Get(-1, 0, 0).id == BlockType.road || mat.Get(-1, 1, 0).id == BlockType.road || mat.Get(-1, -1, 0).id == BlockType.road;
            bool right = mat.Get(1, 0, 0).id == BlockType.road || mat.Get(1, 1, 0).id == BlockType.road || mat.Get(1, -1, 0).id == BlockType.road;

            int nb = (top ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

            if(nb == 4)
            {
                rot = RotationEx.RandomRotation();
                prefab = roads.cross;
            }
            else if(nb == 3)
            {
                prefab = roads.tShape;
                if (!right)
                    rot = Rotation.rot_0;
                else if (!top)
                    rot = Rotation.rot_90;
                else if (!left)
                    rot = Rotation.rot_180;
                else rot = Rotation.rot_270;
            }
            else if(nb == 2)
            {
                prefab = roads.line;
                if(left && right)
                    rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_0 : Rotation.rot_180;
                else if(top && down)
                    rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_90 : Rotation.rot_270;
                else
                {
                    prefab = roads.corner;
                    if (top && left)
                        rot = Rotation.rot_0;
                    else if (left && down)
                        rot = Rotation.rot_90;
                    else if (down && right)
                        rot = Rotation.rot_180;
                    else rot = Rotation.rot_270;
                }
            }
            else if(nb == 1)
            {
                prefab = roads.start;
                if (left)
                    rot = Rotation.rot_0;
                else if (down)
                    rot = Rotation.rot_90;
                else if (right)
                    rot = Rotation.rot_180;
                else rot = Rotation.rot_270;
            }
            else
            {
                rot = RotationEx.RandomRotation();
                prefab = roads.single;
            }
        }

        outRot = RotationEx.ToQuaternion(rot);
    }

    static void GetBlockInfoGrass(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        Rotation rot = ExtractDataRotation(data);
        int value = ExtractDataValue(data);

        outRot = RotationEx.ToQuaternion(rot);

        var grassVariants = Global.instance.allBlocks.grass.variants;

        if (value >= grassVariants.Count || value < 0)
            prefab = null;
        else prefab = grassVariants[value];
    }

    static void GetBlockInfoWaterfall(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        prefab = Global.instance.allBlocks.river.waterfallMiddle;
        outRot = Quaternion.identity;
    }

    // ---------------------------------------

    public static bool GetValidPos(BlockType type, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        switch(type)
        {
            case BlockType.ground:
            case BlockType.groundSlope:
                return GetValidPosGround(type, blockPos, pos, out outPos);
            case BlockType.river:
            case BlockType.lake:
                return GetValidPosPaint(type, blockPos, pos, out outPos);
            case BlockType.grass:
            case BlockType.road:
                return GetValidPosDecoration(type, blockPos, pos, out outPos);
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

    static bool GetValidPosGround(BlockType current, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
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

        if(b2.type != current)
        {
            outPos = pos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    static bool GetValidPosPaint(BlockType current, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        EditorGetBlockEvent b1 = new EditorGetBlockEvent(blockPos);
        EditorGetBlockEvent b2 = new EditorGetBlockEvent(pos);

        Event<EditorGetBlockEvent>.Broadcast(b1);
        Event<EditorGetBlockEvent>.Broadcast(b2);

        if ((IsBlockFull(b1.type) || b1.type == BlockType.air) && current != b1.type)
        {
            outPos = blockPos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    static bool GetValidPosDecoration(BlockType current, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        EditorGetBlockEvent b1 = new EditorGetBlockEvent(blockPos);
        EditorGetBlockEvent b2 = new EditorGetBlockEvent(pos);

        Event<EditorGetBlockEvent>.Broadcast(b1);
        Event<EditorGetBlockEvent>.Broadcast(b2);

        if (!IsBlockFull(b1.type))
        {
            outPos = blockPos;
            return true;
        }

        if (b2.type != current)
        {
            outPos = pos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    // ------------------------------------------

    public static void SetBlock(BlockType type, Vector3Int pos)
    {
        SetBlock(type, (byte)0, pos);
    }

    public static void SetBlock(BlockType type, Rotation rot, Vector3Int pos)
    {
        SetBlock(type, rot, pos);
    }

    public static void SetBlock(BlockType type, Rotation rot, int value, Vector3Int pos)
    {
        byte data = BlockDataEx.MakeData(rot, value);

        SetBlock(type, data, pos);
    }

    public static void SetBlock(BlockType type, byte data, Vector3Int pos)
    {
        Event<EditorSetBlockEvent>.Broadcast(new EditorSetBlockEvent(pos, type, data));
    }

    // ------------------------------------------

    public static bool IsBlockFull(BlockType type)
    {
        switch(type)
        {
            case BlockType.ground:
            case BlockType.groundSlope:
            case BlockType.lake:
            case BlockType.river:
                return true;
            default:
                return false;
        }
    }

    public static Rotation ExtractDataRotation(byte data)
    {
        int value = data & 3;
        return (Rotation)value;
    }

    public static int ExtractDataValue(byte data) //return value [0;63]
    {
        return data >> 2;
    }

    public static byte MakeData(Rotation rot, int value)
    {
        return (byte)((int)rot + (value << 2));
    }
}
