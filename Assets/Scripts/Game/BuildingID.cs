using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingID : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    int m_instanceID = 0;

    private void Awake()
    {
        m_subscriberList.Add(new Event<SetBuildingInstanceIDEvent>.LocalSubscriber(SetID, gameObject));
        m_subscriberList.Add(new Event<GetBuildingInstanceIDEvent>.LocalSubscriber(GetID, gameObject));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetID(SetBuildingInstanceIDEvent e)
    {
        m_instanceID = e.ID;
    }

    void GetID(GetBuildingInstanceIDEvent e)
    {
        e.ID = m_instanceID;
    }
}
