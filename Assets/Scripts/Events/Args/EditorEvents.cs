using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorCurstorGetPosEvent
{
    public Vector3Int pos = Vector3Int.zero;
    public Vector3Int blockPos = Vector3Int.zero;
}

public enum EditorCursorClickType
{
    leftClick,
    rightClick,
}

public class EditorCursorClickEvent
{
    public EditorCursorClickType clickType;

    public EditorCursorClickEvent(EditorCursorClickType _clickType)
    {
        clickType = _clickType;
    }
}

public class EditorSetBlockEvent
{
    public BlockType type;
    public byte data;
    public Vector3Int pos;

    public EditorSetBlockEvent(Vector3Int _pos, BlockType _type, byte _data = 0)
    {
        type = _type;
        data = _data;
        pos = _pos;
    }
}

public class EditorGetBlockEvent
{
    public BlockType type;
    public byte data;
    public Vector3Int pos;

    public EditorGetBlockEvent(Vector3Int _pos)
    {
        type = BlockType.air;
        data = 0;
        pos = _pos;
    }
}

public class EditorSetCursorBlockEvent
{
    public BlockType type;

    public EditorSetCursorBlockEvent(BlockType _type)
    {
        type = _type;
    }
}