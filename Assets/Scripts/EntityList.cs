using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public static class EntityList
{
    class EntityData
    {
        public GameObject entity;
        public float radius;

        public EntityData(GameObject _entity, float _radius)
        {
            entity = _entity;
            radius = _radius;
        }
    }

    static List<EntityData> m_entities = new List<EntityData>();

    public static void Add(GameObject entity, float radius)
    {
        var e = m_entities.Find(x => { return x.entity == entity; });
        if (e == null)
            m_entities.Add(new EntityData(entity, radius));
        else e.radius = radius;
    }

    public static void Remove(GameObject entity)
    {
        m_entities.RemoveAll(x => { return x.entity == entity; });
    }

    public static void Clear()
    {
        m_entities.Clear();
    }

    public static bool GetValidPos(Vector3 center, float radius, out Vector3 pos)
    {
        StaticRandomGenerator<MT19937> rand = new StaticRandomGenerator<MT19937>();
        UniformVector2CircleDistribution d = new UniformVector2CircleDistribution(radius);

        float avoidDistance = Global.instance.entityAvoidDistance;

        for (int i = 0; i < 20; i++)
        {
            Vector2 p = d.Next(rand);
            p.x += center.x;
            p.y += center.z;

            bool isValid = true;

            foreach (var e in m_entities)
            {
                float dist = e.radius + avoidDistance;
                dist *= dist;

                Vector2 ePos = new Vector2(e.entity.transform.position.x, e.entity.transform.position.z);
                float sqrDist = (ePos - p).sqrMagnitude;
                if (sqrDist < dist)
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                continue;

            pos = new Vector3(p.x, center.y, p.y);
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    struct NearEvadeData
    {
        public EntityData data;
        public float minAngle;
        public float maxAngle;
        public bool used;

        public NearEvadeData(EntityData _data, float _minAngle, float _maxAngle)
        {
            data = _data;
            minAngle = _minAngle;
            maxAngle = _maxAngle;
            used = false;
        }
    }

    static List<NearEvadeData> m_evadeData = new List<NearEvadeData>(); 

    public static Vector2 EvadeDirection(GameObject obj, Vector2 wantedForward)
    {
        m_evadeData.Clear();

        var elem = m_entities.Find(x => { return x.entity == obj; });
        if(elem == null || Global.instance == null)
            return new Vector2(obj.transform.forward.x, obj.transform.forward.z);

        float avoidDistance = Global.instance.entityAvoidDistance;

        Vector2 pos = new Vector2(obj.transform.position.x, obj.transform.position.z);

        foreach (var e in m_entities)
        {
            if (e.entity == obj)
                continue;

            Vector2 entityPos = new Vector2(e.entity.transform.position.x, e.entity.transform.position.z);

            float dist = (pos - entityPos).sqrMagnitude;
            float minDist = elem.radius + e.radius + avoidDistance;
            minDist *= minDist;

            if (dist < minDist)
            {
                Vector2 dirToTarget = entityPos - pos;
                float distToTarget = dirToTarget.magnitude;
                dirToTarget /= distToTarget;
                Vector2 orthoDir = new Vector2(dirToTarget.y, -dirToTarget.x);

                Vector2 dirLeft = dirToTarget * distToTarget + orthoDir * (e.radius + elem.radius);
                Vector2 dirRight = dirToTarget * distToTarget - orthoDir * (e.radius + elem.radius);

                float angleLeft = Utility.Angle(wantedForward, dirLeft);
                float angleRight = Utility.Angle(wantedForward, dirRight);

                var data = new NearEvadeData(e, Mathf.Min(angleLeft, angleRight), Mathf.Max(angleLeft, angleRight));
                if (data.minAngle < -Mathf.PI / 2 && data.maxAngle > Mathf.PI / 2)
                    continue;

                m_evadeData.Add(data);
            }
        }

        if(m_evadeData.Count == 0)
            return new Vector2(obj.transform.forward.x, obj.transform.forward.z);

        float leftAngle = 0;
        float rightAngle = 0;

        for(int i = 0; i < m_evadeData.Count; i++)
        {
            var e = m_evadeData[i];

            if(e.minAngle < leftAngle && e.maxAngle > leftAngle)
            {
                leftAngle = e.minAngle;
                i = 0;
            }
            if(e.minAngle < rightAngle && e.maxAngle > rightAngle)
            {
                rightAngle = e.maxAngle;
                i = 0;
            }
        }

        float angle = Mathf.Abs(rightAngle) < Mathf.Abs(leftAngle) ? rightAngle : leftAngle;
        angle += Utility.Angle(wantedForward);

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}
