using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    [SerializeField] LevelScriptableObject m_defaultLevel;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void Awake()
    {
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void LoadCurrentLevel()
    {
        var levelElt = Gamestate.instance.currentLevel;
        if (levelElt == null)
            levelElt = m_defaultLevel;

        var doc = Json.ReadFromString(levelElt.data);

        LoadEvent e = new LoadEvent(doc);
        Event<LoadEvent>.Broadcast(e);
        Event<LoadEndedEvent>.Broadcast(new LoadEndedEvent());
    }
}
