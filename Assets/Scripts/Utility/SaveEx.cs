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

    public static string LoadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to read " + path + "\n" + e.Message);
            return "";
        }
    }

    public static string LoadAsset(string assetName)
    {
        var resource = Resources.Load(assetName) as TextAsset;
        return LoadAsset(resource);
    }

    public static string LoadAsset(TextAsset asset)
    {
        if (asset == null)
            return "";

        return asset.text;
    }
}

