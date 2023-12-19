using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OLDPauseUI : MonoBehaviour
{
    [SerializeField] string menuName;

    bool m_clicked = false;

    private void Start()
    {
        var background = transform.Find("Background");
        if(background != null)
        {
            var button = background.Find("ContinueButton")?.GetComponent<Button>();
            if(button != null)
                button.onClick.AddListener(ClickContinue);
            button = background.Find("QuitButton")?.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(ClickQuit);
        }

        Gamestate.instance.paused = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ClickContinue();
    }

    void ClickQuit()
    {
        if (m_clicked)
            return;
        m_clicked = true;

        SceneSystem.changeScene(menuName);

        Gamestate.instance.paused = false;
    }

    void ClickContinue()
    {
        if (m_clicked)
            return;
        m_clicked = true;

        Destroy(gameObject);

        Gamestate.instance.paused = false;
    }
}
