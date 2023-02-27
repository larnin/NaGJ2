using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField] float m_maxLife;

    SubscriberList m_subscriberList = new SubscriberList();

    float m_life = 0;
    float m_lifeMultiplier = 1;

    private void Awake()
    {
        m_subscriberList.Add(new Event<HitEvent>.LocalSubscriber(OnHit, gameObject));
        m_subscriberList.Add(new Event<GetLifeEvent>.LocalSubscriber(GetLife, gameObject));
        m_subscriberList.Add(new Event<SetLifeMultiplierEvent>.LocalSubscriber(SetMultiplier, gameObject));
        m_subscriberList.Subscribe();

        m_life = m_maxLife;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnHit(HitEvent e)
    {
        m_life -= e.damage / m_lifeMultiplier;
        if(m_life <= 0)
        {
            m_life = 0;
            Event<DeathEvent>.Broadcast(new DeathEvent(), gameObject);
        }
    }

    void GetLife(GetLifeEvent e)
    {
        e.life = m_life * m_lifeMultiplier;
        e.maxLife = m_maxLife * m_lifeMultiplier;
    }

    void SetMultiplier(SetLifeMultiplierEvent e)
    {
        m_lifeMultiplier = e.multiplier;
    }
}
