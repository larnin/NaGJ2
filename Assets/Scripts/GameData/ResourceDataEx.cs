using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ResourceDataEx
{
    public static bool IsLiquid(ResourceType type)
    {
        switch(type)
        {
            case ResourceType.Water:
                return true;
            default:
                return false;
        }
    }
}
