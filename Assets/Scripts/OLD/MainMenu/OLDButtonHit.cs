using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class OLDButtonHit : MonoBehaviour
{
    [SerializeField] float m_hitPower = 1;

    public UnityEvent onClick;

    SubscriberList m_subscriberList = new SubscriberList();

    Rigidbody m_rigidbody;

    Vector3 m_originalPos;
    Quaternion m_originalRot;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        m_subscriberList.Add(new Event<HitEvent>.LocalSubscriber(OnHit, gameObject));
        m_subscriberList.Subscribe();

        m_originalPos = transform.position;
        m_originalRot = transform.rotation;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnHit(HitEvent e)
    {
        if(m_rigidbody != null)
        {
            m_rigidbody.AddForceAtPosition(e.hitDirection * m_hitPower, e.position);
        }

        if(onClick != null)
            onClick.Invoke();
    }

    public void Reset()
    {
        transform.position = m_originalPos;
        transform.rotation = m_originalRot;

        if(m_rigidbody != null)
        {
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
