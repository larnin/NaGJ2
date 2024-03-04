using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceBuildingEvent
{
    public Vector3Int pos;
    public BuildingType buildingType;
    public Rotation rotation;
    public Team team;

    public int ID;

    public PlaceBuildingEvent(Vector3Int _pos, BuildingType _type, Rotation _rot, Team _team)
    {
        pos = _pos;
        buildingType = _type;
        rotation = _rot;
        team = _team;
    }
}

public class RemoveBuildingEvent
{
    public int ID;

    public bool removed;

    public RemoveBuildingEvent(int _ID)
    {
        ID = _ID;
    }
}

public class GetBuildingEvent
{
    public int ID;

    public BuildingElement element;

    public GetBuildingEvent(int _ID)
    {
        ID = _ID;
    }
}

public class GetBuildingAtEvent
{
    public Vector3Int pos;

    public BuildingElement element;

    public GetBuildingAtEvent(Vector3Int _pos)
    {
        pos = _pos;
    }
}

public class GetBuildingByTypeEvent
{
    public BuildingType type;

    public BuildingElement element;

    public GetBuildingByTypeEvent(BuildingType _type)
    {
        type = _type;
    }
}

public class GetBuildingPortsEvent
{
    public List<BuildingOnePortData> ports;
    public List<BuildingContainer> containers;
}

public class SetBuildingInstanceIDEvent
{
    public int ID;

    public SetBuildingInstanceIDEvent(int _ID)
    {
        ID = _ID;
    }
}

public class GetBuildingInstanceIDEvent
{
    public int ID;
}

public class GetBuildingBeltsEvent
{
    public List<BuildingContainer> containers;
    public List<BuildingOnePortData> ports;
    public List<BuildingOneBeltData> belts;
}

public class BuildingUpdatedEvent
{
    public int ID;
    public BoundsInt bounds;

    public BuildingUpdatedEvent(int _ID, BoundsInt _bounds)
    {
        ID = _ID;
        bounds = _bounds;
    }
}