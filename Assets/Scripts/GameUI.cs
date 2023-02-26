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

    int currentButton = -1;
    ButtonInfo[] m_buttons;

    TMP_Text m_populationText;
    TMP_Text m_moneyText;

    TMP_Text m_waveLabelText;
    TMP_Text m_waveDescriptionText;

    private void Start()
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

    void OnClickGroundButton()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.ground].button.interactable = false;

    }

    void OnClickTower0Button()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.tower0].button.interactable = false;


    }

    void OnClickTower1Button()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.tower1].button.interactable = false;


    }

    void OnClickTower2Button()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.tower2].button.interactable = false;


    }

    void OnClickTrashBinButton()
    {
        ResetAllButtons();
        m_buttons[(int)ButtonType.trashCan].button.interactable = false;


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

    }
}
