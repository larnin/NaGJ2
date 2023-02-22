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

    int m_ennemyLayer;
    Transform m_tower = null;
    GameObject m_currentTarget = null;
    float m_durationWithoutTarget = 0;
    float m_updateTargetTimer = 0;
    Vector3 m_initialPos;
    float m_recoilTimer = 0;
    float m_fireTimer = 0;
    int m_currentFirePoint = 0;

    private void Start()
    {
        m_ennemyLayer = LayerMask.NameToLayer("Ennemy");
        m_tower = transform.Find("Tower");
        if (m_tower != null)
            m_initialPos = m_tower.localPosition;
    }

    private void Update()
    {
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

        for(int i = 0; i < cols.Length; i++)
        {
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

        if (m_fireTimer <= 0)
        {
            float angle = Quaternion.Angle(targetRotation, m_tower.rotation);
            if (angle < 2)
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

        if(m_projectilePrefab != null)
        {
            //todo
        }

        if(m_fireParticlesPrefab != null)
        {
            //todo
        }
    }
}