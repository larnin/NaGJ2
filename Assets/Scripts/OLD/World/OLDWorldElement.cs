using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDWorldElement
{
    public GameObject groundObject;
    public OLDGroundType groundType = OLDGroundType.empty;
    public float groundSetTime = 0;

    public GameObject buildingObject;
    public OldBuildingType buildingType = OldBuildingType.empty;
    public int buildingLevel = 0;
}