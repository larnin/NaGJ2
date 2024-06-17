using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceView : MonoBehaviour
{
    [SerializeField] float m_resourceOffset;

    GameLevel m_level;

    Dictionary<int, GameObject> m_resources = new Dictionary<int, GameObject>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GameSetCurrentLevelEvent>.Subscriber(SetLevel));
        m_subscriberList.Add(new Event<GameResetEvent>.Subscriber(Reset));
        m_subscriberList.Add(new Event<GameLoadEvent>.Subscriber(AfterLoad));
        m_subscriberList.Add(new Event<ResourceUpdateEvent>.Subscriber(OnResourceUpdate));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void SetLevel(GameSetCurrentLevelEvent e)
    {
        m_level = e.level;
    }

    void Reset(GameResetEvent e)
    {
        ResetData();
    }

    void ResetData()
    {
        foreach (var b in m_resources)
        {
            if (b.Value != null)
                Destroy(b.Value);
        }

        m_resources.Clear();
    }

    void AfterLoad(GameLoadEvent e)
    {
        ResetData();

        var ids = m_level.beltSystem.GetAllResourcesID();

        foreach(var id in ids)
        {
            var resource = m_level.beltSystem.GetResource(id);
            if (resource == null)
                continue;

            var oneResource = Global.instance.allResources.Get(resource.resource);
            if (oneResource.prefab == null)
                continue;

            var instance = Instantiate(oneResource.prefab);
            instance.transform.parent = transform;

            m_resources.Add(id, instance);
        }

        UpdateResources();
    }

    void OnResourceUpdate(ResourceUpdateEvent e)
    {
        var resource = m_level.beltSystem.GetResource(e.ID);
        GameObject instance = null;
        m_resources.TryGetValue(e.ID, out instance);

        if(resource != null && instance == null)
        {
            var oneResource = Global.instance.allResources.Get(resource.resource);
            if (oneResource.prefab == null)
                return;

            instance = Instantiate(oneResource.prefab);
            instance.transform.parent = transform;

            m_resources.Add(e.ID, instance);
        }
        else if(resource == null && instance != null)
        {
            Destroy(instance);
            m_resources.Remove(e.ID);
        }
    }

    private void Update()
    {
        UpdateResources();
    }
    
    void UpdateResources()
    {
        var size = Global.instance.allBlocks.blockSize;

        foreach (var r in m_resources)
        {
            var resource = m_level.beltSystem.GetResource(r.Key);
            if (resource == null)
                continue;

            var belt = m_level.beltSystem.GetNode(resource.beltIndex);

            Vector3 dirToCenter = new Vector3(resource.pos.x - belt.pos.x, resource.pos.y - belt.pos.y, resource.pos.z - belt.pos.z);
            Rotation rotToCenter = RotationEx.FromVector(dirToCenter);
            float distToCenter = dirToCenter.MagnitudeXZ();

            float normDist = 0;
            if (rotToCenter == belt.rotation || distToCenter < 0.001f)
            {
                var target = new Vector3(belt.pos.x, belt.pos.y, belt.pos.z) + 0.5f * RotationEx.ToVector3(belt.rotation);
                normDist = 1.0f - (resource.pos - target).MagnitudeXZ();
            }
            else normDist = 0.5f - distToCenter;


            Vector3 offset = resource.pos;

            if (belt.direction == BeltDirection.Horizontal)
            {
                if (resource.startDirection == RotationEx.Add(belt.rotation, Rotation.rot_90) || resource.startDirection == RotationEx.Add(belt.rotation, Rotation.rot_270))
                {
                    Vector3 center = new Vector3(belt.pos.x, belt.pos.y, belt.pos.z);
                    Vector3 start = center + RotationEx.ToVector3(resource.startDirection);
                    Vector3 end = center + RotationEx.ToVector3(belt.rotation);
                    Vector3 circleCenter = (start + end) / 2;

                    start = (center + start) / 2 - circleCenter;
                    end = (center + end) / 2 - circleCenter;

                    Vector3 current = Vector3.Slerp(start, end, normDist);
                    current += circleCenter;

                    offset = current;
                }
            }
            else if (belt.direction == BeltDirection.Up)
            {
                const float delta = 0.15f;
                normDist /= 1 - delta;
                if (normDist > 1)
                    normDist = 1;

                offset.y += -(1 - normDist);
            }
            else if (belt.direction == BeltDirection.Down)
            {
                const float delta = 0.15f;
                normDist /= 1 - delta;
                normDist -= delta;
                if (normDist < 0)
                    normDist = 0;

                offset.y += -normDist;
            }

            var pos = new Vector3(offset.x * size.x, offset.y * size.y + m_resourceOffset, offset.z * size.z);

            r.Value.transform.localPosition = pos;
        }
    }
}
