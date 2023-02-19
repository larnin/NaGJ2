using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorGround : CursorBase
{
    List<Vector2Int> m_validPos = new List<Vector2Int>();

    GroundType m_groundType = GroundType.normal;

    private void OnEnable()
    {
        UpdateValidPos();
    }

    public void SetGroundType(GroundType type)
    {
        m_groundType = type;
    }

    protected override CursorValidation ValidatePos(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return CursorValidation.hidden;

        if (m_groundType == GroundType.empty)
        {
            return CursorValidation.neutral;
        }

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

        WorldHolder.Instance().SetGround(m_groundType, x, y);
    }

    protected override void OnRightClick(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return;

        if (WorldHolder.Instance().GetGround(x, y) != GroundType.empty)
            WorldHolder.Instance().SetGround(GroundType.empty, x, y);
    }

    protected override void OnMiddleClick(int x, int y)
    {
        //todo
    }

    protected override void OnUpdate()
    {
        //todo
    }

    void UpdateValidPos()
    {
        if(m_groundType == GroundType.empty)
        {
            m_validPos.Clear();
            return;
        }

        if (WorldHolder.Instance() != null)
            WorldHolder.Instance().GetEmptyGroundSpaces();
    }
}
