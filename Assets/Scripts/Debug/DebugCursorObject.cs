using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DebugCursorObject : MonoBehaviour
{
    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane p = new Plane(new Vector3(0, 1, 0), transform.position);
        float enter = 0;
        if (p.Raycast(ray, out enter))
        {
            Vector3 pos = ray.GetPoint(enter);
            transform.position = pos;
        }
    }
}
