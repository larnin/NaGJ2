using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GismoElement : MonoBehaviour
{
    [SerializeField] GismoType m_type;
    [SerializeField] GismoAxis m_axis;

    private void OnMouseDrag()
    {
        var comp = GetComponentInParent<Gismos>();
        if (comp == null)
            return;

        comp.OnDrag(m_type, m_axis);
    }

    private void OnMouseOver()
    {
        var comp = GetComponentInParent<Gismos>();
        if (comp == null)
            return;

        comp.OnHover(m_type, m_axis);
    }
}
