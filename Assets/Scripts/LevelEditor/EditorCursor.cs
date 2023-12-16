using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorCursor : MonoBehaviour
{
    [SerializeField] LayerMask m_groundLayer;

    Vector3Int m_pos;
    Vector3Int m_blockPos;

    Camera m_camera;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<EditorCurstorGetPosEvent>.Subscriber(GetStatus));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        m_camera = Camera.main;
    }

    private void Update()
    {
        if (m_camera == null)
            return;

        var ray = m_camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        bool haveHit = Physics.Raycast(ray, out hit, m_groundLayer); 
        if(haveHit)
            ProcessHit(ray, hit);
        else
        {
            Plane p = new Plane(Vector3.up, Vector3.zero);
            float enter;
            if(p.Raycast(ray, out enter))
            {
                Vector3 pos = ray.GetPoint(enter);
                Vector3Int posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

                m_pos = posInt + Vector3Int.up;
                m_blockPos = posInt;
            }
        }

        if (Input.GetMouseButtonDown(0) && !Utility.MouseOverUI())
            Event<EditorCursorClickEvent>.Broadcast(new EditorCursorClickEvent(EditorCursorClickType.leftClick));
        if (Input.GetMouseButtonDown(1) && !Utility.MouseOverUI())
            Event<EditorCursorClickEvent>.Broadcast(new EditorCursorClickEvent(EditorCursorClickType.rightClick));
    }

    void ProcessHit(Ray ray, RaycastHit hit)
    {
        Vector3 blockSize = Global.instance.allBlocks.blockSize;
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

        m_pos = posInt + offset;
        m_blockPos = posInt;
    }

    void GetStatus(EditorCurstorGetPosEvent e)
    {
        e.pos = m_pos;
        e.blockPos = m_blockPos;
    }
}
