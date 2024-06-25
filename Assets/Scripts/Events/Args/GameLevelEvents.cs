using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingUpdateEvent
{
    public int ID;
    public ElementUpdateType type;

    public BuildingUpdateEvent(int _ID, ElementUpdateType _type)
    {
        ID = _ID;
        type = _type;
    }
}

public class ResourceUpdateEvent
{
    public int ID;
    public ElementUpdateType type;

    public ResourceUpdateEvent(int _ID, ElementUpdateType _type)
    {
        ID = _ID;
        type = _type;
    }
}

public class BlockUpdateEvent
{
    public Vector3Int pos;

    public BlockUpdateEvent(Vector3Int _pos)
    {
        pos = _pos;
    }
}

public class GameSetCurrentLevelEvent
{
    public GameLevel level;

    public GameSetCurrentLevelEvent(GameLevel _level)
    {
        level = _level;
    }
}

public class GameGetCurrentLevelEvent
{
    public GameLevel level;
}

public class GameLoadEvent { }

public class GameResetEvent { }