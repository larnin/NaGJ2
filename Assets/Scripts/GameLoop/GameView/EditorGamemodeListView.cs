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

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_current = this;

        m_subscriberList.Add(new Event<GameSetCurrentLevelEvent>.Subscriber(SetLevel));
        m_subscriberList.Add(new Event<GameResetEvent>.Subscriber(Reset));
        m_subscriberList.Add(new Event<GameLoadEvent>.Subscriber(AfterLoad));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
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
    }

    void AfterLoad(GameLoadEvent e)
    {

    }
}
