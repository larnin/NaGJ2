using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum OLDGroundType
{
    empty,
    deadly,
    low,
    normal,
    hight,
}

public enum OldBuildingType
{
    empty,
    house,
    factory,
    scienceLab,
    tower0,
    tower1,
    tower2
}

[Serializable]
public class OLDGroundDatas
{
    public OLDGroundType groundType;
    public GameObject alone;
    public GameObject center;
    public GameObject corner;
    public GameObject border;
    public GameObject doubleBorder;
    public GameObject oneSide;
    public int cost;
}

[Serializable]
public class OLDOneBuildingLevelData
{
    public int cost;
    public int corruptCount;
    public GameObject building;
}

[Serializable]
public class OLDBuildingData
{
    public OldBuildingType buildingType;
    public List<OLDOneBuildingLevelData> building;
}

public class OLDElementHolder : MonoBehaviour
{
    [SerializeField] List<OLDGroundDatas> m_grounds = new List<OLDGroundDatas>();
    [SerializeField] List<OLDBuildingData> m_buildings = new List<OLDBuildingData>();

    static OLDElementHolder m_instance = null;
    public static OLDElementHolder Instance() { return m_instance; }

    private void Awake()
    {
        m_instance = this;
    }

    public GameObject GetGround(OLDGroundType type, NearMatrix<bool> matrix, out Rotation rotation)
    {
        foreach (var g in m_grounds)
        {
            if (g.groundType == type)
                return GetGround(g, matrix, out rotation);
        }

        rotation = Rotation.rot_0;
        return null;
    }

    GameObject GetGround(OLDGroundDatas data, NearMatrix<bool> matrix, out Rotation rotation)
    {
        bool top = matrix.Get(0, -1);
        bool down = matrix.Get(0, 1);
        bool left = matrix.Get(-1, 0);
        bool right = matrix.Get(1, 0);
        int nb = (top ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        if(nb == 4)
        {
            rotation = RotationEx.RandomRotation();
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
            rotation = RotationEx.RandomRotation();
            return data.alone;
        }
    }

    public int GetGroundCost(OLDGroundType type)
    {
        foreach (var g in m_grounds)
        {
            if (g.groundType == type)
                return g.cost;
        }

        return 0;
    }

    public int GetMaxBuildingLevel(OldBuildingType type)
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

    public GameObject GetBuilding(OldBuildingType type, int level)
    {
        foreach(var b in m_buildings)
        {
            if (b.buildingType == type)
            {
                if (b.building.Count <= level)
                    return null;
                return b.building[level].building;
            }
        }

        return null;
    }

    public int GetBuildingCost(OldBuildingType type, int level)
    {
        foreach (var b in m_buildings)
        {
            if (b.buildingType == type)
            {
                if (b.building.Count <= level)
                    return 0;
                return b.building[level].cost;
            }
        }

        return 0;
    }

    public int GetBuildingCorruption(OldBuildingType type, int level)
    {
        foreach (var b in m_buildings)
        {
            if (b.buildingType == type)
            {
                if (b.building.Count <= level)
                    return 0;
                return b.building[level].corruptCount;
            }
        }

        return 0;
    }
}