using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject m_mainMenuObj;
    [SerializeField] GameObject m_selectSaveObj;
    [SerializeField] GameObject m_optionsObj;

    [SerializeField] GameObject m_oneSavePrefab;
    [SerializeField] GameObject m_newSavePrefab;

    [SerializeField] string m_editorSceneName;
    [SerializeField] float m_saveButtonsOffset;
    [SerializeField] Vector2 m_transitionSize;
    [SerializeField] float m_transitionDuration;

    private void Start()
    {
        
    }

    void OnClickPlay()
    {

    }

    void OnClickEditor()
    {

    }

    void OnClickOptions()
    {

    }

    void OnClickQuit()
    {

    }

    void OnSelectSave(int index)
    {

    }

    void OnDeleteSave(int index)
    {

    }

    void OnBack()
    {

    }

    GameObject CreateSaveButton(int index)
    {
        return null;
    }

    GameObject CreateNewButton(int index)
    {
        return null;
    }
}