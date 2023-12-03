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
    public int blockData;

    public EditorSetCursorBlockEvent(BlockType _type, int _blockData)
    {
        type = _type;
        blockData = _blockData;
    }
}

public class EditorGetCursorBlockEvent
{
    public BlockType type;
    public Rotation rotation;
    public int blockData;
}

public class EditorSetCursorBuildingEvent
{
    public BuildingType type;

    public EditorSetCursorBuildingEvent(BuildingType _type)
    {
        type = _type;
    }
}

public class EditorGetCursorBuildingEvent
{
    public BuildingType type;
    public Rotation rotation;
}

public class EditorHaveBuildingEvent
{
    public Vector3Int pos;
    public bool haveBuilding;

    public EditorHaveBuildingEvent(Vector3Int _pos)
    {
        pos = _pos;
    }
}

public class EditorGetBuildingEvent
{
    public Vector3Int pos;

    public Vector3Int buildingPos;
    public BuildingType buildingType;
    public Rotation rotation;

    public EditorGetBuildingEvent(Vector3Int _pos)
    {
        pos = _pos;
    }
}

public class EditorCanPlaceBuildingEvent
{
    public Vector3Int pos;
    public BuildingType buildingType;
    public Rotation rotation;

    public bool canBePlaced;

    public EditorCanPlaceBuildingEvent(Vector3Int _pos, BuildingType _type, Rotation _rot)
    {
        pos = _pos;
        buildingType = _type;
        rotation = _rot;
    }
}

public class EditorPlaceBuildingEvent
{
    public Vector3Int pos;
    public BuildingType buildingType;
    public Rotation rotation;

    public EditorPlaceBuildingEvent(Vector3Int _pos, BuildingType _type, Rotation _rot)
    {
        pos = _pos;
        buildingType = _type;
        rotation = _rot;
    }
}

public class EditorRemoveBuildingEvent
{
    public Vector3Int pos;
    
    public EditorRemoveBuildingEvent(Vector3Int _pos)
    {
        pos = _pos;
    }
}