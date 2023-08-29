using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum Rotation
{
    rot_0,
    rot_90,
    rot_180,
    rot_270,
}

public static class RotationEx
{
    public static Rotation RandomRotation()
    {
        return (Rotation)UnityEngine.Random.Range(0, 4);
    }

    public static Quaternion ToQuaternion(Rotation rot)
    {
        if (rot == Rotation.rot_0)
            return Quaternion.identity;
        else if (rot == Rotation.rot_90)
            return Quaternion.Euler(0, 90, 0);
        else if (rot == Rotation.rot_180)
            return Quaternion.Euler(0, 180, 0);
        else return Quaternion.Euler(0, 270, 0);
    }

    public static Rotation Add(Rotation rot, Rotation adding)
    {
        int nbValue = Enum.GetValues(typeof(Rotation)).Length;

        int newRot = (int)rot + (int)adding;
        if (newRot >= nbValue)
            newRot -= nbValue;

        return (Rotation)newRot;
    }

    public static Rotation Sub(Rotation rot, Rotation sub)
    {
        int nbValue = Enum.GetValues(typeof(Rotation)).Length;

        int newRot = (int)rot - (int)sub;
        if (newRot < 0)
            newRot += nbValue;

        return (Rotation)newRot;
    }

    public static Vector2Int ToVectorInt(Rotation rot)
    {
        if (rot == Rotation.rot_0)
            return new Vector2Int(1, 0);
        if (rot == Rotation.rot_90)
            return new Vector2Int(0, 1);
        if (rot == Rotation.rot_180)
            return new Vector2Int(-1, 0);
        return new Vector2Int(0, -1);
    }

    public static Vector2 ToVector(Rotation rot)
    {
        if (rot == Rotation.rot_0)
            return new Vector2(1, 0);
        if (rot == Rotation.rot_90)
            return new Vector2(0, 1);
        if (rot == Rotation.rot_180)
            return new Vector2(-1, 0);
        return new Vector2(0, -1);
    }

    public static Rotation FromVector(Vector2Int dir)
    {
        bool h = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        if(h)
        {
            if (dir.x > 0)
                return Rotation.rot_0;
            return Rotation.rot_180;
        }
        if (dir.y > 0)
            return Rotation.rot_90;
        return Rotation.rot_270;
    }

    public static Rotation FromVector(Vector2 dir)
    {
        bool h = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        if (h)
        {
            if (dir.x > 0)
                return Rotation.rot_0;
            return Rotation.rot_180;
        }
        if (dir.y > 0)
            return Rotation.rot_90;
        return Rotation.rot_270;
    }

    public static Vector2Int Rotate(Vector2Int dir, Rotation rot)
    {
        int count = (int)rot;
        for(int i = 0; i < count; i++)
        {
            int temp = dir.x;
            dir.x = -dir.y;
            dir.y = temp;
        }
        return dir;
    }

    public static Vector2 Rotate(Vector2 dir, Rotation rot)
    {
        int count = (int)rot;
        for (int i = 0; i < count; i++)
        {
            float temp = dir.x;
            dir.x = -dir.y;
            dir.y = dir.x;
        }
        return dir;
    }
}
