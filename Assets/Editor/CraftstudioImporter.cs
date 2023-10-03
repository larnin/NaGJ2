using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CraftstudioImporter : OdinEditorWindow
{
    string m_path;
    string m_projectName;
    string m_projectDescription;
    List<CraftstudioEntityInfo> m_entities;

    [MenuItem("Game/Craftstudio Importer")]
    public static void OpenWindow()
    {
        GetWindow<CraftstudioImporter>().Show();
    }

    [OnInspectorGUI]
    private void OnInspectorGUI()
    {
        if (m_entities == null)
            OnOpenGUI();
        else OnEntityListGUI();
    }

    void OnOpenGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Path", GUILayout.MaxWidth(40));
        m_path = GUILayout.TextField(m_path);
        if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
            OnOpenBrowse();
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100)))
            OnOpen();
    }

    void OnOpenBrowse()
    {
        string path = EditorUtility.OpenFolderPanel("Open craftstudio project", "%appdata%/CraftStudio/CraftStudioServer/Projects", "");
        if (path != null && path.Length != 0)
            m_path = path;
    }

    void OnOpen()
    {
        string projectPath = m_path + "/Project.dat";

        var data = Json.LoadBinaryFile(projectPath);

        if (data == null)
        {
            Debug.LogError("Unable to load " + projectPath);
            return;
        }

        int index = MoveToEntities(data);
        if(index < 0)
        {
            Debug.LogError("Unable to read content");
            return;
        }

        var nbEntity = BitConverter.ToUInt16(data, index);
        index += 2;
        m_entities = new List<CraftstudioEntityInfo>();
        for(int i = 0; i < nbEntity; i++)
        {
            CraftstudioEntityInfo entity;
            index = ReadOneEntity(data, index, out entity);
            if (entity != null)
                m_entities.Add(entity);
        }
    }

    void OnEntityListGUI()
    {
        GUILayout.Label("Found !");
    }

    int MoveToEntities(byte[] data)
    {
        if (data.Length < 16)
            return -1;

        int index = 0;
        index += 2; //1 Format version (currently 9) 
                    //1 Project type(0 is game, 1 is movie)

        index = ExtractString(data, index, out m_projectName);

        index += 6; //2 Startup scene ID
                    //2 Script API Version
                    //1 Membership policy(see below)
                    //1 Default member role(see below)
        index = ExtractString(data, index, out m_projectDescription);

        index += 2; //2 Next unused game control ID

        var nbControl = BitConverter.ToUInt16(data, index);
        index += 2;
        for (int i = 0; i < nbControl; i++)
            index = SkipControl(data, index);

        index += 2; //2 Next project entry ID

        return index;
    }

    int ExtractString(byte[] data, int startIndex, out string text)
    {
        int size = data[startIndex];
        startIndex++;
        text = Encoding.Default.GetString(data, startIndex, size);
        return startIndex + size;
    }

    int SkipControl(byte[] data, int index)
    {
        index += 2; //2 Game control ID
        string controlName;
        index = ExtractString(data, index, out controlName);
        index += 21;//1	Game control type (see below)
                    //1 Game control source(see below)
                    //1 Positive key
                    //1 Negative key
                    //1 Positive button index
                    //1 Negative button index
                    //1 Joystick index
                    //1 Joystick axis index
                    //4 Joystick axis dead zone
                    //4 Axis sensivity(towards one, per second)
                    //4 Axis gravity(towards zero, per second)
                    //1 Snap to zero when changing direction
        return index;
    }

    int ReadOneEntity(byte[] data, int index, out CraftstudioEntityInfo entity)
    {
        entity = new CraftstudioEntityInfo();

        entity.isFolder = BitConverter.ToBoolean(data, index);
        index++;

        entity.entryID = BitConverter.ToUInt16(data, index);
        index += 2;

        entity.parentID = BitConverter.ToUInt16(data, index);
        index += 2;

        index = ExtractString(data, index, out entity.name);

        entity.type = (CraftstudioEntityType)data[index];
        index++;

        index++; //1 True if locked, false otherwise

        if(!entity.isFolder)
        {
            entity.isTrashed = BitConverter.ToBoolean(data, index);
            index++;

            index += 2; //2 Next revision ID

            var nbRevision = BitConverter.ToUInt16(data, index);
            index += 2;
            for(int i = 0; i < nbRevision; i++)
            {
                CraftstudioEntityRevisionInfo revision = new CraftstudioEntityRevisionInfo();
                revision.revisionID = BitConverter.ToUInt16(data, index);
                index += 2;
                index = ExtractString(data, index, out revision.name);
                entity.revisions.Add(revision);
            }
        }

        return index;
    }
}

public enum CraftstudioEntityType
{
    Model = 0,
    ModelAnimation = 6,
    Map = 1,
    TileSet = 3,
    Scene = 2,
    Script = 7,
    Document = 4,
    Sound = 8,
    Font = 9,
}

class CraftstudioEntityInfo
{
    public bool isFolder;
    public UInt16 entryID;
    public UInt16 parentID;
    public string name;
    public CraftstudioEntityType type;
    public bool isTrashed;
    public List<CraftstudioEntityRevisionInfo> revisions = new List<CraftstudioEntityRevisionInfo>();
}

class CraftstudioEntityRevisionInfo
{
    public UInt16 revisionID;
    public string name;
}
