using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Shape
{
    public List<Vector3> points = new List<Vector3>();
    public List<int> indexs = new List<int>();
}


public static class Collisions
{
    public static Shape GetSelectionShape(Bounds bounds)
    {
        var min = bounds.min;
        var max = bounds.max;

        Shape s = new Shape();
        s.points.Add(min);
        s.points.Add(new Vector3(min.x, min.y, max.z));
        s.points.Add(new Vector3(min.x, max.y, min.z));
        s.points.Add(new Vector3(min.x, max.y, max.z));
        s.points.Add(new Vector3(max.x, min.y, min.z));
        s.points.Add(new Vector3(max.x, min.y, max.z));
        s.points.Add(new Vector3(max.x, max.y, min.z));
        s.points.Add(max);

        s.indexs.Add(0); s.indexs.Add(1); s.indexs.Add(2);
        s.indexs.Add(4); s.indexs.Add(6); s.indexs.Add(5);
        s.indexs.Add(0); s.indexs.Add(4); s.indexs.Add(1);
        s.indexs.Add(2); s.indexs.Add(3); s.indexs.Add(6);
        s.indexs.Add(0); s.indexs.Add(2); s.indexs.Add(4);
        s.indexs.Add(1); s.indexs.Add(5); s.indexs.Add(3);

        return s;
    }

    public static Shape GetFullShape(Bounds bounds)
    {
        var min = bounds.min;
        var max = bounds.max;

        Shape s = new Shape();
        s.points.Add(min);
        s.points.Add(new Vector3(min.x, min.y, max.z));
        s.points.Add(new Vector3(min.x, max.y, min.z));
        s.points.Add(new Vector3(min.x, max.y, max.z));
        s.points.Add(new Vector3(max.x, min.y, min.z));
        s.points.Add(new Vector3(max.x, min.y, max.z));
        s.points.Add(new Vector3(max.x, max.y, min.z));
        s.points.Add(max);

        s.indexs.Add(0); s.indexs.Add(1); s.indexs.Add(2);
        s.indexs.Add(3); s.indexs.Add(2); s.indexs.Add(1);

        s.indexs.Add(4); s.indexs.Add(6); s.indexs.Add(5);
        s.indexs.Add(5); s.indexs.Add(6); s.indexs.Add(7);

        s.indexs.Add(0); s.indexs.Add(4); s.indexs.Add(1);
        s.indexs.Add(5); s.indexs.Add(1); s.indexs.Add(4);

        s.indexs.Add(2); s.indexs.Add(3); s.indexs.Add(6);
        s.indexs.Add(3); s.indexs.Add(7); s.indexs.Add(6);

        s.indexs.Add(0); s.indexs.Add(2); s.indexs.Add(4);
        s.indexs.Add(2); s.indexs.Add(6); s.indexs.Add(4);

        s.indexs.Add(1); s.indexs.Add(5); s.indexs.Add(3);
        s.indexs.Add(5); s.indexs.Add(7); s.indexs.Add(3);

        return s;
    }

    struct RaycastElt
    {
        public Vector3 t1;
        public Vector3 t2;
        public Vector3 t3;
        public Vector3 p1;
        public Vector3 p2;

        public RaycastElt(Vector3 _t1, Vector3 _t2, Vector3 _t3, Vector3 _p1, Vector3 _p2)
        {
            t1 = _t1;
            t2 = _t2;
            t3 = _t3;
            p1 = _p1;
            p2 = _p2;
        }
    }

    public static bool Intersect(Shape shape1, Shape shape2)
    {
        int nbTriangle1 = shape1.indexs.Count / 3;
        int nbTriangle2 = shape2.indexs.Count / 3;

        //point - triangle
        foreach(var p in shape1.points)
        {
            bool left = true;
            for(int i = 0; i < nbTriangle2; i++)
            {
                var p1 = shape2.points[shape2.indexs[i * 3]];
                var p2 = shape2.points[shape2.indexs[i * 3 + 1]];
                var p3 = shape2.points[shape2.indexs[i * 3 + 2]];

                left &= IsLeft(p1, p2, p3, p);
                if (!left)
                    break;
            }
            if (left)
                return true;
        }

        foreach (var p in shape2.points)
        {
            bool left = true;
            for (int i = 0; i < nbTriangle1; i++)
            {
                var p1 = shape1.points[shape1.indexs[i * 3]];
                var p2 = shape1.points[shape1.indexs[i * 3 + 1]];
                var p3 = shape1.points[shape1.indexs[i * 3 + 2]];

                left &= IsLeft(p1, p2, p3, p);
                if (!left)
                    break;
            }
            if (left)
                return true;
        }

        //edge - triangle
        for(int i = 0; i < nbTriangle1; i++)
        {
            var p11 = shape1.points[shape1.indexs[i * 3]];
            var p12 = shape1.points[shape1.indexs[i * 3 + 1]];
            var p13 = shape1.points[shape1.indexs[i * 3 + 2]];

            for (int j = 0; j < nbTriangle2; j++)
            {
                var p21 = shape2.points[shape2.indexs[i * 3]];
                var p22 = shape2.points[shape2.indexs[i * 3 + 1]];
                var p23 = shape2.points[shape2.indexs[i * 3 + 2]];

                var elems = new RaycastElt[]
                {
                    new RaycastElt(p11, p12, p13, p21, p22),
                    new RaycastElt(p11, p12, p13, p21, p23),
                    new RaycastElt(p11, p12, p13, p22, p23),

                    new RaycastElt(p21, p22, p23, p11, p12),
                    new RaycastElt(p21, p22, p23, p11, p13),
                    new RaycastElt(p21, p22, p23, p12, p13)
                };

                foreach(var elem in elems)
                {
                    Vector3 dir = elem.p2 - elem.p1;
                    float dist = dir.magnitude;
                    dir /= dist;
                    Vector3 hitPos;
                    var hit = RaycastTriangle(elem.t1, elem.t2, elem.t3, elem.p1, dir, out hitPos);
                    if (!hit)
                        continue;
                    float hitDist = (hitPos - elem.p1).sqrMagnitude;
                    if (hitDist < dist * dist)
                        return true;
                }
            }
        }

        return false;
    }

