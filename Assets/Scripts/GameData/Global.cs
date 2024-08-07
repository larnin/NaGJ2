using System;
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
    [SerializeField] float m_entityAvoidDistance = 1;
    public float entityAvoidDistance { get { return m_entityAvoidDistance; } }

    [SerializeField] AllBlocks m_allBlocks;
    public AllBlocks allBlocks { get { return m_allBlocks; } }

    [SerializeField] AllBuildings m_allBuildings;
    public AllBuildings allBuildings { get { return m_allBuildings; } }

    [SerializeField] AllEntities m_allEntities;
    public AllEntities allEntities { get { return m_allEntities; } }

    [SerializeField] AllResources m_allResources;
    public AllResources allResources { get { return m_allResources; } }

    [SerializeField] LevelListDatas m_levels;
    public LevelListDatas allLevels { get { return m_levels; } }

    [SerializeField] EditorElements m_editor;
    public EditorElements editor { get { return m_editor; } }

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