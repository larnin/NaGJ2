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
