using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingElement
{
    public int ID;
    public BuildingType buildingType;
    public Vector3Int pos;
    public Rotation rotation;
    public GameObject instance;
    public Team team;
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
                buildingObject.AddElement("ID", new JsonNumber(b.ID));
                buildingObject.AddElement("Pos", Json.FromVector3Int(b.pos));
                buildingObject.AddElement("Type", new JsonString(b.buildingType.ToString()));
                buildingObject.AddElement("Rot", new JsonString(b.rotation.ToString()));
                buildingObject.AddElement("Team", new JsonString(b.team.ToString()));
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