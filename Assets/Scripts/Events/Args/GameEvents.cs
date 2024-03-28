using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EnableSelectCursorEvent { }

public class EnableBuildingCursorEvent
{
    public BuildingType type;
    public int level;

    public EnableBuildingCursorEvent(BuildingType _type, int _level)
    {
        type = _type;
        level = _level;
    }
}

public class GetCurrentCursorEvent
{
    public CursorType cursorType;
}


