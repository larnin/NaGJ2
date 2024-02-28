using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AddResourceEvent
{
    public ResourceType resource;
    public int count;

    public int buildingID;
    public int containerIndex;

    public bool result;

    public AddResourceEvent(ResourceType _type, int _count, int _buildingID, int _containerIndex)
    {
        resource = _type;
        count = _count;
        buildingID = _buildingID;
        containerIndex = _containerIndex;

        result = false;
    }
}

public class RemoveResourceEvent : AddResourceEvent
{
    public RemoveResourceEvent(ResourceType _type, int _count, int _buildingID, int _containerIndex) : base(_type, _count, _buildingID, _containerIndex) {}
}

public class GetContainerCapacityEvent
{
    public int buildingID;
    public int containerIndex;

    public int capacity;
    public int freeSpace;

    public GetContainerCapacityEvent(int _buildingID, int _containerIndex)
    {
        buildingID = _buildingID;
        containerIndex = _containerIndex;
    }
}

public class GetContainerItems
{
    public int buildingID;
    public int containerIndex;

    public List<ResourceType> resources = new List<ResourceType>();

    public GetContainerItems(int _buildingID, int _containerIndex)
    {
        buildingID = _buildingID;
        containerIndex = _containerIndex;
    }
}