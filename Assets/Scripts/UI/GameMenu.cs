using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [SerializeField] Sprite m_playSprite;
    [SerializeField] Sprite m_pauseSprite;
    [SerializeField] GameObject m_pausePrefab;
    [SerializeField] Image m_pauseButtonImage;
    [SerializeField] List<BuildingType> m_buildingList;
    [SerializeField] GameObject m_buildingListObject;
    [SerializeField] GameObject m_detailInterface;
    [SerializeField] GameObject m_detailCostLinePrefab;
    [SerializeField] int m_maxDetailCostLine = 3;
    [SerializeField] float m_detailCostLineOffset;

    GameObject m_pauseInstance;
    bool m_paused = false;

    private void Start()
    {
        m_detailInterface.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnPauseClick();

        if (m_paused && m_pauseInstance == null)
            OnPauseClick();
    }

    public void OnPauseClick()
    {
        if(m_paused)
        {
            if (m_pauseInstance != null)
                Destroy(m_pauseInstance);

            m_pauseButtonImage.sprite = m_playSprite;
        }
        else
        {
            m_pauseInstance = Instantiate(m_pausePrefab);
            m_pauseInstance.transform.SetParent(transform, false);
            m_pauseInstance.transform.localPosition = Vector3.zero;

            m_pauseButtonImage.sprite = m_pauseSprite;
        }

        m_paused = !m_paused;
    }

    public void OnEmptyClick()
    {
        //todo
    }

    public void OnBuildingListClick()
    {
        m_buildingListObject.SetActive(!m_buildingListObject.activeSelf);
        if (!m_buildingListObject.activeSelf)
            m_detailInterface.SetActive(false);
    }

    public void OnBuildingClick(int index)
    {
        //todo
    }

    public void OnHoverStart(int index)
    {
        if(index < 0 || index > m_buildingList.Count)
        {
            OnHoverEnd();
            return;
        }

        UpdateDetails(m_buildingList[index]);
    }

    public void OnHoverEnd()
    {
        m_detailInterface.SetActive(false);
    }

    void UpdateDetails(BuildingType type)
    {
        m_detailInterface.SetActive(true);

        var nameObj = m_detailInterface.transform.Find("Name");
        if(nameObj != null)
        {
            var nameTxt = nameObj.GetComponent<TMP_Text>();
            if (nameTxt != null)
                nameTxt.SetText(BuildingDataEx.GetName(type));
        }

        var descObj = m_detailInterface.transform.Find("Description");
        if(descObj != null)
        {
            var descTxt = descObj.GetComponent<TMP_Text>();
            if (descTxt != null)
                descTxt.SetText(BuildingDataEx.GetDescription(type));
        }

        var resources = Global.instance.allResources;

        var costObj = m_detailInterface.transform.Find("Cost");
        if(costObj != null)
        {
            var centerObj = costObj.transform.Find("CostCenter");
            var cost = BuildingDataEx.GetCost(type);

            if(centerObj != null && cost != null)
            {
                while (centerObj.childCount > 0)
                {
                    var obj = centerObj.GetChild(0);
                    obj.SetParent(null, false);
                    Destroy(obj.gameObject);
                }

                int nbCost = cost.costs.Count;
                if (nbCost > m_maxDetailCostLine)
                    nbCost = m_maxDetailCostLine;

                float origin = -(nbCost - 1) * m_detailCostLineOffset / 2.0f;

                for (int i = 0; i < nbCost; i++)
                {
                    var oneLine = Instantiate(m_detailCostLinePrefab);
                    oneLine.transform.SetParent(centerObj, false);
                    oneLine.transform.localPosition = new Vector3(origin + i * m_detailCostLineOffset, 0, 0);

                    var resource = resources.Get(cost.costs[i].resourceType);

                    var image = oneLine.GetComponentInChildren<Image>();
                    if (image != null)
                        image.sprite = resource.sprite;

                    var text = oneLine.GetComponentInChildren<TMP_Text>();
                    if (text != null)
                        text.SetText(cost.costs[i].count.ToString());
                }
            }
        }
    }
}
