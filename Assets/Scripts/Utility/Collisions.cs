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
    public static Shape GetShape(Bounds bounds)
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

    public static bool Intersect(Shape shape1, Shape shape2)
    {
        int nbTriangle1 = shape1.indexs.Count / 3;
        int nbTriangle2 = shape2.indexs.Count / 3;

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
}
