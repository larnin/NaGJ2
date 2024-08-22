using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEntityGroup
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

 public class GameEntityWave
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

    public void Reset()
    {
        groups.Clear();
    }

    public void Load(JsonObject obj)
    {
        Reset();

        var dataArray = obj.GetElement("List")?.JsonArray();
        if (dataArray == null)
            return;

        foreach(var data in dataArray)
        {
            var groupObj = data.JsonObject();
            if (groupObj == null)
                continue;

            GameEntityGroup group = new GameEntityGroup();

            string str = obj.GetElement("Type")?.String();
            if (str == null)
                continue;

            if (!Enum.TryParse(str, out group.type))
                continue;

            group.count = obj.GetElement("Count")?.Int() ?? 0;
            group.delay = obj.GetElement("Delay")?.Float() ?? 0;
            group.timeSpacing = obj.GetElement("Spacing")?.Float() ?? 0;

            groups.Add(group);
        }
    }

    public void Save(JsonObject obj)
    {
        var dataArray = new JsonArray();
        obj.AddElement("List", dataArray);

        foreach(var group in groups)
        {
            var data = new JsonObject();
            dataArray.Add(data);

            data.AddElement("Type", group.type.ToString());
            data.AddElement("Count", group.count);
            data.AddElement("Delay", group.delay);
            data.AddElement("Spacing", group.timeSpacing);
        }
    }
}
