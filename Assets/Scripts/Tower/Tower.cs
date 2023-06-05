using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Tower : MonoBehaviour
{
    const float resetTimeWithoutTarget = 5;

    [SerializeField] List<GameObject> m_firePoints = new List<GameObject>();
    [SerializeField] GameObject m_projectilePrefab;
    [SerializeField] GameObject m_fireParticlesPrefab;
    [SerializeField] float m_recoil = 0.01f;
    [SerializeField] float m_recoilTime = 0.1f;
    [SerializeField] float m_rotationSpeed = 5;
    [SerializeField] float m_fireDistance = 5;
    [SerializeField] float m_fireDelay = 1;
    [SerializeField] float m_minFireAngle = 2;

    int m_ennemyLayer;
    Transform m_tower = null;
    GameObject m_currentTarget = null;
    float m_durationWithoutTarget = 0;
    float m_updateTargetTimer = 0;
    Vector3 m_initialPos;
    float m_recoilTimer = 0;
    float m_fireTimer = 0;
    int m_currentFirePoint = 0;
    bool m_allowedToFire = true;

    GameObject m_forcedTarget = null;

    private void Start()
    {
        if(m_ennemyLayer == 0)
            m_ennemyLayer = LayerMask.NameToLayer("Ennemy");
        m_tower = transform.Find("Tower");
        if (m_tower != null)
            m_initialPos = m_tower.localPosition;
    }

    private void Update()
    {
        if (Gamestate.instance.paused)
            return;

        if (m_tower == null)
            return;

        m_updateTargetTimer -= Time.deltaTime;
        if (m_updateTargetTimer <= 0)
            UpdateTarget();

        ProcessTarget();

        UpdateRecoil();

        if (m_fireTimer > 0)
            m_fireTimer -= Time.deltaTime;
    }

    void UpdateTarget()
    {
        m_updateTargetTimer = 0;

        if (m_forcedTarget != null)
        {
            m_currentTarget = m_forcedTarget;
            return;
        }

        Vector3 p1 = transform.position + m_initialPos;
        Vector3 p2 = p1;
        p1.y += 10; p2.y -= 10;

        var cols = Physics.OverlapCapsule(p1, p2, m_fireDistance, 1 << m_ennemyLayer);

        if(cols.Length == 0)
        {
            m_currentTarget = null;
            return;
        }

        float bestDistance = -1;
        GameObject bestTarget = null;
        Vector3 currentPos = transform.position + m_initialPos;
        Vector3 forward = m_tower.forward;

        CanBeTargetedEvent targeted = new CanBeTargetedEvent();

        for(int i = 0; i < cols.Length; i++)
        {
            targeted.targetable = true;
            Event<CanBeTargetedEvent>.Broadcast(targeted, cols[i].gameObject);
            if (!targeted.targetable)
                continue;

            Vector3 dir = currentPos - cols[i].transform.position;
            float dist = dir.magnitude;
            dir /= dist;

            var c = Vector3.Dot(forward, dir); //[-1, 1] - need it in the range [0.5, 1]
            c = (c + 1) / 4;
            dist *= c;

            if(bestDistance < 0 || bestDistance > dist)
            {
                bestDistance = dist;
                bestTarget = cols[i].gameObject;
            }
        }

        m_currentTarget = bestTarget;
    }

    void ProcessTarget()
    {
        if(m_currentTarget == null)
        {
            m_durationWithoutTarget += Time.deltaTime;
            if (m_durationWithoutTarget >= resetTimeWithoutTarget)
                ProcessResetRotation();
            return;
        }

        m_durationWithoutTarget = 0;

        Vector3 targetForward = m_currentTarget.transform.position - (transform.position + m_initialPos);
        targetForward.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(targetForward);
        RotateTower(targetRotation);

        if (m_fireTimer <= 0 && m_allowedToFire)
        {
            float angle = Quaternion.Angle(targetRotation, m_tower.rotation);
            if (angle < m_minFireAngle)
                Fire();
        }
    }

    void ProcessResetRotation()
    {
        Vector3 forward = m_tower.forward;
        forward.y = 0;
        forward.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(forward);
        RotateTower(targetRotation);
    }

    void RotateTower(Quaternion targetRotation)
    {
        float angle = Quaternion.Angle(m_tower.rotation, targetRotation);
        if (angle < 0.1f)
            return;
        float deltaAngle = Time.deltaTime * m_rotationSpeed;
        if (deltaAngle >= angle)
            m_tower.rotation = targetRotation;
        else m_tower.rotation = Quaternion.Slerp(m_tower.rotation, targetRotation, deltaAngle / angle);
    }

    void UpdateRecoil()
    {
        float currentRecoidDistance = 0;
        if(m_recoilTimer > 0)
        {
            currentRecoidDistance = m_recoilTimer / m_recoilTime * m_recoil;
            m_recoilTimer -= Time.deltaTime;
        }

        Vector3 offset = -m_tower.forward * currentRecoidDistance;

        m_tower.localPosition = m_initialPos + offset;
    }

    void Fire()
    {
        m_fireTimer = m_fireDelay;
        m_recoilTimer = m_recoilTime;

        if (m_firePoints.Count == 0)
            return;

        if (m_currentFirePoint >= m_firePoints.Count)
            m_currentFirePoint = 0;

        Transform currentTransform = m_firePoints[m_currentFirePoint]?.transform;
        m_currentFirePoint++;
        if (currentTransform == null)
            return;

        if(m_projectilePrefab != null)
        {
            GameObject obj = Instantiate(m_projectilePrefab);
            obj.transform.position = currentTransform.position;
            obj.transform.rotation = currentTransform.rotation;

            var bullet = obj.GetComponent<BulletBase>();
            if(bullet != null)
            {
                bullet.ennemyLayer = m_ennemyLayer;
                bullet.target = m_currentTarget.transform;
                //todo dmg & co
            }
        }

        if(m_fireParticlesPrefab != null)
        {
            GameObject obj = Instantiate(m_fireParticlesPrefab);
            obj.transform.position = currentTransform.position;
            obj.transform.rotation = currentTransform.rotation;
        }
    }

    public void AllowFire(bool allowed)
    {
        m_allowedToFire = allowed;
    }

    public void OverrideRange(float newRange)
    {
        m_fireDistance = newRange;
    }

    public void ForceTarget(GameObject target)
    {
        m_forcedTarget = target;
    }
}