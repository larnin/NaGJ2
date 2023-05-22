using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Sphere
{
    public Vector3 center;
    public float radius;

    public Sphere(Vector3 _center, float _radius)
    {
        center = _center;
        radius = _radius;
    }
}

public struct Segment
{
    public Vector3 pos1;
    public Vector3 pos2;

    public Segment(Vector3 _pos1, Vector3 _pos2)
    {
        pos1 = _pos1;
        pos2 = _pos2;
    }
}

public class ConvexeShape
{

    Plane m_plane;
    public Vector3 m_planeX;
    public Vector3 m_planeY;

    List<Vector2> m_points = new List<Vector2>();

    public ConvexeShape()
    {
        m_plane = new Plane(Vector3.up, Vector3.zero);
    }

    public ConvexeShape(Plane p)
    {
        m_plane = p;
        SetPlane();
    }

    public void Clear(Plane p)
    {
        m_points.Clear();
        m_plane = p;
        SetPlane();
    }

    void SetPlane()
    {
        Vector3 dir = Vector3.Cross(m_plane.normal, Vector3.left);
        m_planeX = Vector3.Cross(m_plane.normal, dir);
        m_planeY = Vector3.Cross(m_plane.normal, m_planeX);
    }

    public void AddPoint(Vector3 point)
    {
        Vector2 inPlanePoint = ProjectInPlane(point);

         m_points.Add(inPlanePoint);
    }

    public int GetNbPoints()
    {
        return m_points.Count;
    }

    public Vector3 GetPoint(int index)
    {
        if (index < 0 || index > GetNbPoints())
            return Vector3.zero;

        return PointToWorld(m_points[index]);
    }

    public Vector3 GetNormal()
    {
        return m_plane.normal;
    }

    public Vector2 GetRawPoint(int index)
    {
        if (index < 0 || index > GetNbPoints())
            return Vector3.zero;

        return m_points[index];
    }

    public Vector2 ProjectInPlane(Vector3 point)
    {
        Vector3 normal = m_plane.normal;
        Vector3 origin = -normal * m_plane.distance;

        Vector3 pointInPlane = m_plane.ClosestPointOnPlane(point);
        pointInPlane -= origin;
        return new Vector2(Vector3.Dot(m_planeX, pointInPlane), Vector3.Dot(m_planeY, pointInPlane));
    }

    Vector3 PointToWorld(Vector2 point)
    {
        Vector3 normal = m_plane.normal;
        Vector3 origin = -normal * m_plane.distance;

        return origin + point.x * m_planeX + point.y * m_planeY;
    }

    public void Compute(float posEpsilon)
    {
        //Graham scan
        if (m_points.Count <= 3)
            return;

        RemoveNearPoints(posEpsilon);

        m_points = GenerateShape();
    }

    Rect GetRect()
    {
        if (m_points.Count == 0)
            return new Rect();

        Rect bounds = new Rect(m_points[0], Vector2.zero);
        for (int i = 1; i < m_points.Count; i++)
        {
            Vector2 p = m_points[i];
            if (p.x < bounds.x)
            {
                bounds.width += bounds.x - p.x;
                bounds.x = p.x;
            }
            else if (p.x > bounds.x + bounds.width)
                bounds.x = p.x - bounds.x;
            if (p.y < bounds.y)
            {
                bounds.height += bounds.y - p.y;
                bounds.y = p.y;
            }
            else if (p.y > bounds.y + bounds.height)
                bounds.y = p.y - bounds.y;
        }

        return bounds;
    }

    public Bounds GetBounds()
    {
        if (m_points.Count == 0)
            return new Bounds();

        Bounds b = new Bounds(PointToWorld(m_points[0]), Vector3.zero);
        for (int i = 1; i < m_points.Count; i++)
            b.Encapsulate(PointToWorld(m_points[i]));
        return b;
    }

