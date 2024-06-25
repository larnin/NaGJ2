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
            case BlockType.ironOre:
                GetBlockInfoIronOre(data, mat, out prefab, out outRot);
                break;
            case BlockType.copperOre:
                GetBlockInfoCopperOre(data, mat, out prefab, out outRot);
                break;
            case BlockType.crystal:
                GetBlockInfoCrystal(data, mat, out prefab, out outRot);
                break;
            case BlockType.tree:
                GetBlockInfoTree(data, mat, out prefab, out outRot);
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
                rot = Rotation.rot_90;
            else if (!down)
                rot = Rotation.rot_180;
            else rot = Rotation.rot_270;
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
                    rot = Rotation.rot_270;
                else if (!top && !right)
                    rot = Rotation.rot_0;
                else if (!right && !down)
                    rot = Rotation.rot_90;
                else rot = Rotation.rot_180;

                prefab = layer.corner;
            }
        }
        else if (nb == 1)
        {
            if (top)
                rot = Rotation.rot_90;
            else if (right)
                rot = Rotation.rot_180;
            else if (down)
                rot = Rotation.rot_270;
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
        var lakes = Global.instance.allBlocks.lake;

        prefab = lakes.single;

        Rotation rot = Rotation.rot_0;

        bool top = mat.Get(0, 0, -1).id == BlockType.lake;
        bool down = mat.Get(0, 0, 1).id == BlockType.lake;
        bool left = mat.Get(-1, 0, 0).id == BlockType.lake;
        bool right = mat.Get(1, 0, 0).id == BlockType.lake;

        bool topLeft = mat.Get(-1, 0, -1).id == BlockType.lake;
        bool topRight = mat.Get(1, 0, -1).id == BlockType.lake;
        bool downLeft = mat.Get(-1, 0, 1).id == BlockType.lake;
        bool downRight = mat.Get(1, 0, 1).id == BlockType.lake;

        int nb = (top ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
        int nbCorner = (topLeft ? 1 : 0) + (topRight ? 1 : 0) + (downLeft ? 1 : 0) + (downRight ? 1 : 0);

        Vector3Int[] posArray = new Vector3Int[] { new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1) };

        bool placedRiver = false;
        foreach(var pos in posArray)
        {
            var block = mat.Get(pos.x, pos.y, pos.z);
            if (block.id != BlockType.river)
                continue;

            Rotation riverRot = ExtractDataRotation(block.data);
            var offset = RotationEx.ToVector3Int(riverRot);

            if (offset != pos && offset != -pos)
                continue;

            placedRiver = true;

            if(offset == pos)
            {
                prefab = lakes.lakeOut;
                rot = RotationEx.Add(riverRot, Rotation.rot_180); 
            }
            else
            {
                prefab = lakes.lakeIn;
                rot = riverRot;
            }
        }

        if (!placedRiver)
        {
            if (nb == 4)
            {
                if (nbCorner == 4)
                {
                    rot = RotationEx.RandomRotation();
                    prefab = lakes.center;
                }
                else if (nbCorner == 3)
                {
                    prefab = lakes.oneCorner;
                    if (!topLeft)
                        rot = Rotation.rot_180;
                    else if (!topRight)
                        rot = Rotation.rot_270;
                    else if (!downRight)
                        rot = Rotation.rot_0;
                    else rot = Rotation.rot_90;
                }
                else if (nbCorner == 2)
                {
                    if (topLeft && downRight)
                    {
                        prefab = lakes.twoCornersOpposite;
                        rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_90 : Rotation.rot_270;
                    }
                    else if (topRight && downLeft)
                    {
                        prefab = lakes.twoCornersOpposite;
                        rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_0 : Rotation.rot_180;
                    }
                    else
                    {
                        prefab = lakes.twoCornersLine;
                        if (topLeft && topRight)
                            rot = Rotation.rot_90;
                        else if (topRight && downRight)
                            rot = Rotation.rot_180;
                        else if (downRight && downLeft)
                            rot = Rotation.rot_270;
                        else rot = Rotation.rot_0;
                    }
                }
                else if (nbCorner == 1)
                {
                    prefab = lakes.treeCorners;
                    if (topLeft)
                        rot = Rotation.rot_90;
                    else if (topRight)
                        rot = Rotation.rot_180;
                    else if (downRight)
                        rot = Rotation.rot_270;
                    else rot = Rotation.rot_0;
                }
                else
                {
                    rot = RotationEx.RandomRotation();
                    prefab = lakes.full;
                }
            }
            else if (nb == 3)
            {
                bool oppositeLeft = false;
                bool oppositeRight = false;

                if (!top)
                {
                    rot = Rotation.rot_270;
                    oppositeLeft = downLeft;
                    oppositeRight = downRight;
                }
                else if (!left)
                {
                    rot = Rotation.rot_180;
                    oppositeLeft = downRight;
                    oppositeRight = topRight;
                }
                else if (!down)
                {
                    rot = Rotation.rot_90;
                    oppositeLeft = topRight;
                    oppositeRight = topLeft;

                }
                else
                {
                    rot = Rotation.rot_0;
                    oppositeLeft = topLeft;
                    oppositeRight = downLeft;
                }

                if (oppositeLeft && oppositeRight)
                    prefab = lakes.lineOnly;
                else if (oppositeLeft)
                    prefab = lakes.lineOneCornerLeft;
                else if (oppositeRight)
                    prefab = lakes.lineOneCornerRight;
                else prefab = lakes.treeSide;
            }
            else if (nb == 2)
            {
                if (top && down)
                {
                    prefab = lakes.line;
                    rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_0 : Rotation.rot_180;
                }
                else if (left && right)
                {
                    prefab = lakes.line;
                    rot = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_90 : Rotation.rot_270;
                }
                else
                {
                    if (top && left)
                    {
                        rot = Rotation.rot_270;
                        prefab = topLeft ? lakes.cornerNoOpposite : lakes.corner;
                    }
                    else if (left && down)
                    {
                        rot = Rotation.rot_180;
                        prefab = downLeft ? lakes.cornerNoOpposite : lakes.corner;
                    }
                    else if (down && right)
                    {
                        rot = Rotation.rot_90;
                        prefab = downRight ? lakes.cornerNoOpposite : lakes.corner;
                    }
                    else
                    {
                        rot = Rotation.rot_0;
                        prefab = topRight ? lakes.cornerNoOpposite : lakes.corner;
                    }
                }
            }
            else if (nb == 1)
            {
                prefab = lakes.oneSide;
                if (top)
                    rot = Rotation.rot_0;
                else if (right)
                    rot = Rotation.rot_90;
                else if (down)
                    rot = Rotation.rot_180;
                else rot = Rotation.rot_270;
            }
            else
            {
                rot = RotationEx.RandomRotation();
                prefab = lakes.single;
            }
        }

        outRot = RotationEx.ToQuaternion(rot);
    }

    static void GetBlockInfoRiver(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        var rivers = Global.instance.allBlocks.river;

        prefab = rivers.line;

        Rotation rot = ExtractDataRotation(data);

        Vector3Int offset = RotationEx.ToVector3Int(rot);
        Vector3Int orthoOffset = RotationEx.Rotate(offset, Rotation.rot_90);

        var front = mat.Get(offset.x, offset.y, offset.z).id;
        var back = mat.Get(-offset.x, -offset.y, -offset.z).id;
        var left = mat.Get(orthoOffset.x, orthoOffset.y, orthoOffset.z).id;
        var right = mat.Get(-orthoOffset.x, -orthoOffset.y, -orthoOffset.z).id;

        if(!IsBlockFull(front))
        {
            var down = IsBlockFull(mat.Get(0, -1, 0).id);
            if (down)
                prefab = rivers.waterfallStartTop;
            else prefab = rivers.waterfallStartSingle;
        }
        else
        {
            bool frontWater = front == BlockType.river || front == BlockType.lake;
            bool backWater = back == BlockType.river || back == BlockType.lake;
            bool leftWater = left == BlockType.river || left == BlockType.lake;
            bool rightWater = right == BlockType.river || right == BlockType.lake;

            if (frontWater)
            {
                if (backWater)
                {
                    if (leftWater)
                        prefab = rivers.meetLineLeft;
                    else if (rightWater)
                        prefab = rivers.meetLineRight;
                    else prefab = rivers.line;
                }
                else if (leftWater && rightWater)
                    prefab = rivers.meetSide;
                else if (leftWater)
                    prefab = rivers.cornerLeft;
                else if (rightWater)
                    prefab = rivers.cornerRight;
                else prefab = rivers.line;
            }
            else
            {
                if (!backWater)
                {
                    if (leftWater && rightWater)
                        prefab = rivers.meetSide;
                    else if (leftWater)
                        prefab = rivers.cornerLeft;
                    else if (rightWater)
                        prefab = rivers.cornerRight;
                    else prefab = rivers.line;
                }
                else if(leftWater)
                {
                    prefab = rivers.cornerLeft;
                    rot = RotationEx.Add(rot, Rotation.rot_90);
                }
                else 
                {
                    prefab = rivers.cornerRight;
                    rot = RotationEx.Sub(rot, Rotation.rot_90);
                }
            }
        }

        outRot = RotationEx.ToQuaternion(rot);
    }

    static void GetBlockInfoRoad(NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        var roads = Global.instance.allBlocks.road;

        prefab = roads.single;

        Rotation rot = Rotation.rot_0;

        var ground = mat.Get(0, -1, 0).id;
        if(ground == BlockType.lake || ground == BlockType.river)
        {
            var left = mat.Get(-1, -1, 0).id;
            var right = mat.Get(1, -1, 0).id;

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
                    rot = Rotation.rot_270;
                else if (!left)
                    rot = Rotation.rot_180;
                else rot = Rotation.rot_90;
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
                        rot = Rotation.rot_270;
                    else if (down && right)
                        rot = Rotation.rot_180;
                    else rot = Rotation.rot_90;
                }
            }
            else if(nb == 1)
            {
                prefab = roads.start;
                if (left)
                    rot = Rotation.rot_0;
                else if (down)
                    rot = Rotation.rot_270;
                else if (right)
                    rot = Rotation.rot_180;
                else rot = Rotation.rot_90;
            }
            else
            {
                rot = RotationEx.RandomRotation();
                prefab = roads.single;
            }
        }

        outRot = RotationEx.ToQuaternion(rot);
    }

    static void GetBlockInfoVariants(List<SimpleBlockVariant> variants, byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        Rotation rot = ExtractDataRotation(data);
        int value = ExtractDataValue(data);

        outRot = RotationEx.ToQuaternion(rot);

        if (variants == null || value >= variants.Count || value < 0)
            prefab = null;
        else prefab = variants[value].instance;
    }

    static void GetBlockInfoGrass(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        GetBlockInfoVariants(Global.instance.allBlocks.grass.variants, data, mat, out prefab, out outRot);
    }

    static void GetBlockInfoIronOre(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        GetBlockInfoVariants(Global.instance.allBlocks.ironOre.variants, data, mat, out prefab, out outRot);
    }

    static void GetBlockInfoCopperOre(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        GetBlockInfoVariants(Global.instance.allBlocks.copperOre.variants, data, mat, out prefab, out outRot);
    }

    static void GetBlockInfoCrystal(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        GetBlockInfoVariants(Global.instance.allBlocks.crystal.variants, data, mat, out prefab, out outRot);
    }

    static void GetBlockInfoTree(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        GetBlockInfoVariants(Global.instance.allBlocks.tree.variants, data, mat, out prefab, out outRot);
    }

    static void GetBlockInfoWaterfall(byte data, NearMatrix3<SimpleBlock> mat, out GameObject prefab, out Quaternion outRot)
    {
        bool top = mat.Get(0, 1, 0).id == BlockType.waterfall;
        bool bottom = mat.Get(0, -1, 0).id == BlockType.waterfall;
        bool bottomFull = IsBlockFull(mat.Get(0, -1, 0).id);

        var rivers = Global.instance.allBlocks.river;

        Rotation rot = ExtractDataRotation(data);
        int value = ExtractDataValue(data);

        if (!bottom && !bottomFull)
            prefab = rivers.waterfallEnd;
        else if (top && bottom)
            prefab = rivers.waterfallMiddle;
        else if (top)
            prefab = rivers.waterfallDown;
        else prefab = rivers.waterfallUp;

        outRot = RotationEx.ToQuaternion(rot);
    }

    // ---------------------------------------

    public static bool GetValidPos(GameLevel level, BlockType type, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        switch (type)
        {
            case BlockType.ground:
            case BlockType.groundSlope:
                return GetValidPosGround(level, type, blockPos, pos, out outPos);
            case BlockType.river:
            case BlockType.lake:
                return GetValidPosPaint(level, type, blockPos, pos, out outPos);
            case BlockType.grass:
            case BlockType.road:
            case BlockType.ironOre:
            case BlockType.copperOre:
            case BlockType.crystal:
            case BlockType.tree:
                return GetValidPosDecoration(level, type, blockPos, pos, out outPos);
            case BlockType.air:
                return GetValidPosAir(level, blockPos, pos, out outPos);
            default:
                outPos = Vector3Int.zero;
                return false;
        }
    }

    static bool GetValidPosAir(GameLevel level, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        var b1 = level.grid.GetBlock(blockPos);
        var b2 = level.grid.GetBlock(pos);

        if (b2.id != BlockType.air)
        {
            outPos = pos;
            return true;
        }

        if (b1.id != BlockType.air)
        {
            outPos = blockPos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    static bool GetValidPosGround(GameLevel level, BlockType current, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        var b1 = level.grid.GetBlock(blockPos);
        var b2 = level.grid.GetBlock(pos);

        if (b1.id == BlockType.air)
        {
            outPos = blockPos;
            return true;
        }

        if (b2.id != current)
        {
            outPos = pos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    static bool GetValidPosPaint(GameLevel level, BlockType current, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        var b1 = level.grid.GetBlock(blockPos);
        var b2 = level.grid.GetBlock(pos);

        if ((IsBlockFull(b1.id) || b1.id == BlockType.air || b1.id == BlockType.waterfall) && current != b1.id)
        {
            outPos = blockPos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    static bool GetValidPosDecoration(GameLevel level, BlockType current, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        var b1 = level.grid.GetBlock(blockPos);
        var b2 = level.grid.GetBlock(pos);

        if (!IsBlockFull(b1.id))
        {
            outPos = blockPos;
            return true;
        }

        if (b2.id != current)
        {
            outPos = pos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }



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
            case BlockType.ironOre:
            case BlockType.copperOre:
            case BlockType.crystal:
            case BlockType.tree:
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
        GetBlockEvent b1 = new GetBlockEvent(blockPos);
        GetBlockEvent b2 = new GetBlockEvent(pos);

        Event<GetBlockEvent>.Broadcast(b1);
        Event<GetBlockEvent>.Broadcast(b2);

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
        GetBlockEvent b1 = new GetBlockEvent(blockPos);
        GetBlockEvent b2 = new GetBlockEvent(pos);

        Event<GetBlockEvent>.Broadcast(b1);
        Event<GetBlockEvent>.Broadcast(b2);

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
        GetBlockEvent b1 = new GetBlockEvent(blockPos);
        GetBlockEvent b2 = new GetBlockEvent(pos);

        Event<GetBlockEvent>.Broadcast(b1);
        Event<GetBlockEvent>.Broadcast(b2);

        if ((IsBlockFull(b1.type) || b1.type == BlockType.air || b1.type == BlockType.waterfall) && current != b1.type)
        {
            outPos = blockPos;
            return true;
        }

        outPos = Vector3Int.zero;
        return false;
    }

    static bool GetValidPosDecoration(BlockType current, Vector3Int blockPos, Vector3Int pos, out Vector3Int outPos)
    {
        GetBlockEvent b1 = new GetBlockEvent(blockPos);
        GetBlockEvent b2 = new GetBlockEvent(pos);

        Event<GetBlockEvent>.Broadcast(b1);
        Event<GetBlockEvent>.Broadcast(b2);

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

    public static void SetBlock(GameLevel level, BlockType type, Vector3Int pos)
    {
        SetBlock(level, type, (byte)0, pos);
    }

    public static void SetBlock(GameLevel level, BlockType type, Rotation rot, Vector3Int pos)
    {
        SetBlock(level, type, rot, pos);
    }

    public static void SetBlock(GameLevel level, BlockType type, Rotation rot, int value, Vector3Int pos)
    {
        byte data = BlockDataEx.MakeData(rot, value);

        SetBlock(level, type, data, pos);
    }

    public static void SetBlock(GameLevel level, BlockType type, byte data, Vector3Int pos)
    {
        RemoveCustomBlocksFrom(level, pos);
        level.grid.SetBlock(pos, type, data);
        AddCustomBlocksFrom(level, pos);
    }

    // ------------------------------------------

    public static void RemoveCustomBlocksFrom(GameLevel level, Vector3Int pos)
    {
        var block = level.grid.GetBlock(pos);

        switch (block.id)
        {
            case BlockType.river:
                RemoveCustomBlocksRiverFrom(level, pos, block.data);
                break;
            case BlockType.waterfall:
                RemoveCustomBlockWaterfallFrom(level, pos);
                break;
            default:
                break;
        }
    }

    public static void RemoveCustomBlocksRiverFrom(GameLevel level, Vector3Int pos, byte data)
    {
        Rotation rot = ExtractDataRotation(data);

        Vector3Int front = RotationEx.ToVector3Int(rot) + pos;
        var block = level.grid.GetBlock(front);
        SetBlockEvent setBlock = new SetBlockEvent(front, BlockType.air);
        while (block.id == BlockType.waterfall)
        {
            level.grid.SetBlock(front, BlockType.air);
            front.y--;
            block = level.grid.GetBlock(front);
        }
    }

    public static void RemoveCustomBlockWaterfallFrom(GameLevel level, Vector3Int pos)
    {
        var block = level.grid.GetBlock(pos);
        SetBlockEvent setBlock = new SetBlockEvent(pos, BlockType.air);
        while (block.id == BlockType.waterfall)
        {
            level.grid.SetBlock(pos, BlockType.air);
            pos.y--;
            block = level.grid.GetBlock(pos);
        }
    }

    public static void AddCustomBlocksFrom(GameLevel level, Vector3Int pos)
    {
        var block = level.grid.GetBlock(pos);

        switch (block.id)
        {
            case BlockType.river:
                AddCustomBlocksRiverFrom(level, pos, block.data);
                break;
            default:
                break;
        }
    }

    public static void AddCustomBlocksRiverFrom(GameLevel level, Vector3Int pos, byte data)
    {
        Rotation rot = ExtractDataRotation(data);

        Vector3Int front = RotationEx.ToVector3Int(rot) + pos;
        var block = level.grid.GetBlock(front);
        int nbMax = Global.instance.allBlocks.river.waterfallSize;
        var waterfallData = MakeData(rot, 0);

        for (int i = 0; i < nbMax && block.id == BlockType.air; i++)
        {
            level.grid.SetBlock(front, BlockType.waterfall, waterfallData);
            front.y--;
            block = level.grid.GetBlock(front);
        }
    }

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
        RemoveCustomBlocksFrom(pos);
        Event<SetBlockEvent>.Broadcast(new SetBlockEvent(pos, type, data));
        AddCustomBlocksFrom(pos);
    }

    // ------------------------------------------

    public static void RemoveCustomBlocksFrom(Vector3Int pos)
    {
        GetBlockEvent block = new GetBlockEvent(pos);
        Event<GetBlockEvent>.Broadcast(block);

        switch(block.type)
        {
            case BlockType.river:
                RemoveCustomBlocksRiverFrom(pos, block.data);
                break;
            case BlockType.waterfall:
                RemoveCustomBlockWaterfallFrom(pos);
                break;
            default:
                break;
        }
    }

    public static void RemoveCustomBlocksRiverFrom(Vector3Int pos, byte data)
    {
        Rotation rot = ExtractDataRotation(data);

        Vector3Int front = RotationEx.ToVector3Int(rot) + pos;
        GetBlockEvent block = new GetBlockEvent(front);
        Event<GetBlockEvent>.Broadcast(block);
        SetBlockEvent setBlock = new SetBlockEvent(front, BlockType.air);
        while(block.type == BlockType.waterfall)
        {
            setBlock.pos = block.pos;
            Event<SetBlockEvent>.Broadcast(setBlock);
            block.pos.y--;
            Event<GetBlockEvent>.Broadcast(block);
        }
    }

    public static void RemoveCustomBlockWaterfallFrom(Vector3Int pos)
    {
        GetBlockEvent block = new GetBlockEvent(pos);
        Event<GetBlockEvent>.Broadcast(block);
        SetBlockEvent setBlock = new SetBlockEvent(pos, BlockType.air);
        while (block.type == BlockType.waterfall)
        {
            setBlock.pos = block.pos;
            Event<SetBlockEvent>.Broadcast(setBlock);
            block.pos.y--;
            Event<GetBlockEvent>.Broadcast(block);
        }
    }

    public static void AddCustomBlocksFrom(Vector3Int pos)
    {
        GetBlockEvent block = new GetBlockEvent(pos);
        Event<GetBlockEvent>.Broadcast(block);

        switch (block.type)
        {
            case BlockType.river:
                AddCustomBlocksRiverFrom(pos, block.data);
                break;
            default:
                break;
        }
    }

    public static void AddCustomBlocksRiverFrom(Vector3Int pos, byte data)
    {
        Rotation rot = ExtractDataRotation(data);

        Vector3Int front = RotationEx.ToVector3Int(rot) + pos;
        GetBlockEvent block = new GetBlockEvent(front);
        Event<GetBlockEvent>.Broadcast(block);
        SetBlockEvent setBlock = new SetBlockEvent(front, BlockType.waterfall, MakeData(rot, 0));
        int nbMax = Global.instance.allBlocks.river.waterfallSize;

        for(int i = 0; i < nbMax && block.type == BlockType.air; i++)
        {
            setBlock.pos = block.pos;
            Event<SetBlockEvent>.Broadcast(setBlock);
            block.pos.y--;
            Event<GetBlockEvent>.Broadcast(block);
        }
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

    public static bool CanPlaceBuildingOnBlock(BlockType type)
    {
        return type == BlockType.ground;
    }

    public static Sprite GetVariantSprite(List<SimpleBlockVariant> variants, int value)
    {
        if (value >= variants.Count || value == 0)
            return null;

        return variants[value].sprite;
    }

    public static Sprite GetSprite(BlockType type, int value)
    {
        var blocks = Global.instance.allBlocks;

        switch (type)
        {
            case BlockType.ground:
                return blocks.ground.sprite;
            case BlockType.groundSlope:
                return blocks.groundSlope.sprite;
            case BlockType.lake:
                return blocks.lake.sprite;
            case BlockType.river:
                return blocks.river.riverSprite;
            case BlockType.waterfall:
                return blocks.river.waterfallSprite;
            case BlockType.road:
                return blocks.road.roadSprite;
            case BlockType.grass:
                return GetVariantSprite(blocks.grass.variants, value);
            case BlockType.ironOre:
                return GetVariantSprite(blocks.ironOre.variants, value);
            case BlockType.copperOre:
                return GetVariantSprite(blocks.copperOre.variants, value);
            case BlockType.crystal:
                return GetVariantSprite(blocks.crystal.variants, value);
            case BlockType.tree:
                return GetVariantSprite(blocks.tree.variants, value);
        }

        return null;
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
