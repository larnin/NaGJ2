using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrackingBullet : BulletBase
{
    [SerializeField] float m_maxDuration = 1;
    [SerializeField] float m_speed = 1;
    [SerializeField] float m_rotationSpeed = 1;
    [SerializeField] float m_damages = 1;
    [SerializeField] GameObject m_instantiateOnHit = null;

    float m_duration = 0;

    private void Update()
    {
        if (Gamestate.instance.paused)
            return;

        float dist = m_speed * speedMultiplier * Time.deltaTime;

        RaycastHit hit;

        bool haveHit = Physics.Raycast(transform.position, transform.forward, out hit, dist, 1 << ennemyLayer);

        if (haveHit)
        {
            OnHit(hit);
            return;
        }

        m_duration += Time.deltaTime;
        if (m_duration > m_maxDuration)
        {
            OnHit(hit);
            return;
        }

        Rotate();

        Vector3 newPos = transform.position + transform.forward * dist;
        transform.position = newPos;
    }

    void Rotate()
    {
        if (target == null)
            return;

        Vector3 wantedForward = (target.transform.position - transform.position);
        Quaternion wantedRotation = Quaternion.LookRotation(wantedForward);

        float angle = Quaternion.Angle(transform.rotation, wantedRotation);
        if (angle < 0.1f)
            return;
        float deltaAngle = Time.deltaTime * m_rotationSpeed;
        if(deltaAngle > angle)
        {
            transform.rotation = wantedRotation;
            return;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, deltaAngle / angle);
    }

    void OnHit(RaycastHit hit)
    {
        if(hit.collider != null)
            Event<HitEvent>.Broadcast(new HitEvent(m_damages * damageMultiplier, hit.point, transform.forward), hit.collider.gameObject);

        if (m_instantiateOnHit != null)
        {
            var obj = Instantiate(m_instantiateOnHit);

            if (hit.collider != null)
            {
                obj.transform.position = hit.point;
                obj.transform.forward = hit.normal;
            }
            else
            {
                obj.transform.position = transform.position;
                obj.transform.forward = transform.forward;
            }
        }

        var particles = GetComponentInChildren<ParticleSystem>();
        particles.transform.parent = null;
        particles.Stop();
        Destroy(particles.gameObject, 2);

        Destroy(gameObject);
    }
}
