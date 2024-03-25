using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BeltDirection
{
    Horizontal,
    Up,
    Down,
}

public class BuildingElement
{
    public int ID;
    public BuildingType buildingType;
    public BeltDirection beltDirection;
    public int level;
    public Vector3Int pos;
    public Rotation rotation;
    public GameObject instance;
    public Team team;

    public BuildingElement Clone()
    {
        BuildingElement b = new BuildingElement();
        b.ID = ID;
        b.buildingType = buildingType;
        b.beltDirection = beltDirection;
        b.level = level;
        b.pos = pos;
        b.rotation = rotation;
        b.instance = instance;
        b.team = team;

        return b;
    }
}


public static class BuildingEx
{
    public static void Save(JsonDocument doc, List<BuildingElement> buildings, int nextBuildingID)
    {
        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var worldObject = new JsonObject();
            rootObject.AddElement("Buildings", worldObject);

            var buildingArray = new JsonArray();
            worldObject.AddElement("List", buildingArray);
            worldObject.AddElement("NextID", new JsonNumber(nextBuildingID));

            foreach (var b in buildings)
            {
                var buildingObject = new JsonObject();
                buildingArray.Add(buildingObject);
                buildingObject.AddElement("ID", b.ID);
                buildingObject.AddElement("Level", b.level);
                buildingObject.AddElement("Pos", Json.FromVector3Int(b.pos));
                buildingObject.AddElement("Type", b.buildingType.ToString());
                buildingObject.AddElement("Rot", b.rotation.ToString());
                buildingObject.AddElement("Team", b.team.ToString());
            }
        }
    }

    public static List<BuildingElement> Load(JsonDocument doc, out int nextBuildingID)
    {
        nextBuildingID = 1;
        List<BuildingElement> buildings = new List<BuildingElement>();

        var root = doc.GetRoot();
        if (root != null && root.IsJsonObject())
        {
            var rootObject = root.JsonObject();

            var worldObject = rootObject.GetElement("Buildings")?.JsonObject();
            if (worldObject != null)
            {
                nextBuildingID = worldObject.GetElement("NextID")?.Int() ?? 0;

                var buildingArray = worldObject.GetElement("List")?.JsonArray();
                if (buildingArray != null)
                {
                    foreach (var elem in buildingArray)
                    {
                        var buildingObject = elem.JsonObject();
                        if (buildingObject != null)
                        {
                            BuildingElement building = new BuildingElement();

                            building.ID = buildingObject.GetElement("ID")?.Int() ?? 0;
                            building.level = buildingObject.GetElement("Level")?.Int() ?? 0;
                            building.pos = Json.ToVector3Int(buildingObject.GetElement("Pos")?.JsonArray(), Vector3Int.zero);
                            string str = buildingObject.GetElement("Type")?.String();
                            if (str == null)
                                continue;
                            Enum.TryParse(str, out building.buildingType);
                            str = buildingObject.GetElement("Rot")?.String();
                            if (str == null)
                                continue;
                            Enum.TryParse(str, out building.rotation);
                            str = buildingObject.GetElement("Team")?.String();
                            if (str == null)
                                continue;
                            Enum.TryParse(str, out building.team);

                            buildings.Add(building);
                        }
                    }
                }
            }
        }

        return buildings;
    }
}