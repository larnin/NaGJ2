﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Global : ScriptableObject
{
    public float m_entityAvoidDistance = 1;

    static Global m_instance;

    static string s_path = "World";
    static string s_globalName = "Global";

#if UNITY_EDITOR
    [MenuItem("Game/Create Global")]
    public static void MakeGlobal()
    {
        m_instance = Create<Global>(s_globalName) ?? m_instance;

        AssetDatabase.SaveAssets();
    }

    static T Create<T>(string name) where T : ScriptableObject
    {
        var elements = Resources.LoadAll<ScriptableObject>(s_path);
        foreach (var e in elements)
        {
            if (e.name == name)
            {
                Debug.LogError("An item with the name " + name + " already exist in Ressources/" + s_path);
                return null;
            }
        }

        T asset = ScriptableObjectEx.CreateAsset<T>(s_path, name);
        EditorUtility.SetDirty(asset);

        return asset;
    }
#endif

    public static Global instance
    {
        get
        {
            if(m_instance == null)
            {
                Load();
            }
            return m_instance;
        }
    }

    static void Load()
    {
        m_instance = LoadOneInstance<Global>(s_globalName);
    }

    static T LoadOneInstance<T>(string name) where T : ScriptableObject
    {
        T asset = null;

        var elements = Resources.LoadAll<ScriptableObject>(s_path);
        foreach (var e in elements)
        {
            if (e.name == name)
            {
                asset = e as T;
                break;
            }
        }

        if (asset == null)
            Debug.LogError("The " + name + " asset does not exist in the Ressources/" + s_path + " folder");

        return asset;
    }
}