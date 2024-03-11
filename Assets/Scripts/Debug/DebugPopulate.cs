using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DebugPopulate : MonoBehaviour
{
    [SerializeField] ResourceType m_type;
    [SerializeField] int m_container;
    [SerializeField] float m_delay;

    float m_timer;

    private void Update()
    {
        m_timer += Time.deltaTime;

        if(m_timer >= m_delay)
        {
            m_timer = 0;

            SpawnResource();
        }
    }

    void SpawnResource()
    {
        GetBuildingInstanceIDEvent id = new GetBuildingInstanceIDEvent();
        Event<GetBuildingInstanceIDEvent>.Broadcast(id, gameObject);

        Event<AddResourceEvent>.Broadcast(new AddResourceEvent(m_type, 1, id.ID, m_container));
    }
}
