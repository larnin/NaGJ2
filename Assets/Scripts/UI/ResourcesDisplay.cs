using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] GameObject m_resourcePrefab;
    [SerializeField] Vector2 m_offset;
    [SerializeField] Transform m_resourcePos;

    class OneResourceInstance
    {
        public GameObject instance;
        public TMP_Text valueTxt;
        public SpriteRenderer renderer;
    }

    List<OneResourceInstance> m_resources = new List<OneResourceInstance>();

    private void Update()
    {
        UpdateResourceList();
        UpdatePositions();
    }

    void UpdateResourceList()
    {
        GameGetCurrentLevelEvent level = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(level);

        if (level.level == null)
        {
            foreach (var r in m_resources)
                Destroy(r.instance);
            m_resources.Clear();
        }
        else
        {
            int resourceNB = level.level.resources.GetResourceNb();
            while(resourceNB < m_resources.Count)
            {
                Destroy(m_resources[0].instance);
                m_resources.RemoveAt(0);
            }
            while (resourceNB > m_resources.Count)
                m_resources.Add(CreateResource());
          
            for(int i = 0; i < resourceNB; i++)
            {
                var type = level.level.resources.GetResourceTypeFromIndex(i);
                var resource = Global.instance.allResources.Get(type);
                if (resource != null)
                    m_resources[i].renderer.sprite = resource.sprite;
                m_resources[i].valueTxt.text = level.level.resources.GetResourceNbIntFromIndex(i).ToString();
            }
        }
    }

    void UpdatePositions()
    {
        for(int i = 0; i < m_resources.Count; i++)
            m_resources[i].instance.transform.localPosition = new Vector3(m_offset.x, m_offset.y, 0) * i;
    }

    OneResourceInstance CreateResource()
    {
        OneResourceInstance resource = new OneResourceInstance();
        resource.instance = Instantiate(m_resourcePrefab);
        resource.instance.transform.SetParent(m_resourcePos, true);
        resource.instance.transform.localPosition = Vector3.zero;

        resource.renderer = resource.instance.GetComponentInChildren<SpriteRenderer>();
        resource.valueTxt = resource.instance.GetComponentInChildren<TMP_Text>();

        return resource;
    }
}
