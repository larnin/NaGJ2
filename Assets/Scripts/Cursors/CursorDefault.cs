using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorDefault : CursorBase
{
    [SerializeField] GameObject m_upgradeUIPrefab;

    GameObject m_upgradeUI;

    protected override void OnLeftClick(int x, int y)
    {
        if (WorldHolder.Instance() == null)
            return;

        if (m_upgradeUI != null)
            return;

        var buildingType = WorldHolder.Instance().GetBuilding(x, y);

        if (buildingType == BuildingType.tower0 || buildingType == BuildingType.tower1 || buildingType == BuildingType.tower2)
            SpawnUpgradeUI(x, y);
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

    private void OnDisable()
    {
        if (m_upgradeUI != null)
            Destroy(m_upgradeUI);
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

    void SpawnUpgradeUI(int x, int y)
    {
        var obj = Instantiate(m_upgradeUIPrefab);

        var ui = obj.GetComponent<UpgradeUI>();
        if (ui != null)
            ui.SetPos(x, y);

        m_upgradeUI = obj;
    }
}
