using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorBase : MonoBehaviour
{
    [SerializeField] LayerMask m_groundLayer;

    Camera m_camera;

    private void Start()
    {
        m_camera = Camera.main;
    }

    protected Vector3Int GetCursorPosition()
    {
        var ray = m_camera.ScreenPointToRay(Input.mousePosition);

        Vector3 blockSize = Global.instance.allBlocks.blockSize;

        RaycastHit hit;
        bool haveHit = Physics.Raycast(ray, out hit, m_groundLayer);
        if (haveHit)
        {
            Vector3 hitOffset = new Vector3(0, 0.5f * blockSize.y, 0);
            Vector3 pos = ray.GetPoint(hit.distance) + hitOffset;

            pos -= hit.normal * 0.01f;

            //scale pos
            pos.x /= blockSize.x;
            pos.y /= blockSize.y;
            pos.z /= blockSize.z;

            Vector3Int posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

            Vector3Int offset = Vector3Int.zero;
            if (Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.y) && Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.z))
                offset.x = Mathf.RoundToInt(Mathf.Sign(hit.normal.x));
            else if (Mathf.Abs(hit.normal.y) > Mathf.Abs(hit.normal.z))
                offset.y = Mathf.RoundToInt(Mathf.Sign(hit.normal.y));
            else offset.z = Mathf.RoundToInt(Mathf.Sign(hit.normal.z));

            return posInt + offset;
        }

        Plane p = new Plane(Vector3.up, Vector3.zero);
        float enter;
        if (p.Raycast(ray, out enter))
        {
            Vector3 pos = ray.GetPoint(enter);
            
            //scale pos
            pos.x /= blockSize.x;
            pos.y /= blockSize.y;
            pos.z /= blockSize.z;

            Vector3Int posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

            return posInt + Vector3Int.up;
        }

        return Vector3Int.zero;
    }
}
