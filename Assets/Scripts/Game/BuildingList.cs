using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    List<BuildingElement> m_buildings = new List<BuildingElement>();
    Dictionary<ulong, int> m_posDictionary = new Dictionary<ulong, int>();
    Dictionary<ulong, int> m_idDictionary = new Dictionary<ulong, int>();

}
