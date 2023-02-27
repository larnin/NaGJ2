using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HitEvent
{
    public float damage;

    public HitEvent(float _damage)
    {
        damage = _damage;
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