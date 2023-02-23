using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [NonSerialized] public float damageMultiplier = 1;
    [NonSerialized] public float speedMultiplier = 1;
    [NonSerialized] public float radiusMultiplier = 1;
    [NonSerialized] public Transform target = null;

    [NonSerialized] public int ennemyLayer;
}
