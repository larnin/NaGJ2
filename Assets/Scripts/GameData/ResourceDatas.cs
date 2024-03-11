using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ResourceType
{
    Energy,
    Iron,
    Copper,
    Crystal,
}

[Serializable]
public class ResourceStackData
{
    public int count;
    public ResourceType resourceType;
}

[Serializable]
public class ResourceCostData
{
    public List<ResourceStackData> costs;
}

[Serializable]
public class OneResource
{
    public ResourceType type;
    public GameObject prefab;
    public Sprite sprite;
}

[Serializable]
public class AllResources
{
    public float beltResourceSpacing = 0.5f;
    public float beltSpeed = 1;

    [SerializeField] List<OneResource> m_resources;

    public OneResource Get(ResourceType type)
    {
        foreach(var r in m_resources)
        {
            if (r.type == type)
                return r;
        }

        return null;
    }
}