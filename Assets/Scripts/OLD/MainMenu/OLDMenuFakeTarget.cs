using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDMenuFakeTarget : MonoBehaviour
{
    [SerializeField] LayerMask m_targetLayer;

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool haveHit = Physics.Raycast(ray, out hit, 10000, m_targetLayer.value);

        if (haveHit)
            transform.position = hit.point;
        else transform.position = new Vector3(-10000, -10000, -10000);
    }
}
