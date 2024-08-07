using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class EntityDataEx
{
    public static OneEntityData Get(EntityType type)
    {
        foreach(var e in Global.instance.allEntities.entities)
        {
            if (e.type == type)
                return e;
        }

        return null;
    }
}
