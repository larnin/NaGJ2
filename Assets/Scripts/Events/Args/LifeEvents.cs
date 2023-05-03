using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HitEvent
{
    public float damage;
    public Vector3 position;
    public Vector3 hitDirection;

    public HitEvent(float _damage) : this(_damage, Vector3.zero, Vector3.up) { }

    public HitEvent(float _damage, Vector3 _position, Vector3 _hitDirection)
    {
        damage = _damage;
        position = _position;
        hitDirection = _hitDirection;
    }
}

public class DeathEvent
{

}

public class GetLifeEvent
{
    public float life = 0;
    public float maxLife = 1;
}

public class SetLifeMultiplierEvent
{
    public float multiplier = 1;

    public SetLifeMultiplierEvent(float _multiplier)
    {
        multiplier = _multiplier;
    }
}