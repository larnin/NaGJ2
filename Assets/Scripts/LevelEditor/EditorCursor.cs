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

    private void Update()
    {
        if (m_camera == null)
            return;

        var ray = m_camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        bool haveHit = Physics.Raycast(ray, out hit);
        if(haveHit)
            ProcessHit(ray, hit);
    }

    void ProcessHit(Ray ray, RaycastHit hit)
    {
        Vector3 pos = ray.GetPoint(hit.distance);
        pos -= hit.normal * 0.2f;

        Vector3Int posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

        Vector3Int offset = Vector3Int.zero;
        if (Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.y) && Mathf.Abs(hit.normal.x) > Mathf.Abs(hit.normal.z))
            offset.x = Mathf.RoundToInt(Mathf.Sign(hit.normal.x));
        else if (Mathf.Abs(hit.normal.y) > Mathf.Abs(hit.normal.z))
            offset.y = Mathf.RoundToInt(Mathf.Sign(hit.normal.y));
        else offset.z = Mathf.RoundToInt(Mathf.Sign(hit.normal.z));

        m_pos = posInt + offset;
        m_blockPos = posInt;

        if (Input.GetMouseButtonDown(0))
            Event<EditorCursorClickEvent>.Broadcast(new EditorCursorClickEvent(EditorCursorClickType.leftClick));
        if (Input.GetMouseButtonDown(1))
            Event<EditorCursorClickEvent>.Broadcast(new EditorCursorClickEvent(EditorCursorClickType.rightClick));
    }

    void GetStatus(EditorCurstorGetPosEvent e)
    {
        e.pos = m_pos;
        e.blockPos = m_blockPos;
    }
}
