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
    public Rotation rot;
    public Vector3Int pos;

    public EditorSetBlockEvent(Vector3Int _pos, BlockType _type, Rotation _rot)
    {
        type = _type;
        rot = _rot;
        pos = _pos;
    }

}