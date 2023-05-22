using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class Spawner : MonoBehaviour
{
    [SerializeField] float m_appearDuration = 2;
    [SerializeField] float m_disappearDuration = 1;
    [SerializeField] float m_radius = 1;

    const string materialRadius = "_HoleSize";

    Renderer m_renderer;
    Material m_material;

    float m_timer = 0;
    bool m_disappear = false;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<SpawnerStopEvent>.LocalSubscriber(Stop, gameObject));
        m_subscriberList.Add(new Event<SpawnerGetStatusEvent>.LocalSubscriber(GetStatus, gameObject));
        m_subscriberList.Add(new Event<SpawnEntityEvent>.LocalSubscriber(Spawn, gameObject));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
        if (m_renderer != null)
        {
            m_material = m_renderer.material;
            m_renderer.material = m_material;
        }

        m_material.SetFloat(materialRadius, -1);
    }

    private void Update()
    {
        if (Gamestate.instance.paused)
            return;

        m_timer += Time.deltaTime;
        UpdateHoleMat();
    }

    void UpdateHoleMat()
    {
        float percent = m_disappear ? 1 - (m_timer / m_disappearDuration) : m_timer / m_appearDuration;
        if (percent > 1)
            percent = 1;

        percent = DOVirtual.EasedValue(-1, 0.2f, percent, Ease.OutSine);

        m_material.SetFloat(materialRadius, percent);
    }

    void Stop(SpawnerStopEvent e)
    {
        m_disappear = true;
        m_timer = 0;
    }

    void GetStatus(SpawnerGetStatusEvent e)
    {
        e.canSpawn = !m_disappear && m_timer >= m_appearDuration;
        e.stopped = m_disappear && m_timer >= m_disappearDuration;
    }

    void Spawn(SpawnEntityEvent e)
    {

    }
}
