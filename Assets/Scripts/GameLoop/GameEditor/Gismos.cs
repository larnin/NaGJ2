using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GismoType
{
    none,
    pos,
    rot,
    scale
}

public enum GismoAxis
{
    axisNone,
    axisX,
    axisY,
    axisZ
}

public class Gismos : MonoBehaviour
{
    GismoType m_currentType = GismoType.none;
    GismoAxis m_hoveredAxis = GismoAxis.axisNone;
    GismoAxis m_hoveredAxisLastFrame = GismoAxis.axisNone;

    Vector3 m_pos;
    Quaternion m_rot;
    Vector3 m_scale;

    Vector3 m_gismosScale;

    bool m_axisAlligned = false;
    bool m_posRounded = false;

    Action m_callback;

    Vector3 m_oldMousePos;
    GismoAxis m_clickedAxis = GismoAxis.axisNone;

    Camera m_camera;

    private void Start()
    {
        m_pos = transform.position;
        m_rot = Quaternion.identity;
        m_scale = Vector3.one;

        m_camera = Camera.main;

        UpdateColors();
        UpdateRender();

        m_oldMousePos = Input.mousePosition;
    }

    public void SetGismoType(GismoType type)
    {
        m_currentType = type;

        UpdateColors();
        UpdateRender();
    }

    public GismoType GetGismoType()
    {
        return m_currentType;
    }

    public void SetPos(Vector3 pos)
    {
        m_pos = pos;

        UpdateRender();
    }

    public Vector3 GetPos()
    {
        return m_pos;
    }

    public Vector3Int GetGridPos()
    {
        var size = Global.instance.allBlocks.blockSize;

        return new Vector3Int(Mathf.RoundToInt(m_pos.x / size.x), Mathf.FloorToInt(m_pos.y / size.y), Mathf.RoundToInt(m_pos.z / size.z));
    }

    public void SetScale(Vector3 scale)
    {
        m_scale = scale;

        UpdateRender();
    }

    public Vector3 GetScale()
    {
        return m_scale;
    }

    public void SetRot(Quaternion rot)
    {
        m_rot = rot;

        UpdateRender();
    }

    public Quaternion GetRot()
    {
        return m_rot;
    }

    public void SetAxisAlligned(bool alligned)
    {
        m_axisAlligned = alligned;

        UpdateRender();
    }

    public bool IsAxisAlligned()
    {
        return m_axisAlligned;
    }

    public void SetPosRounded(bool rounded)
    {
        m_posRounded = rounded;

        UpdateRender();
    }

    public bool IsPosRounded()
    {
        return m_posRounded;
    }

    public void SetUpdateCallback(Action callback)
    {
        m_callback = callback;
    }

    public void SetGismosScale(Vector3 scale)
    {
        m_gismosScale = scale;
        UpdateRender();
    }

    public Vector3 GetGismosScale()
    {
        return m_gismosScale;
    }

    void UpdateRender()
    {
        var posObject = transform.Find("Pos");
        var scaleObject = transform.Find("Scale");
        var rotObject = transform.Find("Rot");

        if (posObject != null)
            posObject.gameObject.SetActive(m_currentType == GismoType.pos);
        if (scaleObject != null)
            scaleObject.gameObject.SetActive(m_currentType == GismoType.scale);
        if (rotObject != null)
            rotObject.gameObject.SetActive(m_currentType == GismoType.rot);

        if(m_posRounded)
        {
            var posInt = GetGridPos();
            var size = Global.instance.allBlocks.blockSize;

            Vector3 pos = new Vector3(posInt.x * size.x, posInt.y * size.y, posInt.z * size.z);
            transform.position = pos;
        }
        else transform.position = m_pos;

        if (m_axisAlligned)
            transform.rotation = Quaternion.identity;
        else transform.rotation = m_rot;

        transform.localScale = m_gismosScale;
    }

    void UpdateColors()
    {
        int nbChild = transform.childCount;

        for(int i = 0; i < nbChild; i++)
        {
            var c = transform.GetChild(i);

            UpdateColor(c, "X", new Color(1, 0, 0), m_hoveredAxis == GismoAxis.axisX || m_hoveredAxisLastFrame == GismoAxis.axisX);
            UpdateColor(c, "Y", new Color(0, 1, 0), m_hoveredAxis == GismoAxis.axisY || m_hoveredAxisLastFrame == GismoAxis.axisY);
            UpdateColor(c, "Z", new Color(0.2f, 0.3f, 1), m_hoveredAxis == GismoAxis.axisZ || m_hoveredAxisLastFrame == GismoAxis.axisZ);
        }
    }

    void UpdateColor(Transform parent, string name, Color c, bool selected)
    {
        var elem = parent.Find(name);
        if (elem == null)
            return;

        var comp = elem.GetComponentInChildren<MeshRenderer>();
        if (comp == null)
            return;

        if(!selected)
        {
            c.r *= 0.7f;
            c.g *= 0.7f;
            c.b *= 0.7f;
        }

        var mats = comp.materials;
        foreach (var m in mats)
            m.color = c;

        comp.materials = mats;
    }

    public void OnDrag(GismoType type, GismoAxis axis)
    {
        if (type != m_currentType)
            return;

        m_clickedAxis = axis;
    }

    public void OnHover(GismoType type, GismoAxis axis)
    {
        m_hoveredAxis = axis;
        UpdateColors();
    }

    private void Update()
    {
        if(m_clickedAxis != GismoAxis.axisNone)
        {
            if (m_currentType == GismoType.pos)
                OnDragPos(m_clickedAxis);

            if (m_currentType == GismoType.rot)
                OnDragRot(m_clickedAxis);

            if (m_currentType == GismoType.scale)
                OnDragScale(m_clickedAxis);

            m_clickedAxis = GismoAxis.axisNone;
        }

        if(m_hoveredAxis != GismoAxis.axisNone || m_hoveredAxisLastFrame != GismoAxis.axisNone)
        {
            m_hoveredAxisLastFrame = m_hoveredAxis;
            m_hoveredAxis = GismoAxis.axisNone;
            UpdateColors();
        }

        m_oldMousePos = Input.mousePosition;
    }

    void OnDragPos(GismoAxis axis)
    {
        if (m_clickedAxis == GismoAxis.axisNone)
            return;

        var rayOld = m_camera.ScreenPointToRay(m_oldMousePos);
        var rayNew = m_camera.ScreenPointToRay(Input.mousePosition);

        var up = Vector3.zero;
        if (m_clickedAxis == GismoAxis.axisX)
            up = new Vector3(1, 0, 0);
        else if (m_clickedAxis == GismoAxis.axisY)
            up = new Vector3(0, 1, 0);
        else if (m_clickedAxis == GismoAxis.axisZ)
            up = new Vector3(0, 0, 1);

        var planeNormal = Vector3.Cross(up, rayNew.direction);
        planeNormal = Vector3.Cross(up, planeNormal);

        var plane = new Plane(planeNormal, transform.position);

        float enterOld = 0, enterNew = 0;
        
        if (!plane.Raycast(rayOld, out enterOld))
            return;
        if (!plane.Raycast(rayNew, out enterNew))
            return;
        Vector3 posOld = rayOld.GetPoint(enterOld);
        Vector3 posNew = rayNew.GetPoint(enterNew);

        Vector3 offset = posNew - posOld;
        offset.x *= up.x;
        offset.y *= up.y;
        offset.z *= up.z;

        m_pos += offset;

        UpdateRender();
    }

    void OnDragScale(GismoAxis axis)
    {

    }

    void OnDragRot(GismoAxis axis)
    {

    }
}