    public static bool Raycast(Shape shape, Vector3 pos, Vector3 dir, out Vector3 hit, out Vector3 normal)
    {
        hit = Vector3.zero;
        normal = Vector3.up;

        int nbTriangle = shape.indexs.Count / 3;

        Vector3 bestHit = Vector3.zero;
        float bestDist = 0;
        int hitIndex = -1;

        for(int i = 0; i < nbTriangle; i++)
        {
            var p1 = shape.points[shape.indexs[i * 3]];
            var p2 = shape.points[shape.indexs[i * 3 + 1]];
            var p3 = shape.points[shape.indexs[i * 3 + 2]];

            Vector3 localHit;
            bool hitLocal = RaycastTriangle(p1, p2, p3, pos, dir, out localHit);
            if (!hitLocal)
                continue;

            float localDist = (localHit - pos).sqrMagnitude;
            if(hitIndex < 0 || bestDist > localDist)
            {
                bestHit = localHit;
                bestDist = localDist;
                hitIndex = i;
            }
        }

        if(hitIndex >= 0)
        {
            hit = bestHit;

            var p1 = shape.points[shape.indexs[hitIndex * 3]];
            var p2 = shape.points[shape.indexs[hitIndex * 3 + 1]];
            var p3 = shape.points[shape.indexs[hitIndex * 3 + 2]];

            normal = Normal(p1, p2, p3);

            return true;
        }
        return false;
    }

    public static bool RaycastTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 rayStart, Vector3 rayDir, out Vector3 hit)
    {
        //https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm

        hit = Vector3.zero;

        float epsilon = float.Epsilon;

        Vector3 edge1 = p2 - p1;
        Vector3 edge2 = p3 - p1;
        Vector3 rayCrossE2 = Vector3.Cross(rayDir, edge2);
        float det = Vector3.Dot(edge1, rayCrossE2);

        if (det > -epsilon && det < epsilon)
            return false;

        float invDet = 1.0f / det;
        Vector3 s = rayStart - p1;
        float u = invDet * Vector3.Dot(s, rayCrossE2);

        if (u < 0 || u > 1)
            return false;

        Vector3 sCrossE1 = Vector3.Cross(s, edge1);
        float v = invDet * Vector3.Dot(rayDir, sCrossE1);

        if (v < 0 || u + v > 1)
            return false;

        // At this stage we can compute t to find out where the intersection point is on the line.
        float t = invDet * Vector3.Dot(edge2, sCrossE1);
        if (t > epsilon)
        {
            hit = rayStart + rayDir * t;
            return true;
        }

        return false;
    }

    public static Vector3 Normal(Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        Vector3 a = pos2 - pos1;
        Vector3 b = pos3 - pos1;

        return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    static bool IsLeft(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 p)
    {
        Vector3 vect = p - pos1;
        Vector3 norm = Normal(pos1, pos2, pos3);

        return Vector3.Dot(vect, norm) < 0;
    }

    public static void DrawShape(Shape shape, Vector3 scale, Color color)
    {
        int nbTriangle = shape.indexs.Count / 3;

        for(int i = 0; i < nbTriangle; i++)
        {
            var pos1 = shape.points[shape.indexs[i * 3]];
            var pos2 = shape.points[shape.indexs[i * 3 + 1]];
            var pos3 = shape.points[shape.indexs[i * 3 + 2]];

            pos1 = new Vector3(pos1.x * scale.x, pos1.y * scale.y, pos1.z * scale.z);
            pos2 = new Vector3(pos2.x * scale.x, pos2.y * scale.y, pos2.z * scale.z);
            pos3 = new Vector3(pos3.x * scale.x, pos3.y * scale.y, pos3.z * scale.z);

            DebugDraw.Triangle(pos1, pos2, pos3, color);
        }
    }
}
