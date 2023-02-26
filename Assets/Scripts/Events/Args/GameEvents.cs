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

    public GetNearestBuildingEvent(Vector3 _currentPos)
    {
        currentPos = _currentPos;
    }
}
