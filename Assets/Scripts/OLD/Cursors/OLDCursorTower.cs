using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OLDCursorTower : OLDCursorBase
{
    OldBuildingType m_building = OldBuildingType.empty;

    public void SetBuilding(OldBuildingType building)
    {
        m_building = building;
    }

    protected override void OnLeftClick(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        if (ValidatePos(x, y) != OLDCursorValidation.allowed)
            return;

        OLDGameSystem.Instance().PlaceTower(x, y, m_building, 0);
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

    protected override OLDCursorValidation ValidatePos(int x, int y)
    {
        if (OLDElementHolder.Instance() == null)
            return OLDCursorValidation.hidden;
        if (OLDWorldHolder.Instance() == null)
            return OLDCursorValidation.hidden;

        int cost = OLDElementHolder.Instance().GetBuildingCost(m_building, 0);

        GetPopulationAndMoneyEvent e = new GetPopulationAndMoneyEvent();
        Event<GetPopulationAndMoneyEvent>.Broadcast(e);
        if (e.money < cost)
            return OLDCursorValidation.warning;

        var groundType = OLDWorldHolder.Instance().GetGround(x, y);

        if (groundType == OLDGroundType.empty)
            return OLDCursorValidation.disallowed;

        var buildingType = OLDWorldHolder.Instance().GetBuilding(x, y);

        if (buildingType == OldBuildingType.tower0 || buildingType == OldBuildingType.tower1 || buildingType == OldBuildingType.tower2)
            return OLDCursorValidation.disallowed;

        return OLDCursorValidation.allowed;
    }
}
