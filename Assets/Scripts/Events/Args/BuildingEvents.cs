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
    public int level;
    public Rotation rotation;
    public Team team;

    public int ID;

    public PlaceBuildingEvent(Vector3Int _pos, BuildingType _type, int _level, Rotation _rot, Team _team)
    {
        pos = _pos;
        buildingType = _type;
        level = _level;
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
    public List<BuildingOnePortData> ports = new List<BuildingOnePortData>();
    public List<BuildingContainer> containers = new List<BuildingContainer>();
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
    public List<BuildingContainer> containers = new List<BuildingContainer>();
    public List<BuildingOnePortData> ports = new List<BuildingOnePortData>();
    public List<BuildingOneBeltData> belts = new List<BuildingOneBeltData>();
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

public class GetNearBeltsEvent
{
    public Vector3Int pos;
    public NearMatrix3<SimpleBeltInfos> matrix = new NearMatrix3<SimpleBeltInfos>();

    public GetNearBeltsEvent(Vector3Int _pos)
    {
        pos = _pos;

        var data = new SimpleBeltInfos { haveBelt = false, rotation = Rotation.rot_0 };

        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                    matrix.Set(data, i, j, k);
    }
}