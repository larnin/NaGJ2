using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WorldElement
{
    public GameObject groundObject;
    public GameObject buildingObject;

    public GroundType groundType = GroundType.empty;
    public BuildingType buildingType = BuildingType.empty;
}