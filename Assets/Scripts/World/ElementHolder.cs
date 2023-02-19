using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GroundType
{
    empty,
    normal,
}

public enum BuildingType
{
    empty,
}

public enum Rotation
{
    rot_0,
    rot_90,
    rot_180,
    rot_270,
}

[Serializable]
public class GroundDatas
{
    public GroundType groundType;
    public GameObject alone;
    public GameObject center;
    public GameObject corner;
    public GameObject border;
    public GameObject doubleBorder;
    public GameObject oneSide;
}

[Serializable]
public class BuildingData
{
    public BuildingType buildingType;
    public List<GameObject> building;
}

public class ElementHolder : MonoBehaviour
{
    [SerializeField] List<GroundDatas> m_grounds = new List<GroundDatas>();
    [SerializeField] List<BuildingData> m_buildings = new List<BuildingData>();

    static ElementHolder m_instance = null;
    public static ElementHolder Instance() { return m_instance; }

    private void Awake()
    {
        m_instance = this;
    }

    public GameObject GetGround(GroundType type, NearMatrix matrix, out Rotation rotation)
    {
        foreach (var g in m_grounds)
        {
            if (g.groundType == type)
                return GetGround(g, matrix, out rotation);
        }

        rotation = Rotation.rot_0;
        return null;
    }

    Rotation RandomRotation()
    {
        return (Rotation)UnityEngine.Random.Range(0, 4);
    }

    GameObject GetGround(GroundDatas data, NearMatrix matrix, out Rotation rotation)
    {
        bool top = matrix.Get(0, -1);
        bool down = matrix.Get(0, 1);
        bool left = matrix.Get(-1, 0);
        bool right = matrix.Get(1, 0);
        int nb = (top ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        if(nb == 4)
        {
            rotation = RandomRotation();
            return data.center;
        }
        else if(nb == 3)
        {
            if (!top)
                rotation = Rotation.rot_0;
            else if (!right)
                rotation = Rotation.rot_270;
            else if (!down)
                rotation = Rotation.rot_180;
            else rotation = Rotation.rot_90;
            return data.border;
        }
        else if(nb == 2)
        {
            if(!top && !down)
            {
                rotation = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_0 : Rotation.rot_180;
                return data.doubleBorder;
            }
            else if(!left && !right)
            {

                rotation = UnityEngine.Random.Range(0, 2) == 0 ? Rotation.rot_90 : Rotation.rot_270;
                return data.doubleBorder;
            }
            else
            {
                if (!left && !top)
                    rotation = Rotation.rot_90;
                else if (!top && !right)
                    rotation = Rotation.rot_0;
                else if (!right && !down)
                    rotation = Rotation.rot_270;
                else rotation = Rotation.rot_180;

                return data.corner;
            }
        }
        else if(nb == 1)
        {
            if (top)
                rotation = Rotation.rot_270;
            else if (right)
                rotation = Rotation.rot_180;
            else if (down)
                rotation = Rotation.rot_90;
            else rotation = Rotation.rot_0;
            return data.oneSide;
        }
        else
        {
            rotation = RandomRotation();
            return data.alone;
        }
    }

    public int GetMaxBuildingLevel(BuildingType type)
    {
        foreach (var b in m_buildings)
        {
            if (b.buildingType == type)
            {
                return b.building.Count;
            }
        }

        return 0;
    }

    public GameObject GetBuilding(BuildingType type, int level)
    {
        foreach(var b in m_buildings)
        {
            if (b.buildingType == type)
            {
                if (b.building.Count <= level)
                    return null;
                return b.building[level];
            }
        }

        return null;
    }
}