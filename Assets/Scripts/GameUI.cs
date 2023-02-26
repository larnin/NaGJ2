using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    enum ButtonType
    {
        ground,
        tower0,
        tower1,
        tower2,
        trashCan,
    }

    class ButtonInfo
    {
        public Button button;
        public TMP_Text cost;
    }

    [SerializeField] CursorDefault m_defaultCursor;
    [SerializeField] CursorGround m_groundCursor;
    [SerializeField] CursorTower m_towerCursor;
    [SerializeField] CursorDelete m_deleteCursor;

    int m_currentButton = -1;
    ButtonInfo[] m_buttons;

    TMP_Text m_populationText;
    TMP_Text m_moneyText;

    TMP_Text m_waveLabelText;
    TMP_Text m_waveDescriptionText;

    bool m_pricesUpdated = false;

    private void Awake()
    {
        //explore that shit of hierarchy !

        var back = transform.Find("Back");
        if (back == null)
            return;
        var pop = back.Find("Population");
        if (pop != null)
            m_populationText = pop.Find("Value")?.GetComponent<TMP_Text>();
        var money = back.Find("Money");
        if (money != null)
            m_moneyText = money.Find("Value")?.GetComponent<TMP_Text>();

        m_buttons = new ButtonInfo[Enum.GetValues(typeof(ButtonType)).Length];
        for (int i = 0; i < m_buttons.Length; i++)
            m_buttons[i] = new ButtonInfo();

        m_buttons[(int)ButtonType.ground].button = back.Find("GroundButton")?.GetComponent<Button>();
        m_buttons[(int)ButtonType.tower0].button = back.Find("Tower0Button")?.GetComponent<Button>();
        m_buttons[(int)ButtonType.tower1].button = back.Find("Tower1Button")?.GetComponent<Button>();
        m_buttons[(int)ButtonType.tower2].button = back.Find("Tower2Button")?.GetComponent<Button>();
        m_buttons[(int)ButtonType.trashCan].button = back.Find("DeleteButton")?.GetComponent<Button>();
        
        for(int i = 0; i < m_buttons.Length; i++)
        {
            if (m_buttons[i].button == null)
                return; //stop here, we must have found every button
            m_buttons[i].cost = m_buttons[i].button.transform.Find("Cost")?.GetComponent<TMP_Text>();
        }

        m_buttons[(int)ButtonType.ground].button.onClick.AddListener(OnClickGroundButton);
        m_buttons[(int)ButtonType.tower0].button.onClick.AddListener(OnClickTower0Button);
        m_buttons[(int)ButtonType.tower1].button.onClick.AddListener(OnClickTower1Button);
        m_buttons[(int)ButtonType.tower2].button.onClick.AddListener(OnClickTower2Button);
        m_buttons[(int)ButtonType.trashCan].button.onClick.AddListener(OnClickTrashBinButton);

        var wave = back.Find("WaveBack");
        if(wave != null)
        {
            m_waveLabelText = wave.Find("NextWaveLabel")?.GetComponent<TMP_Text>();
            m_waveDescriptionText = wave.Find("NextWaveText")?.GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        UpdateCursor();
    }

    void OnClickGroundButton()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.ground].button.interactable = false;

        m_currentButton = (int)ButtonType.ground;

        UpdateCursor();
    }

    void OnClickTower0Button()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.tower0].button.interactable = false;

        m_currentButton = (int)ButtonType.tower0;

        UpdateCursor();
    }

    void OnClickTower1Button()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.tower1].button.interactable = false;

        m_currentButton = (int)ButtonType.tower1;

        UpdateCursor();
    }

    void OnClickTower2Button()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.tower2].button.interactable = false;

        m_currentButton = (int)ButtonType.tower2;

        UpdateCursor();
    }

    void OnClickTrashBinButton()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.trashCan].button.interactable = false;

        m_currentButton = (int)ButtonType.trashCan;

        UpdateCursor();
    }

    void ResetAllButtons()
    {
        for (int i = 0; i < m_buttons.Length; i++)
            m_buttons[i].button.interactable = true;
    }

    private void Update()
    {
        UpdatePopAndMoney();
        UpdateWave();

        if (!m_pricesUpdated)
            UpdatePrices();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            m_currentButton = -1;
            ResetAllButtons();
            UpdateCursor();
        }
    }

    void UpdatePopAndMoney()
    {
        GetPopulationAndMoneyEvent e = new GetPopulationAndMoneyEvent();
        Event<GetPopulationAndMoneyEvent>.Broadcast(e);

        m_populationText.text = e.population.ToString();
        m_moneyText.text = e.money.ToString();
    }

    void UpdateWave()
    {
        //todo
    }

    void UpdatePrices()
    {
        if (ElementHolder.Instance() == null)
            return;

        m_pricesUpdated = true;

        int priceGround = ElementHolder.Instance().GetGroundCost(GroundType.normal);
        int priceTower0 = ElementHolder.Instance().GetBuildingCost(BuildingType.tower0, 0);
        int priceTower1 = ElementHolder.Instance().GetBuildingCost(BuildingType.tower1, 0);
        int priceTower2 = ElementHolder.Instance().GetBuildingCost(BuildingType.tower2, 0);

        m_buttons[(int)ButtonType.ground].cost.text = priceGround.ToString();
        m_buttons[(int)ButtonType.tower0].cost.text = priceTower0.ToString();
        m_buttons[(int)ButtonType.tower1].cost.text = priceTower1.ToString();
        m_buttons[(int)ButtonType.tower2].cost.text = priceTower2.ToString();
    }

    void UpdateCursor()
    {
        m_defaultCursor.gameObject.SetActive(false);
        m_groundCursor.gameObject.SetActive(false);
        m_towerCursor.gameObject.SetActive(false);
        m_deleteCursor.gameObject.SetActive(false);

        if (m_currentButton == -1)
            m_defaultCursor.gameObject.SetActive(true);
        else if (m_currentButton == (int)ButtonType.ground)
            m_groundCursor.gameObject.SetActive(true);
        else if (m_currentButton == (int)ButtonType.trashCan)
            m_deleteCursor.gameObject.SetActive(true);
        else
        {
            m_towerCursor.gameObject.SetActive(true);
            if (m_currentButton == (int)ButtonType.tower0)
                m_towerCursor.SetBuilding(BuildingType.tower0);
            else if (m_currentButton == (int)ButtonType.tower1)
                m_towerCursor.SetBuilding(BuildingType.tower1);
            else if (m_currentButton == (int)ButtonType.tower2)
                m_towerCursor.SetBuilding(BuildingType.tower2);
        }

    }
}
