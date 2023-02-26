using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorGround : CursorBase
{
    private void OnEnable()
    {

    }

    protected override CursorValidation ValidatePos(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return CursorValidation.hidden;

        if (ElementHolder.Instance() == null)
            return CursorValidation.hidden;

        int cost = ElementHolder.Instance().GetGroundCost(GroundType.normal);

        GetPopulationAndMoneyEvent e = new GetPopulationAndMoneyEvent();
        Event<GetPopulationAndMoneyEvent>.Broadcast(e);
        if (e.money < cost)
            return CursorValidation.warning;

        var mat = WorldHolder.Instance().GetGroundNearMatrix(x, y);

        if (mat.Get(-1, 0) || mat.Get(0, -1) || mat.Get(1, 0) || mat.Get(0, 1))
            return CursorValidation.allowed;
        return CursorValidation.disallowed;
    }

    protected override void OnLeftClick(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return;

        if (ValidatePos(x, y) != CursorValidation.allowed)
            return;

        GameSystem.Instance().PlaceGround(x, y);
    }

    protected override void OnRightClick(int x, int y)
    {
        //todo

        //if (WorldHolder.Instance() == null)
        //    return;

        //if (WorldHolder.Instance().GetGround(x, y) != GroundType.empty)
        //    WorldHolder.Instance().SetGround(GroundType.empty, x, y);
    }

    protected override void OnMiddleClick(int x, int y)
    {
        //todo
    }

    protected override void OnUpdate()
    {
        //todo
    }
}
