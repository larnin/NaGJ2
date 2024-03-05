using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] string m_mainMenuName;

    bool m_clicked = false;

    private void Start()
    {
        Gamestate.instance.paused = true;
    }

    private void OnDestroy()
    {
        Gamestate.instance.paused = false;
    }

    public void OnResumeClick()
    {
        if (m_clicked)
            return;

        m_clicked = true;
        Destroy(gameObject);
    }

    public void OnQuitClick()
    {
        if (m_clicked)
            return;

        m_clicked = true;
        SceneSystem.changeScene(m_mainMenuName);
    }
}
