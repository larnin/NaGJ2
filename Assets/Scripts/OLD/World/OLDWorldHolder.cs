using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDWorldHolder : MonoBehaviour
{
    [SerializeField] float m_elementSize = 1;

    static OLDWorldHolder m_instance = null;
    public static OLDWorldHolder Instance() { return m_instance; }

    ResizableMatrix<OLDWorldElement> m_world = new ResizableMatrix<OLDWorldElement>(1, 1);

    private void Awake()
    {
        m_instance = this;
    }

    public float GetElementSize()
    {
        return m_elementSize;
    }

    public Vector2Int RayToPos(Ray ray)
    {
        Plane p = new Plane(new Vector3(0, 1, 0), transform.position);
        float enter = 0;
        if(p.Raycast(ray, out enter))
        {
            Vector3 pos = ray.GetPoint(enter);
            return ProjectPos(pos);
        }

        return Vector2Int.zero;
    }

    public Vector2Int ProjectPos(Vector3 pos)
    {
        pos -= transform.position;
        pos /= m_elementSize;

        return new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
    }

    public Vector3 GetElemPos(int x, int y)
    {
        Vector3 pos = transform.position;
        pos.x += x * m_elementSize;
        pos.z += y * m_elementSize;

        return pos;
    }

    public bool SetGround(OLDGroundType type, int x, int y)
    {
        if(!m_world.IsInGrid(x, y))
        {
            if (type == OLDGroundType.empty)
                return true;
            var elem = new OLDWorldElement();
            elem.groundType = type;
            elem.groundSetTime = Time.time;
            m_world.Set(elem, x, y);
            UpdateGroundAt(x, y);
            return true;
        }
        else 
        {
            var elem = m_world.Get(x, y);
            if (elem == null)
            {
                elem = new OLDWorldElement();
                m_world.Set(elem, x, y);
            }
            if (elem.buildingType != OldBuildingType.empty && type == OLDGroundType.empty)
                return false;
            if (elem.groundType != type)
            {
                elem.groundType = type;
                elem.groundSetTime = Time.time;
                UpdateGroundAt(x, y);
            }
        }
        return true;
    }

    public bool SetBuilding(OldBuildingType type, int level, int x, int y)
    {
        if (!m_world.IsInGrid(x, y))
            return type == OldBuildingType.empty;

        var elem = m_world.Get(x, y);
        if (elem == null)
            return type == OldBuildingType.empty;

        if (elem.groundType == OLDGroundType.empty)
            return type == OldBuildingType.empty;

        if(elem.buildingType != type || elem.buildingLevel != level)
        {
            elem.buildingType = type;
            elem.buildingLevel = level;
            UpdateBuildingAt(x, y);
        }
        return true;
    }

    public OLDGroundType GetGround(int x, int y)
    {
        if (!m_world.IsInGrid(x, y))
            return OLDGroundType.empty;

        var elem = m_world.Get(x, y);
        if (elem == null)
            return OLDGroundType.empty;
        return elem.groundType;
    }

    public float GetGroundSetTimer(int x, int y)
    {
        if (!m_world.IsInGrid(x, y))
            return 0;

        var elem = m_world.Get(x, y);
        if (elem == null)
            return 0;
        return elem.groundSetTime;
    }

    public OldBuildingType GetBuilding(int x, int y, out int level)
    {
        level = 0;
        if (!m_world.IsInGrid(x, y))
            return OldBuildingType.empty;

        var elem = m_world.Get(x, y);
        if (elem == null)
            return OldBuildingType.empty;
        level = elem.buildingLevel;
        return elem.buildingType;
    }

    public OldBuildingType GetBuilding(int x, int y)
    {
        if (!m_world.IsInGrid(x, y))
            return OldBuildingType.empty;

        var elem = m_world.Get(x, y);
        if (elem == null)
            return OldBuildingType.empty;
        return elem.buildingType;
    }
    
    public int GetBuildingLevel(int x, int y)
    {
        if (!m_world.IsInGrid(x, y))
            return 0;

        var elem = m_world.Get(x, y);
        if (elem == null)
            return 0;
        return elem.buildingLevel;
    }

    void UpdateGroundAt(int x, int y)
    {
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                int tX = x + i;
                int tY = y + j;

                if (!m_world.IsInGrid(tX, tY))
                    continue;

                var elem = m_world.Get(tX, tY);
                if (elem == null)
                    continue;

                if (elem.groundObject != null)
                    Destroy(elem.groundObject);

                if (OLDElementHolder.Instance() == null)
                    continue;

                var mat = GetGroundNearMatrix(tX, tY);
                Rotation rot = Rotation.rot_0;
                var prefab = OLDElementHolder.Instance().GetGround(elem.groundType, mat, out rot);
                if (prefab == null)
                    continue;

                var obj = Instantiate(prefab);

                obj.transform.parent = transform;
                obj.transform.position = GetElemPos(tX, tY);
                ApplyRotation(obj, rot);
                elem.groundObject = obj;
            }
        }
    }

    void UpdateBuildingAt(int x, int y)
    {
        var elem = m_world.Get(x, y);
        if (elem == null)
            return;

        if(elem.buildingObject != null)
            Destroy(elem.buildingObject);

        if (OLDElementHolder.Instance() == null)
            return;
        var prefab = OLDElementHolder.Instance().GetBuilding(elem.buildingType, elem.buildingLevel);
        if (prefab == null)
            return;

        var obj = Instantiate(prefab);

        obj.transform.parent = transform;
        obj.transform.position = GetElemPos(x, y);

        Rotation rot = (Rotation)UnityEngine.Random.Range(0, 4);
        if (elem.buildingType == OldBuildingType.tower0 || elem.buildingType == OldBuildingType.tower1 || elem.buildingType == OldBuildingType.tower2)
            rot = Rotation.rot_0;
        ApplyRotation(obj, rot);
        elem.buildingObject = obj;

        Event<SetBuildingInfoEvent>.Broadcast(new SetBuildingInfoEvent(x, y), obj);
    }     

    public NearMatrix<bool> GetGroundNearMatrix(int x, int y)
    {
        NearMatrix<bool> mat = new NearMatrix<bool>();

        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                int tX = x + i;
                int tY = y + j;

                if (!m_world.IsInGrid(tX, tY))
                    continue;

                var elem = m_world.Get(tX, tY);
                if (elem != null)
                    mat.Set(elem.groundType != OLDGroundType.empty, i, j);
            }
        }

        return mat;
    }

    public NearMatrix<bool> GetBuildingNearMatrix(int x, int y)
    {
        NearMatrix<bool> mat = new NearMatrix<bool>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int tX = x + i;
                int tY = y + j;

                if (!m_world.IsInGrid(tX, tY))
                    continue;

                var elem = m_world.Get(tX, tY);
                if (elem != null)
                {
                    bool valid = elem.buildingType == OldBuildingType.factory || elem.buildingType == OldBuildingType.house || elem.buildingType == OldBuildingType.scienceLab;
                    mat.Set(valid, i, j);
                }
            }
        }

        return mat;
    }

    public RectInt GetBounds()
    {
        int minX = m_world.MinX();
        int minY = m_world.MinY();
        int maxX = m_world.MaxX();
        int maxY = m_world.MaxY();

        return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    public List<Vector2Int> GetEmptyGroundSpaces()
    {
        List<Vector2Int> spaces = new List<Vector2Int>();

        int minX = m_world.MinX() - 1;
        int minY = m_world.MinY() - 1;
        int maxX = m_world.MaxX() + 1;
        int maxY = m_world.MaxY() + 1;

        for(int i = minX; i <= maxX; i++)
        {
            for(int j = minY; j <= maxY; j++)
            {
                var mat = GetGroundNearMatrix(i, j);

                if (mat.Get(0, 0))
                    continue;

                if (mat.Get(-1, 0) || mat.Get(0, -1) || mat.Get(1, 0) || mat.Get(0, 1))
                    spaces.Add(new Vector2Int(i, j));
            }
        }

        return spaces;
    }

    public List<Vector2Int> GetEmptyBuildingSpaces(float minTimer)
    {
        List<Vector2Int> spaces = new List<Vector2Int>();

        int minX = m_world.MinX();
        int minY = m_world.MinY();
        int maxX = m_world.MaxX();
        int maxY = m_world.MaxY();

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                var elem = m_world.Get(i, j);
                if (elem == null)
                    continue;

                if (elem.groundType == OLDGroundType.deadly || elem.groundType == OLDGroundType.empty)
                    continue;

                if (elem.buildingType != OldBuildingType.empty)
                    continue;

                if (Time.time - elem.groundSetTime < minTimer)
                    continue;

                var mat = GetBuildingNearMatrix(i, j);

                if (mat.Get(0, 0))
                    continue;

                if (mat.Get(-1, 0) || mat.Get(0, -1) || mat.Get(1, 0) || mat.Get(0, 1))
                    spaces.Add(new Vector2Int(i, j));
            }
        }

        return spaces;
    }

    void ApplyRotation(GameObject obj, Rotation rotation)
    {
        if (rotation == Rotation.rot_0)
            return;
        else if(rotation == Rotation.rot_90)
        {
            obj.transform.rotation = Quaternion.Euler(0, 90, 0);
            var pos = obj.transform.position;
            pos.z += m_elementSize;
            obj.transform.position = pos;
        }
        else if(rotation == Rotation.rot_180)
        {
            obj.transform.rotation = Quaternion.Euler(0, 180, 0);
            var pos = obj.transform.position;
            pos.x += m_elementSize;
            pos.z += m_elementSize;
            obj.transform.position = pos;
        }
        else if(rotation == Rotation.rot_270)
        {
            obj.transform.rotation = Quaternion.Euler(0, 270, 0);
            var pos = obj.transform.position;
            pos.x += m_elementSize;
            obj.transform.position = pos;
        }
    }
}