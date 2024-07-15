using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameWorldBehaviour : MonoBehaviour
{
    [SerializeField] float m_threadUpdate = 0.1f;

    GameWorld m_behaviour;

    static GameWorldBehaviour m_instance;

    SubscriberList m_subscriberList = new SubscriberList();

    bool m_clicked = false;
    int m_currentList = -1;
    string m_currentPath = "";

    private void Awake()
    {
        if(m_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        m_instance = this;
        DontDestroyOnLoad(gameObject);

        m_behaviour.Init(m_threadUpdate);

        m_subscriberList.Add(new Event<GameGetCurrentLevelEvent>.Subscriber(GetLevel));
        m_subscriberList.Subscribe();
    }

    private void Update()
    {
        m_behaviour.Update(Time.deltaTime);
    }

    private void OnDestroy()
    {
        m_behaviour.Destroy();
        m_subscriberList.Unsubscribe();
    }

    void GetLevel(GameGetCurrentLevelEvent e)
    {
        e.level = null;
        var oneLvl = m_behaviour.GetCurrentLevel();
        if (oneLvl != null)
            e.level = oneLvl.level;
    }
}
