using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDFastBullet : OLDBulletBase
{
    [SerializeField] float m_maxDistance = 1;
    [SerializeField] float m_speed = 1;
    [SerializeField] float m_damages = 1;
    [SerializeField] GameObject m_instantiateOnHit = null;

    float m_distance = 0;

    private void Update()
    {
        if (Gamestate.instance.paused)
            return;

        if (m_distance >= m_maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        float dist = m_speed * speedMultiplier * Time.deltaTime;

        RaycastHit hit;

        bool haveHit = Physics.Raycast(transform.position, transform.forward, out hit, dist, 1 << ennemyLayer);

        if(haveHit)
        {
            DoHit(hit);
            Destroy(gameObject);
            return;
        }

        m_distance += dist;

        Vector3 newPos = transform.position + transform.forward * dist;
        transform.position = newPos;
    }

    void DoHit(RaycastHit hit)
    {
        if(m_instantiateOnHit != null)
        {
            var obj = Instantiate(m_instantiateOnHit);

            obj.transform.position = hit.point;
            obj.transform.forward = hit.normal;
        }

        Event<HitEvent>.Broadcast(new HitEvent(m_damages * damageMultiplier, hit.point, transform.forward), hit.collider.gameObject);
    }
}