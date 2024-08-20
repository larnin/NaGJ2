using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEntityListView : MonoBehaviour
{
    GameLevel m_level;

    Dictionary<int, GameObject> m_entities = new Dictionary<int, GameObject>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GameSetCurrentLevelEvent>.Subscriber(SetLevel));
        m_subscriberList.Add(new Event<GameResetEvent>.Subscriber(Reset));
        m_subscriberList.Add(new Event<GameLoadEvent>.Subscriber(AfterLoad));
        m_subscriberList.Add(new Event<EntityUpdateEvent>.Subscriber(OnEntityUpdate));
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

        int nbEntity = m_level.entityList.GetEntityNb();
        for (int i = 0; i < nbEntity; i++)
            UpdateRender(i);
    }

    void ResetData()
    {
        foreach (var b in m_entities)
        {
            if (b.Value != null)
                Destroy(b.Value);
        }

        m_entities.Clear();
    }

    void AfterLoad(GameLoadEvent e)
    {
        ResetData();

        if (m_level == null)
            return;

        int nbEntity = m_level.entityList.GetEntityNb();
        for (int i = 0; i < nbEntity; i++)
            UpdateRender(i);
    }

    void OnEntityUpdate(EntityUpdateEvent e)
    {
        GameObject oldInstance = null;
        m_entities.TryGetValue(e.ID, out oldInstance);
        GameEntity oldEntity = null;
        if (oldInstance != null)
            oldEntity = oldInstance.GetComponent<GameEntityView>()?.GetEntity();

        GameEntity newEntity = null;
        if (e.type != ElementUpdateType.removed)
            newEntity = m_level.entityList.GetEntity(e.ID);

        if (oldEntity == null && newEntity == null)
            return;

        Vector3 pos = Vector3.zero;
        if (newEntity != null)
            pos = newEntity.GetViewPos();
        else if (oldEntity != null)
            pos = oldEntity.GetViewPos();

        if (e.type == ElementUpdateType.removed)
            RemoveRender(e.ID);
        else UpdateRender(m_level.entityList.GetEntityIndex(e.ID));
    }


    void RemoveRender(int ID)
    {
        GameObject oldInstance = null;
        m_entities.TryGetValue(ID, out oldInstance);

        if (oldInstance != null)
            Destroy(oldInstance);

        m_entities.Remove(ID);
    }

    void UpdateRender(int index)
    {
        if (index < 0)
            return;

        var entity = m_level.entityList.GetEntityFromIndex(index);

        GameObject instance = null;

        if (entity != null)
        {
            var infos = EntityDataEx.Get(entity.GetEntityType());
            if (infos == null || infos.prefab == null)
                return;

            instance = Instantiate(infos.prefab);

            var size = Global.instance.allBlocks.blockSize;
            instance.transform.parent = transform;
            instance.transform.localPosition = entity.GetViewPos();
            instance.transform.localRotation = entity.GetViewRotation();

            SetEntity(instance, entity);
        }

        GameObject oldInstance = null;
        m_entities.TryGetValue(entity.GetID(), out oldInstance);
        if (oldInstance != null)
            Destroy(oldInstance);

        if (instance == null)
            m_entities.Remove(entity.GetID());
        else m_entities[entity.GetID()] = instance;
    }

    void SetEntity(GameObject instance, GameEntity entity)
    {
        var view = instance.AddComponent<GameEntityView>();
        view.SetEntity(entity);
    }
}
