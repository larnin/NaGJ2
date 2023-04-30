using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Enemy : MonoBehaviour
{
    [SerializeField] GameObject m_projectilePrefab;
    [SerializeField] float m_fireRate = 1;
    [SerializeField] float m_fireRange = 1;
    [SerializeField] float m_moveSpeed = 1;
    [SerializeField] bool m_explode = false;
    [SerializeField] float m_explosionRadius = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    bool m_haveTarget = false;
    Vector2Int m_targetBuiding = Vector2Int.zero;
    Vector3 m_target = Vector3.zero;
    float m_fireDelay = 0;

    int m_playerLayer = 0;

    float m_multiplier = 1;

    public void SetMultiplier(float multiplier)
    {
        m_multiplier = multiplier;

        Event<SetLifeMultiplierEvent>.Broadcast(new SetLifeMultiplierEvent(multiplier));
    }

    private void Awake()
    {
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnDeath(DeathEvent e)
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        m_playerLayer = LayerMask.NameToLayer("Player");

        UpdateTarget();
    }

    private void Update()
    {
        UpdateTarget();

        if (!m_haveTarget)
            return;

        Vector3 targetMove = m_target;
        targetMove.y = transform.position.y;

        Vector3 dir = targetMove - transform.position;
        float dist = dir.magnitude;

        if(dist < m_fireRange)
        {
            m_fireDelay -= Time.deltaTime;
            if (m_fireDelay <= 0)
                Fire();
        }
        else
        {
            dir /= dist;

            float d = m_moveSpeed * Time.deltaTime;
            var newPos = transform.position + d * dir;
            transform.position = newPos;
        }
    }

    void Fire()
    {
        m_fireDelay = 1 / m_fireRate;
        if (m_projectilePrefab != null)
        {
            var obj = Instantiate(m_projectilePrefab);
            obj.transform.position = transform.position;

            Vector3 forward = m_target - transform.position;
            Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
            obj.transform.rotation = rot;

            var bullet = obj.GetComponent<BulletBase>();
            if (bullet != null)
            {
                bullet.ennemyLayer = m_playerLayer;
                bullet.target = null;
                bullet.damageMultiplier = m_multiplier;
            }
        }

        if(m_explode)
        {
            var col = Physics.OverlapSphere(transform.position, m_explosionRadius, 1 << m_playerLayer);
            foreach(var c in col)
            {
                HitEvent e = new HitEvent(m_multiplier);
                Event<HitEvent>.Broadcast(e, c.gameObject);
            }

            Destroy(gameObject);
        }
    }

    void UpdateTarget()
    {
        GetNearestBuildingEvent e = new GetNearestBuildingEvent(transform.position);
        Event<GetNearestBuildingEvent>.Broadcast(e);

        m_haveTarget = e.buildingFound;
        m_target = Vector3.zero;
        if (WorldHolder.Instance() != null)
            m_target = WorldHolder.Instance().GetElemPos(e.buildingPos.x, e.buildingPos.y);
        m_targetBuiding = e.buildingPos;
        m_target.x += 0.5f;
        m_target.z += 0.5f;
    }
}
