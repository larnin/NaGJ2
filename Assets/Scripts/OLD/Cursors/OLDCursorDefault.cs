using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDCursorDefault : OLDCursorBase
{
    [SerializeField] GameObject m_upgradeUIPrefab;

    GameObject m_upgradeUI;

    protected override void OnLeftClick(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        if (m_upgradeUI != null)
            return;

        var buildingType = OLDWorldHolder.Instance().GetBuilding(x, y);

        if (buildingType == OldBuildingType.tower0 || buildingType == OldBuildingType.tower1 || buildingType == OldBuildingType.tower2)
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

    protected override OLDCursorValidation ValidatePos(int x, int y)
    {
        if (OLDWorldHolder.Instance() == null)
            return OLDCursorValidation.hidden;

        var ground = OLDWorldHolder.Instance().GetGround(x, y);

        if (ground == OLDGroundType.empty)
            return OLDCursorValidation.hidden;

        return OLDCursorValidation.neutral;
    }

    void SpawnUpgradeUI(int x, int y)
    {
        var obj = Instantiate(m_upgradeUIPrefab);

        var ui = obj.GetComponent<OLDUpgradeUI>();
        if (ui != null)
            ui.SetPos(x, y);

        m_upgradeUI = obj;
    }
}
