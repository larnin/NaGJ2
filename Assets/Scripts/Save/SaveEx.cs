using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class SaveEx
{
    public static string GetSaveFilePath(string title, string basePath = "", string filter = "")
    {
#if UNITY_EDITOR
        return EditorUtility.SaveFilePanel(title, basePath, "Level", filter);
#else
        return "";
#endif
    }

    public static string GetLoadFiltPath(string title, string basePath = "", string filter = "")
    {
#if UNITY_EDITOR
        return EditorUtility.OpenFilePanel(title, basePath, filter);
#else
        return "";
#endif
    }

    public static bool SaveToFile(string path, string data)
    {
        try
        {
            File.WriteAllText(path, data);
        }
        catch(Exception e)
        {
            Debug.LogError("Unable to save " + path + "\n" + e.Message);
            return false;
        }
        return true;
    }

    public static bool FileExist(string path)
    {
        return File.Exists(path);
    }

    public static string LoadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to read " + path + "\n" + e.Message);
            return null;
        }
    }

    public static void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public static string LoadAsset(string assetName)
    {
        var resource = Resources.Load(assetName) as TextAsset;
        return LoadAsset(resource);
    }

    public static string LoadAsset(TextAsset asset)
    {
        if (asset == null)
            return null;

        return asset.text;
    }

    public static string GetRelativeAssetPath(string path)
    {
        string originFolder = "Assets";

        int startIndex = 0;

        do
        {
            int index = path.IndexOf(originFolder, startIndex);
            if (index < 0)
                return path;

            char lastChar = '\\';
            char nextChar = '\\';

            if (index > 0)
                lastChar = path[index - 1];
            if (index < path.Length - originFolder.Length - 1)
                nextChar = path[index + originFolder.Length];

            if ((lastChar == '\\' || lastChar == '/') && (nextChar == '/' || nextChar == '\\'))
                return path.Substring(index);

            startIndex = index + 1;

        } while (startIndex >= 0);

        return path;
    }

    public static void SaveLevelFromEditor(string path, JsonDocument doc)
    {
        string data = Json.WriteToString(doc);

        var obj = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(path);
        if (obj == null)
        {
            obj = ScriptableObject.CreateInstance<LevelScriptableObject>();
            obj.data = data;
            AssetDatabase.CreateAsset(obj, path);
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
        else
        {
            obj.data = data;
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }

    public static JsonDocument LoadLevelFromEditor(string path)
    {
        var obj = AssetDatabase.LoadAssetAtPath<LevelScriptableObject>(path);
        if (obj != null)
            return null;

        var json = Json.ReadFromString(obj.data);
        return json;
    }
}

