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
    public int ID;
    public bool haveBuilding;

    public EditorHaveBuildingEvent(int _id)
    {
        ID = _id;
    }
}

public class EditorGetBuildingEvent
{
    public int ID;

    public Vector3Int buildingPos;
    public BuildingType buildingType;
    public Rotation rotation;
    public Team team;

    public EditorGetBuildingEvent(int _id)
    {
        ID = _id;
    }
}

public class EditorGetBuildingAtEvent
{
    public Vector3Int pos;

    public int ID;
    public Vector3Int buildingPos;
    public BuildingType buildingType;
    public Rotation rotation;
    public Team team;

    public EditorGetBuildingAtEvent(Vector3Int _pos)
    {
        pos = _pos;
    }
}

public class EditorCanPlaceBuildingEvent
{
    public Vector3Int pos;
    public BuildingType buildingType;
    public Rotation rotation;
    public Team team;

    public bool canBePlaced;

    public EditorCanPlaceBuildingEvent(Vector3Int _pos, BuildingType _type, Rotation _rot, Team _team)
    {
        pos = _pos;
        buildingType = _type;
        rotation = _rot;
        team = _team;
    }
}

public class EditorPlaceBuildingEvent
{
    public Vector3Int pos;
    public BuildingType buildingType;
    public Rotation rotation;
    public Team team;

    public int ID;

    public EditorPlaceBuildingEvent(Vector3Int _pos, BuildingType _type, Rotation _rot, Team _team)
    {
        pos = _pos;
        buildingType = _type;
        rotation = _rot;
        team = _team;
    }
}

public class EditorRemoveBuildingEvent
{
    public int ID;
    
    public EditorRemoveBuildingEvent(int _id)
    {
        ID = _id;
    }
}

public class EditorSetBuildingInstance
{
    public int ID;

    public EditorSetBuildingInstance(int _id)
    {
        ID = _id;
    }
}
