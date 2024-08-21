using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
class GameEntityGroup
{
    public EntityType type;
    public int count;
    public float delay;
    public float timeSpacing;

    public int GetNbSpawn(float time)
    {
        if (time < delay)
            return 0;

        time -= delay;
        time /= timeSpacing;

        int nb = Mathf.CeilToInt(time);
        if (nb >= count)
            return count;
        return nb;
    }
}

[Serializable]
 class GameEntityWave
{
    public List<GameEntityGroup> groups = new List<GameEntityGroup>();

    public float GetTotalDuration()
    {
        float maxTime = 0;
        foreach(var g in groups)
        {
            float currentTime = g.delay;
            if (g.count > 1)
                currentTime += g.timeSpacing * (g.count - 1);
            if (currentTime > maxTime)
                maxTime = currentTime;
        }

        return maxTime;
    }

    public int GetTotalCount()
    {
        int nb = 0;
        foreach (var g in groups)
            nb += g.count;
        return nb;
    }
}
