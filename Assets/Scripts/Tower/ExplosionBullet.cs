using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ExplosionBullet : BulletBase
{
    [SerializeField] float m_speed = 1;
    [SerializeField] float m_damages = 1;
    [SerializeField] float m_radius = 1;
    [SerializeField] GameObject m_instantiateOnExplosion = null;

    Vector3 m_maxPos;

    private void Start()
    {
        if (target != null)
            m_maxPos = target.transform.position;
        else m_maxPos = transform.position + transform.forward * m_speed * 2;
    }

    private void Update()
    {
        if (Gamestate.instance.paused)
            return;

        float dist = m_speed * speedMultiplier * Time.deltaTime;

        RaycastHit hit;

        bool haveHit = Physics.Raycast(transform.position, transform.forward, out hit, dist, 1 << ennemyLayer);

        if (haveHit)
        {
            Explode();
            return;
        }

        Vector3 newPos = transform.position + transform.forward * dist;

        float oldDist = (transform.position - m_maxPos).sqrMagnitude;
        float newDist = (newPos - m_maxPos).sqrMagnitude;

        if(newDist > oldDist)
        {
            Explode();
            return;
        }

        transform.position = newPos;
    }

    void Explode()
    {
        var cols = Physics.OverlapSphere(transform.position, m_radius, ennemyLayer);

        for (int i = 0; i < cols.Length; i++)
        {
            Vector3 hitDirection = (cols[i].transform.position - transform.position).normalized;
            Event<HitEvent>.Broadcast(new HitEvent(m_damages * damageMultiplier, transform.position, hitDirection));
        }

        if (m_instantiateOnExplosion != null)
        {
            var obj = Instantiate(m_instantiateOnExplosion);

            obj.transform.position = transform.position;
            obj.transform.localScale = new Vector3(m_radius, m_radius, m_radius) * 2;
        }

        Destroy(gameObject);
    }
}
