using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CursorDelete : CursorBase
{
    protected override void OnLeftClick(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return;

        var validation = ValidatePos(x, y);

        if (validation == CursorValidation.warning)
            GameSystem.Instance().RemoveTower(x, y);
        else if (validation == CursorValidation.allowed)
            GameSystem.Instance().RemoveGround(x, y);
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
        if (WorldHolder.Instance() == null)
            return CursorValidation.hidden;

        var groundType = WorldHolder.Instance().GetGround(x, y);
        if (groundType == GroundType.empty)
            return CursorValidation.disallowed;

        var buildingType = WorldHolder.Instance().GetBuilding(x, y);
        if (buildingType == OldBuildingType.tower0 || buildingType == OldBuildingType.tower1 || buildingType == OldBuildingType.tower2)
            return CursorValidation.warning;

        return CursorValidation.allowed;
    }
}
