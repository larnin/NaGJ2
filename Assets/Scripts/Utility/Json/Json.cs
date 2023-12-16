using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Json
{
    public static JsonDocument ReadFromFile(string path)
    {
        string data = SaveEx.LoadFile(path);

        return ReadFromString(data);
    }

    public static JsonDocument ReadFromString(string data)
    {
        if (data == null)
            return new JsonDocument();

        var reader = new JsonTextReader(new StringReader(data));

        var doc = new JsonDocument();
        doc.Read(reader);

        return doc;
    }

    public static void WriteToFile(string path, JsonDocument doc)
    {
        var data = WriteToString(doc);

        SaveEx.SaveToFile(path, data);
    }

    public static string WriteToString(JsonDocument doc)
    {
        var builder = new StringBuilder();
        var writer = new StringWriter(builder);
        
        using (var jWriter = new JsonTextWriter(writer))
        {
            jWriter.Formatting = Formatting.Indented;

            doc.Write(jWriter);
        }

        return builder.ToString();
    }

    public static Byte[] LoadBinaryFile(string path)
    {
        try
        {
            if (!File.Exists(path))
                return null;

            return File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to read file " + path);
            Debug.LogError(e.Message);
        }

        return null;
    }

    public static Vector3 ToVector3(JsonArray array, Vector3 def = default(Vector3))
    {
        Vector3 vect = def;
        if(array != null && array.Size() == 3)
        {
            vect.x = array[0].Float();
            vect.y = array[1].Float();
            vect.z = array[2].Float();
        }
        return vect;
    }

    public static JsonArray FromVector3(Vector3 vect)
    {
        var array = new JsonArray();
        array.Add(new JsonNumber(vect.x));
        array.Add(new JsonNumber(vect.y));
        array.Add(new JsonNumber(vect.z));

        return array;
    }

    public static Vector3Int ToVector3Int(JsonArray array, Vector3Int def = default(Vector3Int))
    {
        Vector3Int vect = def;
        if (array != null && array.Size() == 3)
        {
            vect.x = array[0].Int();
            vect.y = array[1].Int();
            vect.z = array[2].Int();
        }
        return vect;
    }

    public static JsonArray FromVector3Int(Vector3Int vect)
    {
        var array = new JsonArray();
        array.Add(new JsonNumber(vect.x));
        array.Add(new JsonNumber(vect.y));
        array.Add(new JsonNumber(vect.z));

        return array;
    }

    public static Vector2 ToVector2(JsonArray array, Vector2 def = default(Vector2))
    {
        Vector2 vect = def;
        if (array != null && array.Size() == 2)
        {
            vect.x = array[0].Float();
            vect.y = array[1].Float();
        }
        return vect;
    }

    public static JsonArray FromVector2(Vector2 vect)
    {
        var array = new JsonArray();
        array.Add(new JsonNumber(vect.x));
        array.Add(new JsonNumber(vect.y));

        return array;
    }

    public static Vector2Int ToVector2Int(JsonArray array, Vector2Int def = default(Vector2Int))
    {
        Vector2Int vect = def;
        if (array != null && array.Size() == 2)
        {
            vect.x = array[0].Int();
            vect.y = array[1].Int();
        }
        return vect;
    }

    public static JsonArray FromVector2Int(Vector2Int vect)
    {
        var array = new JsonArray();
        array.Add(new JsonNumber(vect.x));
        array.Add(new JsonNumber(vect.y));

        return array;
    }
}


public static class JsonTools
{ 
    public static JsonElement ReadNextElement(JsonReader reader, bool needRead = true)
    {
        if (needRead)
            reader.Read();

        do
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return new JsonObject();
                case JsonToken.StartArray:
                    return new JsonArray();
                case JsonToken.String:
                    return new JsonString();
                case JsonToken.Integer:
                case JsonToken.Float:
                    return new JsonNumber();
                default:
                    break;
            }
        } while (reader.Read());

        return null;
    }
}
