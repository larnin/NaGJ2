using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GameEditorCursor : MonoBehaviour
{
    Vector3Int m_pos;
    Vector3Int m_blockPos;
    int m_buildingID = 0;

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

        GameGetCurrentLevelEvent getLevel = new GameGetCurrentLevelEvent();
        Event<GameGetCurrentLevelEvent>.Broadcast(getLevel);
        if (getLevel.level == null)
            return;

        var ray = m_camera.ScreenPointToRay(Input.mousePosition);

        Vector3 posGrid, normalGrid;
        bool hitGrid = getLevel.level.grid.RaycastWorld(ray.origin, ray.direction, out posGrid, out normalGrid);
        Vector3 posBuildings, normalBuildings;
        int buildingID;
        bool hitBuildings = getLevel.level.buildingList.Raycast(ray.origin, ray.direction, out posBuildings, out normalBuildings, out buildingID);

        m_buildingID = 0;

        if (!hitGrid && !hitBuildings)
        {
            Plane p = new Plane(Vector3.up, Vector3.zero);
            float enter;
            if (p.Raycast(ray, out enter))
            {
                Vector3 pos = ray.GetPoint(enter);
                Vector3Int posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

                m_pos = posInt + Vector3Int.up;
                m_blockPos = posInt;
            }
        }
        else if (hitGrid)
            SetPos(posGrid, normalGrid);
        else if (hitBuildings)
        {
            SetPos(posBuildings, normalBuildings);
            m_buildingID = buildingID;
        }
        else
        {
            float distGrid = (ray.origin - posGrid).sqrMagnitude;
            float distBuildings = (ray.origin - posBuildings).sqrMagnitude;

            if(distGrid < distBuildings)
                SetPos(posGrid, normalGrid);
            else
            {
                SetPos(posBuildings, normalBuildings);
                m_buildingID = buildingID;
            }
        }

        if (Input.GetMouseButtonDown(0) && !Utility.MouseOverUI())
            Event<EditorCursorClickEvent>.Broadcast(new EditorCursorClickEvent(EditorCursorClickType.leftClick));
        if (Input.GetMouseButtonDown(1) && !Utility.MouseOverUI())
            Event<EditorCursorClickEvent>.Broadcast(new EditorCursorClickEvent(EditorCursorClickType.rightClick));
        
        //DebugDrawPos(m_pos, Color.red);
        //DebugDrawPos(m_blockPos, Color.blue);
    }

    void DebugDrawPos(Vector3Int pos, Color c)
    {
        var size = Global.instance.allBlocks.blockSize;

        Vector3 realPos = new Vector3(pos.x * size.x, (pos.y - 0.5f) * size.y, pos.z * size.z);

        DebugDraw.CentredBox(realPos, size, c);
    }

    void GetStatus(EditorCurstorGetPosEvent e)
    {
        e.pos = m_pos;
        e.blockPos = m_blockPos;
        e.buildingID = m_buildingID;
    }

    void SetPos(Vector3 pos, Vector3 normal)
    {
        Vector3Int posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

        Vector3 absNormal = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
        Vector3Int dir = Vector3Int.zero;

        if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
            dir.x = normal.x < 0 ? -1 : 1;
        else if (absNormal.y > absNormal.z)
            dir.y = normal.y < 0 ? -1 : 1;
        else dir.z = normal.z < 0 ? -1 : 1;

        m_pos = posInt;
        m_blockPos = posInt + dir;
    }
}