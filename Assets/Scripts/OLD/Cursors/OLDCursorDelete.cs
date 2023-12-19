using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OLDCursorDelete : OLDCursorBase
{
    protected override void OnLeftClick(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        var validation = ValidatePos(x, y);

        if (validation == OLDCursorValidation.warning)
            OLDGameSystem.Instance().RemoveTower(x, y);
        else if (validation == OLDCursorValidation.allowed)
            OLDGameSystem.Instance().RemoveGround(x, y);
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
        if (OLDWorldHolder.Instance() == null)
            return OLDCursorValidation.hidden;

        var groundType = OLDWorldHolder.Instance().GetGround(x, y);
        if (groundType == OLDGroundType.empty)
            return OLDCursorValidation.disallowed;

        var buildingType = OLDWorldHolder.Instance().GetBuilding(x, y);
        if (buildingType == OldBuildingType.tower0 || buildingType == OldBuildingType.tower1 || buildingType == OldBuildingType.tower2)
            return OLDCursorValidation.warning;

        return OLDCursorValidation.allowed;
    }
}
