using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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