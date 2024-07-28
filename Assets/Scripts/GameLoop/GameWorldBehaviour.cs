using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameWorldBehaviour : MonoBehaviour
{
    [SerializeField] float m_threadUpdate = 0.1f;

    GameWorld m_behaviour = new GameWorld();

    static GameWorldBehaviour m_instance;

    SubscriberList m_subscriberList = new SubscriberList();

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

    private void OnEnable()
    {
        m_behaviour.SetCurrentWorldIndex(0);

        var oneLvl = m_behaviour.GetCurrentLevel();

        if (oneLvl != null)
            Event<GameSetCurrentLevelEvent>.Broadcast(new GameSetCurrentLevelEvent(oneLvl.level));

        Event<GameResetEvent>.Broadcast(new GameResetEvent());
        Event<GameLoadEvent>.Broadcast(new GameLoadEvent());

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
