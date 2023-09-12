using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
