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

    public GameObject GetGround(GroundType type, NearMatrix matrix, out Quaternion rotation)
    {
        foreach (var g in m_grounds)
        {
            if (g.groundType == type)
                return GetGround(g, matrix, out rotation);
        }

        rotation = Quaternion.identity;
        return null;
    }

    GameObject GetGround(GroundDatas data, NearMatrix matrix, out Quaternion rotation)
    {
        //todo variations with matrix
        rotation = Quaternion.identity;
        return data.alone;
    }

    public GameObject GetBuilding(BuildingType type)
    {
        foreach(var b in m_buildings)
        {
            if (b.buildingType == type)
            {
                if (b.building.Count == 0)
                    return null;
                else if (b.building.Count == 1)
                    return b.building[0];
                else return b.building[UnityEngine.Random.Range(0, b.building.Count)];
            }
        }

        return null;
    }
}