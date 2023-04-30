using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SetBuildingInfoEvent
{
    public int x;
    public int y;

    public SetBuildingInfoEvent(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}