    void RemoveNearPoints(float posEpsilon)
    {
        float maxDist = posEpsilon * posEpsilon;

        for(int i = 0; i < m_points.Count; i++)
        {
            for(int j = i + 1; j < m_points.Count; j++)
            {
                float dist = (m_points[i] - m_points[j]).sqrMagnitude;
                if(dist < maxDist)
                {
                    m_points.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }
    }

    struct SortData
    {
        public int index;
        public float angle;
    }

    List<Vector2> GenerateShape()
    {
        List<Vector2> points = new List<Vector2>();

        int firstPointIndex = 0;
        for(int i = 1; i < m_points.Count; i++)
        {
            if (m_points[i].y < m_points[firstPointIndex].y)
                firstPointIndex = i;
            else if (m_points[i].y == m_points[firstPointIndex].y && m_points[i].x < m_points[firstPointIndex].x)
                firstPointIndex = i;
        }
        points.Add(m_points[firstPointIndex]);

        List<SortData> data = new List<SortData>();
        for(int i = 0; i < m_points.Count; i++)
        {
            if (i == firstPointIndex)
                continue;

            SortData d;
            d.index = i;
            Vector2 dir = m_points[i] - m_points[firstPointIndex];
            d.angle = dir.x / dir.y;
            data.Add(d);
        }

        data.Sort((a, b) => { return a.angle.CompareTo(b.angle); });

        points.Add(m_points[data[0].index]);

        for(int i = 1; i < data.Count; i++)
        {
           points.Add(m_points[data[i].index]);

            while (points.Count >= 3)
            {
                Vector2 p1 = points[points.Count - 1];
                Vector2 p2 = points[points.Count - 2];
                Vector2 p3 = points[points.Count - 3];

                if (Utility.IsLeft(p3, p2, p1))
                    points.RemoveAt(points.Count - 2);
                else break;
            }
        }

        return points;
    }

    public bool IsInShape(Vector3 point)
    {
        return IsInShape(ProjectInPlane(point));
    }

    bool IsInShape(Vector2 point)
    {
        //the point must be on the same side of all the vectors of the shape

        bool isLeftShape = false;
        for (int i = 0; i < m_points.Count; i++)
        {
            int index2 = i == 0 ? m_points.Count - 1 : i - 1;

            bool localLeft = Utility.IsLeft(m_points[i], m_points[index2], point);
            if (i == 0)
                isLeftShape = localLeft;

            if (isLeftShape != localLeft)
                return false;
        }

        return true;
    }

}

public static class CutMesh
{
    public static ConvexeShape CutSurface(MeshFilter meshInput, Plane plane)
    {
        var transform = meshInput.transform;

        var mesh = meshInput.mesh;
        if (mesh == null)
            return null;

        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        for(int i = 0; i < vertices.Length; i++)
            vertices[i] = transform.TransformPoint(vertices[i]);

        List<Segment> segments = new List<Segment>();

        int nbTriangles = triangles.Length / 3;
        for(int i = 0; i < nbTriangles; i++)
        {
            var pos1 = vertices[triangles[i * 3]];
            var pos2 = vertices[triangles[i * 3 + 1]];
            var pos3 = vertices[triangles[i * 3 + 2]];

            var sphere = GetBounds(pos1, pos2, pos3);

            float dist = plane.GetDistanceToPoint(sphere.center);
            if (dist > sphere.radius)
                continue;

            Segment segment;
            bool valid = Intersect(plane, pos1, pos2, pos3, out segment);
            if (valid)
                segments.Add(segment);
        }

        ConvexeShape shape = new ConvexeShape(plane);
        for(int i = 0; i < segments.Count; i++)
        {
            shape.AddPoint(segments[i].pos1);
            shape.AddPoint(segments[i].pos2);
        }
        shape.Compute(0.01f);

        return shape;
    }

    public static Sphere GetBounds(Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        Vector3 center = (pos1 + pos2 + pos3) / 3;

        float radius1 = (center - pos1).sqrMagnitude;
        float radius2 = (center - pos2).sqrMagnitude;
        float radius3 = (center - pos3).sqrMagnitude;

        float radius = MathF.Max(radius1, MathF.Max(radius2, radius3));

        return new Sphere(center, MathF.Sqrt(radius));
    }

    public static bool Intersect(Plane p, Vector3 pos1, Vector3 pos2, Vector3 pos3, out Segment segment)
    {
        Vector3[] points = new Vector3[] { pos1, pos2, pos3 };
        Vector3[] outputs = new Vector3[points.Length];
        int nbOutputs = 0;

        for(int i = 0; i < points.Length; i++)
        {
            int index2 = i == 0 ? points.Length - 1 : i - 1;

            Vector3 pos = Vector3.zero;
            if (Intersect(p, points[i], points[index2], out pos))
                outputs[nbOutputs++] = pos;
        }

        if(nbOutputs == 0)
        {
            segment = new Segment();
            return false;
        }    
        else if(nbOutputs == 1)
        {
            segment = new Segment(outputs[0], outputs[0]);
            return true;
        }
        else if(nbOutputs == 2)
        {
            segment = new Segment(outputs[0], outputs[1]);
            return true;
        }
        else //nbOutputs == 3 - Need to remove sus
        {
            float d1 = (outputs[0] - outputs[1]).sqrMagnitude;
            float d2 = (outputs[1] - outputs[2]).sqrMagnitude;
            float d3 = (outputs[2] - outputs[0]).sqrMagnitude;

            if (d1 > d2 && d1 > d3)
                segment = new Segment(outputs[0], outputs[1]);
            else if (d2 > d1 && d2 > d3)
                segment = new Segment(outputs[1], outputs[2]);
            else segment = new Segment(outputs[2], outputs[3]);
            return true;
        }    
    }

    public static bool Intersect(Plane p, Vector3 pos1, Vector3 pos2, out Vector3 pos)
    {
        float enter = 0;
        var ray = new Ray(pos1, pos2 - pos1);

        float dist = (pos2 - pos1).magnitude;

        if (!p.Raycast(ray, out enter) || enter > dist)
        {
            pos = Vector3.zero;
            return false;
        }

        pos = ray.GetPoint(enter);
        return true;
    }

    public static void MakeShape(Mesh mesh, ConvexeShape shape, Vector3 center, float thickness)
    {
        SimpleMeshParam<NormalUVVertexDefinition> vertices = new SimpleMeshParam<NormalUVVertexDefinition>();
        AddShape(vertices, shape, center, thickness);

        MakeShape(mesh, vertices, center);
    }

    public static void MakeShape(Mesh mesh, SimpleMeshParam<NormalUVVertexDefinition> meshParams, Vector3 center)
    {
        var data = meshParams.GetMesh(0);

        MeshEx.SetNormalUVMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        Bounds b = GetBounds(data);
        b.center = b.center - center;

        mesh.bounds = b;
    }

    static Bounds GetBounds(MeshParamData<NormalUVVertexDefinition> meshParams)
    {
        if (meshParams.verticesSize == 0)
            return new Bounds();

        Bounds b = new Bounds(meshParams.vertices[0].pos, Vector3.zero);
        for(int i = 1; i < meshParams.verticesSize; i++)
        {
            b.Encapsulate(meshParams.vertices[i].pos);
        }

        return b;
    }

    public static void AddShape(SimpleMeshParam<NormalUVVertexDefinition> meshParams, ConvexeShape shape, Vector3 center, float thickness)
    {
        var data = meshParams.Allocate(shape.GetNbPoints() * 3, (3 * shape.GetNbPoints() + shape.GetNbPoints() - 2) * 3);

        // make center
        for (int i = 0; i < shape.GetNbPoints(); i++)
        {
            data.vertices[data.verticesSize + i].pos = shape.GetPoint(i) - center;
            data.vertices[data.verticesSize + i].uv = shape.GetRawPoint(i);
            data.vertices[data.verticesSize + i].normal = new Vector3(1, 0, 0);
        }
        data.verticesSize += shape.GetNbPoints();

        for (int i = 2; i < shape.GetNbPoints(); i++)
        {
            int index = i - 2;
            data.indexes[data.indexesSize + index * 3] = 0;
            data.indexes[data.indexesSize + index * 3 + 1] = (ushort)(i);
            data.indexes[data.indexesSize + index * 3 + 2] = (ushort)(i - 1);
        }
        data.indexesSize = (shape.GetNbPoints() - 2) * 3;

        // make border
        Vector3 normal = shape.GetNormal();
        for (int i = 0; i < shape.GetNbPoints(); i++)
        {
            int i2 = i == 0 ? shape.GetNbPoints() - 1 : i - 1;

            Vector3 p1 = shape.GetPoint(i2);
            Vector3 p2 = shape.GetPoint(i);

            Vector3 dir = (p2 - p1).normalized;
            Vector3 ortho = Vector3.Cross(dir, normal);

            p1 -= ortho * thickness;
            p2 -= ortho * thickness;

            int vi1 = data.verticesSize + 2 * i;
            int vi2 = vi1 + 1;
            int vi3 = i < shape.GetNbPoints() - 1 ? vi2 + 1 : data.verticesSize;

            data.vertices[vi1].pos = p1 - center;
            data.vertices[vi1].normal = new Vector3(0, 1, 0);
            data.vertices[vi1].uv = shape.ProjectInPlane(p1);

            data.vertices[vi2].pos = p2 - center;
            data.vertices[vi2].normal = new Vector3(0, 1, 0);
            data.vertices[vi2].uv = shape.ProjectInPlane(p2);

            data.indexes[data.indexesSize + i * 9] = (ushort)(i);
            data.indexes[data.indexesSize + i * 9 + 1] = (ushort)(vi1);
            data.indexes[data.indexesSize + i * 9 + 2] = (ushort)(i2);

            data.indexes[data.indexesSize + i * 9 + 3] = (ushort)(i);
            data.indexes[data.indexesSize + i * 9 + 4] = (ushort)(vi2);
            data.indexes[data.indexesSize + i * 9 + 5] = (ushort)(vi1);

            data.indexes[data.indexesSize + i * 9 + 6] = (ushort)(i);
            data.indexes[data.indexesSize + i * 9 + 7] = (ushort)(vi3);
            data.indexes[data.indexesSize + i * 9 + 8] = (ushort)(vi2);
        }
        data.verticesSize += shape.GetNbPoints() * 2;
        data.indexesSize += shape.GetNbPoints() * 3 * 3;
    }
}


