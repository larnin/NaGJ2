using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CursorDefault : CursorBase
{
    protected override void OnLeftClick(int x, int y)
    {
        //todo click on tower
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

        var ground = WorldHolder.Instance().GetGround(x, y);

        if (ground == GroundType.empty)
            return CursorValidation.hidden;

        return CursorValidation.neutral;
    }
}
