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
    public int buildingID = 0;
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

public class EditorGetCursorTypeEvent
{
    public EditorCursorType cursorType;
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
    public int level;
    public Team team;

    public EditorSetCursorBuildingEvent(BuildingType _type, int _level, Team _team)
    {
        type = _type;
        level = _level;
    }
}

public class EditorGetCursorBuildingEvent
{
    public BuildingType type;
    public Rotation rotation;
    public int level;
    public Team team;
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

    public BuildingElement building;

    public EditorGetBuildingEvent(int _id)
    {
        ID = _id;
    }
}

public class EditorGetBuildingAtEvent
{
    public Vector3Int pos;

    public BuildingElement building;

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
    public int level;

    public int ID;

    public EditorPlaceBuildingEvent(Vector3Int _pos, BuildingType _type, Rotation _rot, Team _team, int _level)
    {
        pos = _pos;
        buildingType = _type;
        rotation = _rot;
        team = _team;
        level = _level;
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

public class EditorSetBuildingInstanceEvent
{
    public int ID;

    public EditorSetBuildingInstanceEvent(int _id)
    {
        ID = _id;
    }
}

public class EditorUpdateInstanceEvent { }

public class EditorSelectBlockTypeEvent
{
    public BlockType type;
    public int value;

    public EditorSelectBlockTypeEvent(BlockType _type, int _value)
    {
        type = _type;
        value = _value;
    }
}

public class EditorSelectBuildingTypeEvent
{
    public BuildingType type;
    public int level;

    public EditorSelectBuildingTypeEvent(BuildingType _type, int _level)
    {
        type = _type;
        level = _level;
    }
}

public class EditorCursorOnUIEvent
{
    public Vector3 pos;
    public bool onUI;

    public EditorCursorOnUIEvent(Vector3 _pos)
    {
        pos = _pos;
        onUI = false;
    }
}

public class EditorDrawWindowNextFrameEvent
{
    public int windowID;
    public Rect rect;
    public GUI.WindowFunction func;
    public string label;
    public bool modal;

    public EditorDrawWindowNextFrameEvent(int _windowID, Rect _rect, GUI.WindowFunction _func, string _label, bool _modal = false)
    {
        windowID = _windowID;
        rect = _rect;
        func = _func;
        label = _label;
        modal = _modal;
    }
}

public class EditorHideGismosEvent { }
