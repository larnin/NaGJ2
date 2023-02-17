using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Utility
{
    public static float Angle(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(b.y, b.x) - Mathf.Atan2(a.y, a.x);
    }

    public static float Angle(Vector2 vect)
    {
        return Mathf.Atan2(vect.y, vect.x);
    }

    public static Vector2 Project(Vector2 vect, Vector2 dir)
    {
        float a = Angle(vect, dir);

        return dir.normalized * Mathf.Cos(a) * vect.magnitude;
    }

    static float DistanceToPoint(Vector3 pos, Vector3 point)
    {
        return (point - pos).magnitude;
    }

    static float DistanceToPoint(Vector2 pos, Vector2 point)
    {
        return (point - pos).magnitude;
    }

    public static bool IsLeft(Vector2 line1, Vector2 line2, Vector2 pos)
    {
        return ((line2.x - line1.x) * (pos.y - line1.y) - (line2.y - line1.y) * (pos.x - line1.x)) > 0;
    }

    public static bool IsRight(Vector2 line1, Vector2 line2, Vector2 pos)
    {
        return ((line2.x - line1.x) * (pos.y - line1.y) - (line2.y - line1.y) * (pos.x - line1.x)) < 0;
    }

    public static T DeepClone<T>(this T obj)
    {
        using (MemoryStream memory_stream = new MemoryStream())
        {
            // Serialize the object into the memory stream.
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memory_stream, obj);

            // Rewind the stream and use it to create a new object.
            memory_stream.Position = 0;
            return (T)formatter.Deserialize(memory_stream);
        }
    }
}
