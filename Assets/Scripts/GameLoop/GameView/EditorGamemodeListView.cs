using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorGamemodeListView : MonoBehaviour
{
    static EditorGamemodeListView m_current = null;
    public static EditorGamemodeListView GetCurrent() { return m_current; }

    GameLevel m_level;

    List<EditorGamemodeViewBase> m_modeViews = new List<EditorGamemodeViewBase>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_current = this;

        m_subscriberList.Add(new Event<GameSetCurrentLevelEvent>.Subscriber(SetLevel));
        m_subscriberList.Add(new Event<GameResetEvent>.Subscriber(Reset));
        m_subscriberList.Add(new Event<GameLoadEvent>.Subscriber(AfterLoad)); 
        m_subscriberList.Add(new Event<EditorHideGismosEvent>.Subscriber(OnHideGismos));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();

        for (int i = 0; i < m_modeViews.Count; i++)
        {
            if (m_modeViews[i] != null)
                m_modeViews[i].OnDestroy();
        }

        m_modeViews.Clear();
    }

    void SetLevel(GameSetCurrentLevelEvent e)
    {
        m_level = e.level;
    }

    void Reset(GameResetEvent e)
    {
        ResetData();
    }

    void ResetData()
    {
        m_modeViews.Clear();
    }

    void AfterLoad(GameLoadEvent e)
    {

    }

    void OnHideGismos(EditorHideGismosEvent e)
    {
        if (m_level == null)
            return;

        foreach(var m in m_modeViews)
        {
            m.HideGismos();
        }
    }

    private void Update()
    {
        if (m_level == null)
            return;

        UpdateList();
    }

    void UpdateList()
    {
        List<EditorGamemodeViewBase> newList = new List<EditorGamemodeViewBase>();
        int nbMode = m_level.gamemode.GetGamemodeNb();
        for (int i = 0; i < nbMode; i++)
        {
            GamemodeBase mode = m_level.gamemode.GetGamemode(i);
            EditorGamemodeViewBase view = null;

            for (int j = 0; j < m_modeViews.Count; j++)
            {
                if (m_modeViews[j] == null)
                    continue;

                if (m_modeViews[j].GetGamemode() == mode)
                {
                    view = m_modeViews[j];
                    m_modeViews[j] = null;
                }
            }

            if (view == null)
            {
                view = mode.CreateEditorView();
                view.SetParent(transform);
                view.Init();
            }

            newList.Add(view);
        }

        for (int i = 0; i < m_modeViews.Count; i++)
        {
            if (m_modeViews[i] != null)
                m_modeViews[i].OnDestroy();
        }

        m_modeViews = newList;
    }

    public int GetGamemodeViewNb()
    {
        return m_modeViews.Count;
    }

    public EditorGamemodeViewBase GetGamemodeViewFromIndex(int index)
    {
        if (index < 0 || index >= m_modeViews.Count)
            return null;
        return m_modeViews[index];
    }

    public EditorGamemodeViewBase GetGamemodeView(GamemodeBase mode)
    {
        foreach(var view in m_modeViews)
        {
            if (view.GetGamemode() == mode)
                return view;
        }

        return null;
    }
}
