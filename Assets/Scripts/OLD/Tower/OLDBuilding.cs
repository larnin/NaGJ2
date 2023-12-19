using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDBuilding : MonoBehaviour
{
    int m_x = 0;
    int m_y = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Add(new Event<SetBuildingInfoEvent>.LocalSubscriber(SetBuildingInfo, gameObject));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnDeath(DeathEvent e)
    {
        if (OLDGameSystem.Instance() == null)
            return;

        OLDGameSystem.Instance().DestroyBuilding(m_x, m_y);
    }

    void SetBuildingInfo(SetBuildingInfoEvent e)
    {
        m_x = e.x;
        m_y = e.y;
    }
}