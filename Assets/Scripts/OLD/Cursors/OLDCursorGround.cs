using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDCursorGround : OLDCursorBase
{
    private void OnEnable()
    {

    }

    protected override OLDCursorValidation ValidatePos(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return OLDCursorValidation.hidden;

        if (OLDElementHolder.Instance() == null)
            return OLDCursorValidation.hidden;

        int cost = OLDElementHolder.Instance().GetGroundCost(OLDGroundType.normal);

        GetPopulationAndMoneyEvent e = new GetPopulationAndMoneyEvent();
        Event<GetPopulationAndMoneyEvent>.Broadcast(e);
        if (e.money < cost)
            return OLDCursorValidation.warning;

        var mat = OLDWorldHolder.Instance().GetGroundNearMatrix(x, y);

        if (mat.Get(-1, 0) || mat.Get(0, -1) || mat.Get(1, 0) || mat.Get(0, 1))
            return OLDCursorValidation.allowed;
        return OLDCursorValidation.disallowed;
    }

    protected override void OnLeftClick(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        if (ValidatePos(x, y) != OLDCursorValidation.allowed)
            return;

        OLDGameSystem.Instance().PlaceGround(x, y);
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
