using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public enum CursorType
{
    select,
    placeBuilding,
}

public class GameSystem : MonoBehaviour
{
    [SerializeField] LevelScriptableObject m_defaultLevel;
    [SerializeField] CursorPlaceBuilding m_placeBuildingCursor;
    [SerializeField] CursorSelect m_selectCursor;

    CursorType m_cursorType;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Start()
    {
        SetCurrentCursor(CursorType.select);

        LoadCurrentLevel();
    }

    private void Awake()
    {
        m_subscriberList.Add(new Event<EnableSelectCursorEvent>.Subscriber(EnableSelectCursor));
        m_subscriberList.Add(new Event<EnableBuildingCursorEvent>.Subscriber(EnableBuildingCursor));
        m_subscriberList.Add(new Event<GetCurrentCursorEvent>.Subscriber(GetCurrentCursor));

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

    void SetCurrentCursor(CursorType cursorType)
    {
        m_placeBuildingCursor.enabled = false;
        m_selectCursor.enabled = false;

        if (cursorType == CursorType.select)
            m_selectCursor.enabled = true;
        else if (cursorType == CursorType.placeBuilding)
            m_placeBuildingCursor.enabled = true;

        m_cursorType = cursorType;
    }

    void EnableSelectCursor(EnableSelectCursorEvent e)
    {
        SetCurrentCursor(CursorType.select);
    }

    void EnableBuildingCursor(EnableBuildingCursorEvent e)
    {
        SetCurrentCursor(CursorType.placeBuilding);

        m_placeBuildingCursor.SetBuilding(e.type, e.level);
    }

    void GetCurrentCursor(GetCurrentCursorEvent e)
    {
        e.cursorType = m_cursorType;
    }
}
