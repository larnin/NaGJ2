using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GetPopulationAndMoneyEvent
{
    public int population = 0;
    public int money = 0;
}

public class GetNearestBuildingEvent
{
    public Vector3 currentPos;
    public Vector2Int buildingPos;
    public bool buildingFound;

    public GetNearestBuildingEvent(Vector3 _currentPos)
    {
        currentPos = _currentPos;
        buildingPos = Vector2Int.zero;
        buildingFound = false;
    }
}

public class GetWaveTextEvent
{
    public string label = "";
    public string description = "";
}
public class GetMaxPopulationMoneyWaveEvent
{
    public int population = 0;
    public int money = 0;
    public int wave = 0;
}

public class GetSelectedCursorButtonEvent
{
    public int m_currentButton = -1;
}

public class SpawnerStopEvent { }

public class SpawnerGetStatusEvent
{
    public bool canSpawn = false;
    public bool stopped = false;
}

public class SpawnEntityEvent
{
    public GameObject prefab;
    public float multiplier;

    public SpawnEntityEvent(GameObject _prefab, float _multiplier)
    {
        prefab = _prefab;
        multiplier = _multiplier;
    }
}

public class SetBehaviourEnabledEvent
{
    public bool enabled;

    public SetBehaviourEnabledEvent(bool _enabled)
    {
        enabled = _enabled;
    }
}

public class CanBeTargetedEvent
{
    public bool targetable = true;
}
