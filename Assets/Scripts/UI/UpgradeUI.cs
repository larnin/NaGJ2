using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    Canvas m_canvas;
    RectTransform m_rect;

    TMP_Text m_label;
    TMP_Text m_costText;
    TMP_Text m_buttonLabel;

    Vector2Int m_pos;

    private void Awake()
    {
        m_canvas = GetComponent<Canvas>();
        var back = transform.Find("Background");
        if (back == null)
            return;
        m_rect = back.GetComponent<RectTransform>();

        m_label = back.Find("Label")?.GetComponent<TMP_Text>();
        m_costText = back.Find("CostText")?.GetComponent<TMP_Text>();

        var button = back.Find("UpgradeButton");
        if (button == null)
            return;
        m_buttonLabel = button.Find("Text")?.GetComponent<TMP_Text>();

        if(button != null)
        {
            var behaviour = button.GetComponent<Button>();
            if (behaviour != null)
                behaviour.onClick.AddListener(OnButtonClick);
        }
    }

    private void Start()
    {
        if (WorldHolder.Instance() == null)
            return;
        if (ElementHolder.Instance() == null)
            return;

        int level = 0;
        var building = WorldHolder.Instance().GetBuilding(m_pos.x, m_pos.y, out level);

        m_label.text = "Level " + (level + 1).ToString();

        int nbLevel = ElementHolder.Instance().GetMaxBuildingLevel(building);
        if(nbLevel <= level + 1)
        {
            m_buttonLabel.text = "MAX";
            m_costText.text = "0";
        }
        else
        {
            m_buttonLabel.text = "Upgrade";
            int cost = ElementHolder.Instance().GetBuildingCost(building, level + 1);
            m_costText.text = cost.ToString();
        }

        UpdatePos();
    }

    private void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            Destroy(gameObject, 0.1f);
        }

        UpdatePos();
    }

    public void SetPos(int x, int y)
    {
        m_pos = new Vector2Int(x, y);
    }

    void OnButtonClick()
    {

        int level = 0;
        var building = WorldHolder.Instance().GetBuilding(m_pos.x, m_pos.y, out level);

        m_label.text = "Level " + (level + 1).ToString();

        int nbLevel = ElementHolder.Instance().GetMaxBuildingLevel(building);
        if (nbLevel > level + 1)
        {
            int cost = ElementHolder.Instance().GetBuildingCost(building, level + 1);

            GetPopulationAndMoneyEvent e = new GetPopulationAndMoneyEvent();
            Event<GetPopulationAndMoneyEvent>.Broadcast(e);
            if (e.money >= cost)
                GameSystem.Instance().PlaceTower(m_pos.x, m_pos.y, building, level + 1);
        }

        Destroy(gameObject);
    }

    void UpdatePos()
    {
        //pos
        var pos = WorldHolder.Instance().GetElemPos(m_pos.x, m_pos.y);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        screenPos.y += 120;

        m_rect.position = screenPos;
    }
}
