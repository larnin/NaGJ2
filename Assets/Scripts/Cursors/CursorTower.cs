using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CursorTower : CursorBase
{
    BuildingType m_building = BuildingType.empty;

    public void SetBuilding(BuildingType building)
    {
        m_building = building;
    }

    protected override void OnLeftClick(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return;

        if (ValidatePos(x, y) != CursorValidation.allowed)
            return;

        GameSystem.Instance().PlaceTower(x, y, m_building, 0);
    }

    protected override void OnMiddleClick(int x, int y)
    {
        //todo
    }

    protected override void OnRightClick(int x, int y)
    {
        //todo
    }

    protected override void OnUpdate()
    {
        //todo
    }

    protected override CursorValidation ValidatePos(int x, int y)
    {
        if (ElementHolder.Instance() == null)
            return CursorValidation.hidden;
        if (WorldHolder.Instance() == null)
            return CursorValidation.hidden;

        int cost = ElementHolder.Instance().GetBuildingCost(m_building, 0);

        GetPopulationAndMoneyEvent e = new GetPopulationAndMoneyEvent();
        Event<GetPopulationAndMoneyEvent>.Broadcast(e);
        if (e.money < cost)
            return CursorValidation.warning;

        var groundType = WorldHolder.Instance().GetGround(x, y);

        if (groundType == GroundType.empty)
            return CursorValidation.disallowed;

        var buildingType = WorldHolder.Instance().GetBuilding(x, y);

        if (buildingType == BuildingType.tower0 || buildingType == BuildingType.tower1 || buildingType == BuildingType.tower2)
            return CursorValidation.disallowed;

        return CursorValidation.allowed;
    }
}
