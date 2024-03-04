using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LevelSelectDifficulty
{
    easy,
    medium,
    hard,
    impossible
}

[Serializable]
public class LevelSelectOneLevel
{
    public string name;
    public LevelScriptableObject level;
    public Sprite preview;
    public LevelSelectDifficulty difficulty;
}

public class LevelSelect : MonoBehaviour
{
    [SerializeField] List<LevelSelectOneLevel> m_levels;
    [SerializeField] GameObject m_leftButton;
    [SerializeField] GameObject m_rightButton;
    [SerializeField] string m_previousMenuName;
    [SerializeField] string m_gameName;

    [SerializeField] GameObject m_oneLevelPrefab;
    [SerializeField] int m_nbLevelPerPage = 5;
    [SerializeField] float m_levelOffset = 200;

    bool m_selectedMenu = false;
    int m_currentPage = 0;

    Transform m_levelsTransform;

    private void Start()
    {
        m_levelsTransform = transform.GetChild(0);
        m_levelsTransform = m_levelsTransform.Find("LevelsArea");

        PopulatePage(m_currentPage);
    }

    void PopulatePage(int index)
    {
        while (m_levelsTransform.childCount > 0)
        {
            var obj = m_levelsTransform.GetChild(0);
            obj.SetParent(null, false);
            Destroy(obj.gameObject);
        }

        if (m_levels == null)
            return;

        int first = index * m_nbLevelPerPage;
        int last = (index + 1) * m_nbLevelPerPage;

        float origin = (m_nbLevelPerPage - 1) / 2.0f * m_levelOffset;

        for(int i = first; i < last; i++)
        {
            if (i < 0 || i >= m_levels.Count)
                continue;

            var obj = Instantiate(m_oneLevelPrefab);
            obj.transform.SetParent(m_levelsTransform, false);

            obj.transform.localPosition = new Vector3(0, origin - (i - first) * m_levelOffset, 0);

            var button = obj.GetComponentInChildren<Button>();
            if (button != null)
            {
                int levelIndex = i;
                button.onClick.AddListener(()=> { OnSelectLevel(levelIndex); });
            }

            var level = m_levels[i];

            var titleObj = obj.transform.Find("Name");
            if(titleObj != null)
            {
                var titleText = titleObj.GetComponent<TMP_Text>();
                if (titleText != null)
                    titleText.text = level.name;
            }

            var difficultyObj = obj.transform.Find("DifficultyText");
            if(difficultyObj != null)
            {
                var difficultyText = difficultyObj.GetComponent<TMP_Text>();
                if (difficultyText != null)
                    difficultyText.text = level.difficulty.ToString();
            }

            var playTimeLabel = obj.transform.Find("PlaytimeLabel");
            var playTimeObj = obj.transform.Find("PlaytimeValue");
            TMP_Text playTimeText = null;
            if (playTimeObj != null)
                playTimeText = playTimeObj.GetComponent<TMP_Text>();

            if(playTimeLabel != null && playTimeText != null)
            {
                // todo display real playtime from this level

                playTimeLabel.gameObject.SetActive(false);
                playTimeText.text = "New !";
            }

            var img = obj.GetComponentInChildren<Image>();
            if (img != null && level.preview != null)
                img.sprite = level.preview;
        }

        m_leftButton.SetActive(first > 0);
        m_rightButton.SetActive(last < m_levels.Count);
    }

    public void OnSelectLevel(int index)
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;

        Gamestate.instance.currentLevel = m_levels[index].level;

        SceneSystem.changeScene(m_gameName);
    }

    public void OnBack()
    {
        if (m_selectedMenu)
            return;

        m_selectedMenu = true;

        SceneSystem.changeScene(m_previousMenuName);
    }

    public void OnLeft()
    {
        if (m_selectedMenu)
            return;

        m_currentPage--;
        PopulatePage(m_currentPage);
    }

    public void OnRight()
    {
        if (m_selectedMenu)
            return;

        m_currentPage++;
        PopulatePage(m_currentPage);
    }
}
