using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DisplaySelectedBuildingsEvent
{
    public List<Vector3Int> pos;

    public DisplaySelectedBuildingsEvent(List<Vector3Int> _pos)
    {
        pos = _pos;
    }
}

public class ClearDisplaySelectionEvent { }

public class DisplaySelectionRectangleEvent
{
    public Vector3 pos1;
    public Vector3 pos2;
    public Color color;

    public DisplaySelectionRectangleEvent(Vector3 _pos1, Vector3 _pos2, Color _color)
    {
        pos1 = _pos1;
        pos2 = _pos2;
        color = _color;
    }
}

public class HideSelectionRectangleEvent { }