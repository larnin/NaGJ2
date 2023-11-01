using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
public class AllResources
{

}