using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] string menuName;

    bool m_clicked = false;

    private void Start()
    {
        GetMaxPopulationMoneyWaveEvent e = new GetMaxPopulationMoneyWaveEvent();
        Event<GetMaxPopulationMoneyWaveEvent>.Broadcast(e);

        //really specific menus
        var back1 = transform.Find("Background");
        var back2 = back1?.Find("BackScore");
        if(back2 != null)
        {
            var label = back2.Find("WaveValue")?.GetComponent<TMP_Text>();
            if (label != null)
                label.text = e.wave.ToString();
            label = back2.Find("PopulationValue")?.GetComponent<TMP_Text>();
            if (label != null)
                label.text = e.population.ToString();
            label = back2.Find("MoneyValue")?.GetComponent<TMP_Text>();
            if (label != null)
                label.text = e.money.ToString();
        }
        if(back1 != null)
        {
            var button = back1.Find("Button")?.GetComponent<Button>();
            if(button != null)
                button.onClick.AddListener(OnClickButton);
        }

        Gamestate.instance.paused = true;
    }

    void OnClickButton()
    {
        if (m_clicked)
            return;
        m_clicked = true;

        SceneSystem.changeScene(menuName);

        Gamestate.instance.paused = false;
    }
}
