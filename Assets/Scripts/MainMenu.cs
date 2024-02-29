using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NRand;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject m_mainMenuObj;
    [SerializeField] GameObject m_selectSaveObj;
    [SerializeField] GameObject m_optionsObj;

    [SerializeField] GameObject m_oneSavePrefab;
    [SerializeField] GameObject m_newSavePrefab;

    [SerializeField] float m_saveButtonsOffset;
    [SerializeField] Vector2 m_transitionSize;
    [SerializeField] float m_transitionDuration;

    [SerializeField] string m_editorLevelName;
    [SerializeField] string m_gameLevelName;

    enum MenuObject
    {
        main,
        selectSave,
        options,
    }

    Transform m_selectSaveCenter;

    MenuObject m_currentMenu = MenuObject.main;
    MenuObject m_nextMenu = MenuObject.main;

    Vector2 m_targetPos;
    float m_transitionPercent;

    bool m_selectedMenu;

    private void Start()
    {
        m_selectSaveCenter = m_selectSaveObj.transform.Find("Center");

        PopulateSelectSave();

        m_mainMenuObj.SetActive(false);
        m_selectSaveObj.SetActive(false);
        m_optionsObj.SetActive(false);
        GetInstance(m_currentMenu).SetActive(true);
    }

    public void OnClickPlay()
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;
        StartTransition(MenuObject.selectSave);
    }

    public void OnClickEditor()
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;
        SceneSystem.changeScene(m_editorLevelName);
    }

    public void OnClickOptions()
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;
        StartTransition(MenuObject.options);
    }

    public void OnClickQuit()
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;
        Application.Quit();
    }

    void OnSelectSave(int index)
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;
        SceneSystem.changeScene(m_gameLevelName);

        Save.instance.SelectSaveSlot(index);
    }

    void OnDeleteSave(int index)
    {
        if (m_selectedMenu)
            return;

        Save.instance.DeleteSave(index);

        for(int i = 0; i < m_selectSaveCenter.childCount; i++)
            Destroy(m_selectSaveCenter.GetChild(i));

        PopulateSelectSave();
    }

    public void OnBack()
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;
        StartTransition(MenuObject.main);
    }

    private void Update()
    {
        if (m_currentMenu != m_nextMenu)
        {
            m_transitionPercent += Time.deltaTime / m_transitionDuration;

            bool ended = false;
            if (m_transitionPercent >= 1)
            {
                m_transitionPercent = 1;
                ended = true;
            }

            float value = DOVirtual.EasedValue(0, 1, m_transitionPercent, Ease.InOutQuad);

            Vector3 currentPos = value * new Vector3(m_targetPos.x, m_targetPos.y, 0);
            Vector3 nextPos = - (1 - value) * new Vector3(m_targetPos.x, m_targetPos.y, 0);

            GetInstance(m_currentMenu).transform.localPosition = currentPos;
            GetInstance(m_nextMenu).transform.localPosition = nextPos;

            if (ended)
            {
                GetInstance(m_currentMenu).SetActive(false);
                m_currentMenu = m_nextMenu;

                m_selectedMenu = false;
            }
        }
    }

    void PopulateSelectSave()
    {
        int nbSave = Save.maxSaveSlots;

        float origin = -m_saveButtonsOffset * (nbSave - 1) / 2;

        for (int i = 0; i < nbSave; i++)
        {
            var button = CreateSaveButton(i);
            button.transform.SetParent(m_selectSaveCenter, false);
            button.transform.localPosition = new Vector3(origin + i * m_saveButtonsOffset, 0, 0);
        }
    }

    GameObject CreateSaveButton(int index)
    {
        var header = Save.instance.GetHeader(index);

        if(header == null || header.empty)
        {
            var buttonObj = Instantiate(m_newSavePrefab);
            var button = buttonObj.GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(() => { OnSelectSave(index); });

            return buttonObj;
        }
        else
        {
            var buttonObj = Instantiate(m_oneSavePrefab);
            var button = buttonObj.GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(() => { OnSelectSave(index); });

            var deleteElt = buttonObj.transform.Find("DeleteButton");
            if(deleteElt != null)
            {
                button = deleteElt.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => { OnDeleteSave(index); });
            }

            var timeElt = buttonObj.transform.Find("PlayTimeValue");
            if(timeElt != null)
            {
                var text = timeElt.GetComponent<TMP_Text>();
                if (text != null)
                    text.text = header.GetPlayTimeAsString();
            }

            return buttonObj;
        }
    }

    GameObject GetInstance(MenuObject type)
    {
        if (type == MenuObject.main)
            return m_mainMenuObj;
        if (type == MenuObject.options)
            return m_optionsObj;
        if (type == MenuObject.selectSave)
            return m_selectSaveObj;

        return null;
    }

    void StartTransition(MenuObject nextType)
    {
        if (m_currentMenu == nextType)
            return;

        m_nextMenu = nextType;

        Rotation rot = (Rotation)(new UniformIntDistribution(0, 3).Next(new StaticRandomGenerator<MT19937>()));

        Vector2 dir = RotationEx.ToVector(rot) * m_transitionSize;

        m_targetPos = dir;

        var obj = GetInstance(nextType);
        obj.SetActive(true);
        obj.transform.localPosition = new Vector3(-m_targetPos.x, -m_targetPos.y, 0);
        m_transitionPercent = 0;

        m_nextMenu = nextType;
    }
}