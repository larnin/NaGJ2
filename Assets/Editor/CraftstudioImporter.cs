﻿using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.Formats.Fbx.Exporter;

public class CraftstudioImporter : OdinEditorWindow
{
    string m_path = "C:/Users/ni-la/AppData/Roaming/CraftStudio/CraftStudioServer/Projects/Backup";
    string m_projectName;
    string m_projectDescription;
    Vector2 m_scrollEntity;
    List<CraftstudioEntityInfo> m_entities;
    CraftstudioEntityTree m_modelTree;

    int m_currentIndex;
    CraftstudioModel m_currentModel;
    List<CraftstudioModelAnimation> m_currentModelAnimations = new List<CraftstudioModelAnimation>();
    GameObject m_currentObject;
    MeshFilter m_filter;
    MeshRenderer m_renderer;
    int m_currentTool = 0;
    int m_currentPreview = 0;
    int m_currentTreeTool = 0;
    Mesh m_currentMesh;
    Vector2 m_scrollBlocks;
    Vector2 m_scrollAnimations;

    string m_exportPath;
    bool m_exportSkinned = true;
    bool m_exportAnimations = true;
    bool m_exportTexture = true;

    [MenuItem("Game/Craftstudio Importer")]
    public static void OpenWindow()
    {
        GetWindow<CraftstudioImporter>().Show();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (m_currentObject != null)
            DestroyImmediate(m_currentObject);

        if (gameObjectEditor != null)
            DestroyImmediate(gameObjectEditor);
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
        bool browse = false;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Path", GUILayout.MaxWidth(40));
        m_path = GUILayout.TextField(m_path);
        if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
            browse = true;
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Open", GUILayout.MaxWidth(100)))
            OnOpen();

        if(browse)
            OnOpenBrowse();
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

        m_modelTree = MakeEntityTree(CraftstudioEntityType.Model);
    }

    CraftstudioEntityTree MakeEntityTree(CraftstudioEntityType type)
    {
        CraftstudioEntityTree root = new CraftstudioEntityTree();
        root.folded = true;
        root.entityIndex = -1;

        bool somethingChanged = false;
        int deep = 0;
        bool[] addedOnTree = new bool[m_entities.Count];
        do
        {
            deep++;
            somethingChanged = false;

            for(int i = 0; i < m_entities.Count; i++)
            {
                var entity = m_entities[i];
                if (!entity.isFolder && entity.type != type)
                    continue;
                if (entity.isTrashed)
                    continue;
                if (addedOnTree[i])
                    continue;

                if (AddOnTree(root, i))
                {
                    somethingChanged = true;
                    addedOnTree[i] = true;
                }
            }    

        } while (somethingChanged && deep < 20);

        OptimizeEntityTree(root);
        return root;
    }

    bool AddOnTree(CraftstudioEntityTree tree, int entityIndex)
    {
        var currentEntity = tree.entityIndex >= 0 ? m_entities[tree.entityIndex] : null;
        var newEntity = m_entities[entityIndex];

        bool add = false;
        if (newEntity.parentID == CraftstudioEntityInfo.invalidID && currentEntity == null)
            add = true;
        else if (currentEntity != null && currentEntity.entryID == newEntity.parentID)
            add = true;

        if(add)
        {
            CraftstudioEntityTree child = new CraftstudioEntityTree();
            child.entityIndex = entityIndex;
            tree.childrens.Add(child);
            return true;
        }

        foreach(var child in tree.childrens)
        {
            if (AddOnTree(child, entityIndex))
                return true;
        }

        return false;
    }

    bool OptimizeEntityTree(CraftstudioEntityTree tree) // true if can be optimized
    {
        for(int i = 0; i < tree.childrens.Count; i++)
        {
            if(OptimizeEntityTree(tree.childrens[i]))
            {
                tree.childrens.RemoveAt(i);
                i--;
            }
        }

        if(tree.childrens.Count == 0)
        {
            if (tree.entityIndex >= 0)
            {
                var entity = m_entities[tree.entityIndex];
                if (entity.isFolder)
                    return true;
            }
        }

        return false;
    }

    void OnEntityListGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.MaxWidth(200));
        OnEntityListDrawGUI();
        GUILayout.EndVertical();

        GUIEx.DrawVerticalLine(EditorGUIEx.White, 1);

        GUILayout.BeginVertical();
        if (m_currentModel != null)
            OnDrawCurrentModelGUI();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
    void OnEntityListDrawGUI()
    {
        m_scrollEntity = GUILayout.BeginScrollView(m_scrollEntity);

        OnDrawTreeGUI(m_modelTree, 0);

        GUILayout.EndScrollView();
    }

    void OnDrawTreeGUI(CraftstudioEntityTree tree, int deep)
    {
        if(tree.entityIndex >= 0)
        {
            var entity = m_entities[tree.entityIndex];

            if(deep > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(deep * 10);
            }

            if (entity.isFolder)
            {
                tree.folded = EditorGUILayout.BeginFoldoutHeaderGroup(tree.folded, entity.name);
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            else
            {
                if (GUILayout.Button(entity.name))
                    OnOpenEntity(tree.entityIndex);
            }


            if (deep > 0)
                GUILayout.EndHorizontal();
        }

        if (tree.folded)
        {
            foreach (var child in tree.childrens)
                OnDrawTreeGUI(child, deep + 1);
        }
    }

    void OnDrawCurrentModelGUI()
    {
        if (m_currentModel == null)
            return;

        GUILayout.Label(GetEntityPath(m_currentIndex));

        m_currentTool = GUILayout.Toolbar(m_currentTool, new string[] { "Model", "Export" });

        GUIEx.DrawHorizontalLine(EditorGUIEx.White, 1);

        if (m_currentTool == 0)
            OnDrawCurrentModelDisplayGUI();
        else if (m_currentTool == 1)
            OnDrawCurrentModelExportGUI();
    }

    void OnDrawCurrentModelDisplayGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        OnDrawCurrentModelPreviewGUI();
        GUILayout.EndVertical();
        GUIEx.DrawVerticalLine(EditorGUIEx.White, 1);
        GUILayout.BeginVertical(GUILayout.MaxWidth(200));
        m_currentTreeTool = GUILayout.Toolbar(m_currentTreeTool, new string[] { "Submesh", "Bones" });
        if(m_currentTreeTool == 0)
            OnDrawSubmeshGUI();
        GUILayout.Label("Blocks");
        OnDrawBlockListGUI();
        GUIEx.DrawHorizontalLine(EditorGUIEx.White, 1);
        GUILayout.Label("Animations");
        OnDrawAnimationListGUI();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void OnDrawCurrentModelPreviewGUI()
    {
        int oldPreview = m_currentPreview;

        m_currentPreview = GUILayout.Toolbar(m_currentPreview, new string[] { "Textured", "Submesh", "Texture" });
        if (m_currentPreview == 0 || m_currentPreview == 1)
            OnDrawCurrentModelPreviewTexturedGUI();
        else if (m_currentPreview == 2)
            OnDrawCurrentModelPreviewTextureGUI();

        if (oldPreview != m_currentPreview)
        {
            m_currentMesh = MakeMesh(m_currentMesh, m_currentModel, m_currentPreview == 0);
            SetObjectMeshAndMaterials();
        }
    }

    Editor gameObjectEditor;

    void OnDrawCurrentModelPreviewTexturedGUI()
    {
        if (gameObjectEditor == null)
            gameObjectEditor = Editor.CreateEditor(m_currentObject);

        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        var rect = GUILayoutUtility.GetLastRect();

        GUIStyle bgColor = new GUIStyle();
        gameObjectEditor.OnInteractivePreviewGUI(rect, bgColor);
    }

    void OnDrawCurrentModelPreviewSubmeshGUI()
    {

    }

    void OnDrawCurrentModelPreviewTextureGUI()
    {
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), m_currentModel.texture, ScaleMode.ScaleToFit);
    }

    void OnDrawSubmeshGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Submesh", GUILayout.MaxWidth(100));
        m_currentModel.submeshNb = EditorGUILayout.IntField(m_currentModel.submeshNb);
        if (m_currentModel.submeshNb < 1)
            m_currentModel.submeshNb = 1;
        if (m_currentModel.submeshNb > 64)
            m_currentModel.submeshNb = 64;
        GUILayout.EndHorizontal();
    }

    void OnDrawBlockListGUI()
    {
        m_scrollBlocks = GUILayout.BeginScrollView(m_scrollBlocks, GUILayout.MaxWidth(200));

        OnDrawBlockTreeGUI(m_currentModel, m_currentModel.tree, 0);

        GUILayout.EndScrollView();
    }

    void OnDrawBlockTreeGUI(CraftstudioModel model, CraftstudioModelNodeTree tree, int deep)
    {
        if (tree.nodeIndex >= 0)
        {
            var node = model.nodes[tree.nodeIndex];

            GUILayout.BeginHorizontal();
            
            if (deep > 0)
                GUILayout.Space(deep * 10);

            var oldColor = GUI.backgroundColor;
            
            if(m_currentTreeTool == 0)
                GUI.backgroundColor = SubmeshToColor(node.submeshIndex);
            if (tree.childrens.Count > 0)
            {
                tree.folded = EditorGUILayout.BeginFoldoutHeaderGroup(tree.folded, node.name);
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            else
            {
                GUILayout.Button(node.name);
            }

            if (m_currentTreeTool == 0)
            {
                int oldIndex = node.submeshIndex;

                node.submeshIndex = EditorGUILayout.IntField(node.submeshIndex, GUILayout.MaxWidth(20));
                if (node.submeshIndex < 0)
                    node.submeshIndex = 0;
                if (node.submeshIndex >= model.submeshNb)
                    node.submeshIndex = model.submeshNb - 1;

                if (oldIndex != node.submeshIndex)
                {
                    m_currentMesh = MakeMesh(m_currentMesh, m_currentModel, m_currentPreview == 0);
                    SetObjectMeshAndMaterials();
                }
            }
            else if(m_currentTreeTool == 1)
                node.bone = GUILayout.Toggle(node.bone, "", GUILayout.MaxWidth(20));

            GUI.backgroundColor = oldColor;

            GUILayout.EndHorizontal();
        }

        if (tree.folded)
        {
            foreach (var child in tree.childrens)
                OnDrawBlockTreeGUI(model, child, deep + 1);
        }
    }

    void OnDrawAnimationListGUI()
    {
        m_scrollAnimations = GUILayout.BeginScrollView(m_scrollAnimations, GUILayout.MaxHeight(200), GUILayout.MaxWidth(200));

        for(int i = 0; i < m_currentModelAnimations.Count; i++)
        {
            var entity = m_entities[m_currentModelAnimations[i].index];
            GUILayout.BeginHorizontal();
            GUILayout.Label(entity.name);

            m_currentModelAnimations[i].export = GUILayout.Toggle(m_currentModelAnimations[i].export, "", GUILayout.MaxWidth(20)); ;

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    void OnDrawCurrentModelExportGUI()
    {
        bool browse = false;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Path", GUILayout.MaxWidth(40));
        m_exportPath = GUILayout.TextField(m_exportPath);
        if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
            browse = true;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        m_exportSkinned = GUILayout.Toggle(m_exportSkinned, "Export Skinned");
        if (m_exportSkinned)
            m_exportAnimations = GUILayout.Toggle(m_exportAnimations, "Export Animations");
        m_exportTexture = GUILayout.Toggle(m_exportTexture, "Export Texture");
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Export", GUILayout.MaxWidth(120)))
            Export();

        if (browse)
            OnOpenBrowseExport();
    }

    void OnOpenBrowseExport()
    {
        if (m_currentIndex < 0)
            return;

        string fileName = m_entities[m_currentIndex].name;

        string path = EditorUtility.SaveFolderPanel("Save " + fileName, "Assets", "Export");
        if (path != null && path.Length != 0)
            m_exportPath = path;
    }

    struct ExportNodeData
    {
        public GameObject node;
        public int index;
    }

    void Export()
    {
        if (m_currentIndex < 0)
            return;

        if (m_exportPath == null || m_exportPath.Length <= 0)
            return;

        string assetsPath = (string)m_exportPath.Clone();
        int startPath = m_exportPath.IndexOf("Assets");
        if (startPath >= 0)
            assetsPath = m_exportPath.Substring(startPath);

        if (!AssetDatabase.IsValidFolder(assetsPath))
            return;

        string fileName = m_entities[m_currentIndex].name;

        AssetDatabase.CreateFolder(assetsPath, fileName);

        GameObject exportObj = new GameObject(fileName);

        Texture2D texture = null;
        if(m_exportTexture)
        {
            string texturePath = assetsPath + "/" + fileName + "/" + fileName + "_Tex.png";
            ExportTexture(texturePath);

            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            texture.filterMode = FilterMode.Point;

            EditorUtility.SetDirty(texture);
            AssetDatabase.SaveAssetIfDirty(texture);
        }

        Mesh mesh = null;
        if (m_exportSkinned)
        {
            mesh = ExportSkined(exportObj, texture);

            if (m_exportAnimations && (m_exportPath != null || m_exportPath.Length > 0))
            {
                string animPath = assetsPath + "/" + fileName;

                for (int i = 0; i < m_currentModelAnimations.Count; i++)
                {
                    var anim = ExportAnimation(m_currentModel, m_currentModelAnimations[i]);

                    ExportAnimation(anim, animPath);
                }
            }
        }
        else mesh = ExportSimple(exportObj, texture);

        if(mesh != null)
        {
            string meshPath = assetsPath + "/" + fileName + "/" + fileName + "_Mesh.asset";

            AssetDatabase.CreateAsset(mesh, meshPath);
        }

        Material[] materials = null;
        {
            var skinned = exportObj.GetComponent<SkinnedMeshRenderer>();
            if (skinned != null)
                materials = skinned.sharedMaterials;
            else
            {
                var renderer = exportObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                    materials = renderer.sharedMaterials;
            }
        }

        if(materials != null)
        {
            for(int i = 0; i < materials.Length; i++)
            {
                string matPath = assetsPath + "/" + fileName + "/" + fileName;
                if (materials.Length > 1)
                    matPath += "_" + i.ToString();
                matPath += "_Mat.asset";
                AssetDatabase.CreateAsset(materials[i], matPath);
            }
        }

        string realPath = assetsPath + "/" + fileName + "/" + fileName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(exportObj, realPath);

        GameObject.DestroyImmediate(exportObj);
    }

    Mesh ExportSkined(GameObject exportObj, Texture2D texture)
    {
        var skinnedRenderer = exportObj.AddComponent<SkinnedMeshRenderer>();

        List<ExportNodeData> nodes = AddExportNode(exportObj, m_currentModel.tree);
        if (nodes.Count > 0)
            skinnedRenderer.rootBone = nodes[0].node.transform;

        Transform[] bones = new Transform[nodes.Count];
        Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];
        for (int i = 0; i < nodes.Count; i++)
        {
            bones[i] = nodes[i].node.transform;
            bindPoses[i] = bones[i].worldToLocalMatrix * bones[0].localToWorldMatrix;
        }
        skinnedRenderer.bones = bones;

        var meshData = MakeMeshDatas(m_currentModel, true);
        var meshWithBones = Clone(meshData);

        //add bones weights
        for (int i = 0; i < m_currentModel.nodes.Count; i++)
        {
            int nodeIndex = -1;
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].index == i)
                    nodeIndex = j;
            }

            for (int j = 0; j < 24; j++)
            {
                int verticeIndex = i * 24 + j;
                if (verticeIndex < meshWithBones.verticesSize)
                {
                    meshWithBones.vertices[verticeIndex].boneIndex = nodeIndex;
                    meshWithBones.vertices[verticeIndex].boneWeight = 1;
                }
            }
        }

        Mesh mesh = MakeMeshExport(meshWithBones);
        mesh.bindposes = bindPoses;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        skinnedRenderer.sharedMesh = mesh;
        skinnedRenderer.localBounds = mesh.bounds;

        Material[] mats = new Material[m_currentModel.submeshNb];
        for (int i = 0; i < mats.Length; i++)
            mats[i] = GetExportMaterial(texture);
        skinnedRenderer.materials = mats;

        return mesh;
    }

    void ExportAnimation(AnimationClip clip, string path)
    {
        string savePath = path + "/" + clip.name + "_Anim.asset";

        AssetDatabase.CreateAsset(clip, savePath);
    }

    Mesh ExportSimple(GameObject exportObj, Texture2D texture)
    {
        var meshFilter = exportObj.AddComponent<MeshFilter>();
        var meshRenderer = exportObj.AddComponent<MeshRenderer>();

        var mesh = MakeMesh(null, m_currentModel, true);
        meshFilter.mesh = mesh;

        Material[] mats = new Material[m_currentModel.submeshNb];
        for (int i = 0; i < mats.Length; i++)
            mats[i] = GetExportMaterial(texture);
        meshRenderer.materials = mats;

        return mesh;
    }

    void ExportTexture(string path)
    {
        if(path == null || path.Length == 0)

        if (m_currentModel == null)
            return;

        var texture = m_currentModel.texture;
        if (texture == null)
            return;

        var bytes = ImageConversion.EncodeToPNG(m_currentModel.texture as Texture2D);

        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + path);
        UnityEditor.AssetDatabase.Refresh();
    }

    AnimationClip ExportAnimation(CraftstudioModel model, CraftstudioModelAnimation anim)
    {
        var clip = new AnimationClip();

        var entity = m_entities[anim.index];
        clip.name = entity.name;
        clip.frameRate = 30;
        float duration = anim.duration / 30.0f;

        if (anim.holdLastKey)
            clip.wrapMode = WrapMode.ClampForever;
        else clip.wrapMode = WrapMode.Loop;

        foreach(var animNode in anim.nodes)
        {
            List<int> nodeIndexs = new List<int>();

            for (int i = 0 ; i < model.nodes.Count; i++)
            {
                if (animNode.name == model.nodes[i].name)
                    nodeIndexs.Add(i);
            }

            foreach(var nodeIndex in nodeIndexs)
            {
                var node = model.nodes[nodeIndex];

                AnimationCurve posX = new AnimationCurve();
                AnimationCurve posY = new AnimationCurve();
                AnimationCurve posZ = new AnimationCurve();

                AnimationCurve rotZ = new AnimationCurve();
                AnimationCurve rotX = new AnimationCurve();
                AnimationCurve rotY = new AnimationCurve();

                AnimationCurve scaleX = new AnimationCurve();
                AnimationCurve scaleY = new AnimationCurve();
                AnimationCurve scaleZ = new AnimationCurve();

                for(UInt16 i = 0; i <= anim.duration; i++)
                {
                    CraftstudioModelAnimationNodeVector3 posNode = null;
                    CraftstudioModelAnimationNodeVector3 offsetNode = null;
                    CraftstudioModelAnimationNodeVector3 scaleNode = null;
                    CraftstudioModelAnimationNodeQuaternion rotNode = null;

                    float time = i / 30.0f;

                    foreach (var p in animNode.pos)
                    {
                        if (p.keyTime == i)
                        {
                            posNode = p;
                            break;
                        }
                    }

                    foreach (var p in animNode.offset)
                    {
                        if (p.keyTime == i)
                        {
                            offsetNode = p;
                            break;
                        }
                    }

                    foreach (var p in animNode.scale)
                    {
                        if (p.keyTime == i)
                        {
                            scaleNode = p;
                            break;
                        }
                    }

                    foreach (var p in animNode.rot)
                    {
                        if (p.keyTime == i)
                        {
                            rotNode = p;
                            break;
                        }
                    }

                    if(posNode != null || offsetNode != null)
                    {
                        Vector3 pos = Vector3.zero;
                        if (posNode != null)
                            pos += posNode.value;
                        if (offsetNode != null)
                            pos += offsetNode.value;

                        pos += node.pos + node.offset;

                        posX.AddKey(new Keyframe(time, pos.x, 0, 0, 0, 0));
                        posY.AddKey(new Keyframe(time, pos.y, 0, 0, 0, 0));
                        posZ.AddKey(new Keyframe(time, pos.z, 0, 0, 0, 0));
                    }

                    if(scaleNode != null)
                    {
                        scaleX.AddKey(new Keyframe(time, scaleNode.value.x * node.scale.x, 0, 0, 0, 0));
                        scaleY.AddKey(new Keyframe(time, scaleNode.value.y * node.scale.y, 0, 0, 0, 0));
                        scaleZ.AddKey(new Keyframe(time, scaleNode.value.z * node.scale.z, 0, 0, 0, 0));
                    }

                    if(rotNode != null)
                    {
                        Vector3 rot = (Quaternion.Inverse(node.rot) * rotNode.value).eulerAngles;

                        rotX.AddKey(new Keyframe(time, rot.x, 0, 0, 0, 0));
                        rotY.AddKey(new Keyframe(time, rot.y, 0, 0, 0, 0));
                        rotZ.AddKey(new Keyframe(time, rot.z, 0, 0, 0, 0));
                    }
                }

                SetCurveMode(posX, anim.holdLastKey, duration);
                SetCurveMode(posY, anim.holdLastKey, duration);
                SetCurveMode(posZ, anim.holdLastKey, duration);
                SetCurveMode(scaleX, anim.holdLastKey, duration);
                SetCurveMode(scaleY, anim.holdLastKey, duration);
                SetCurveMode(scaleZ, anim.holdLastKey, duration);
                SetCurveMode(rotX, anim.holdLastKey, duration);
                SetCurveMode(rotY, anim.holdLastKey, duration);
                SetCurveMode(rotZ, anim.holdLastKey, duration);

                NormalizeRotation(rotX);
                NormalizeRotation(rotY);
                NormalizeRotation(rotZ);

                var path = GetNodeFullPath(model, nodeIndex);
                if (posX.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localPosition.x", posX);
                if (posY.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localPosition.y", posY);
                if (posZ.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localPosition.z", posZ);

                if (rotX.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localEulerAnglesRaw.x", rotX);
                if (rotY.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localEulerAnglesRaw.y", rotY);
                if (rotZ.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localEulerAnglesRaw.z", rotZ);

                if (scaleX.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localScale.x", scaleX);
                if (scaleY.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localScale.y", scaleY);
                if (scaleZ.length > 0)
                    clip.SetCurve(path, typeof(Transform), "localScale.z", scaleZ);
            }
        }

        return clip;
    }

    void SetCurveMode(AnimationCurve curve, bool holdLastKey, float duration)
    {
        if(curve.length > 0)
        {
            var lastKey = curve.keys[curve.length - 1];
            var firstKey = curve.keys[0];

            if (holdLastKey)
                curve.AddKey(new Keyframe(duration, lastKey.value, 0, 0, 0, 0));
            else
            {
                if (Mathf.Abs(firstKey.time) < 0.001f)
                {
                    if (Mathf.Abs(lastKey.time - duration) > 0.001f)
                        curve.AddKey(new Keyframe(duration, firstKey.value, 0, 0, 0, 0));
                }
                else if (Mathf.Abs(lastKey.time - duration) < 0.001f)
                    curve.AddKey(new Keyframe(0, lastKey.value, 0, 0, 0, 0));
                else
                {
                    float firstDistance = firstKey.time;
                    float lastDistance = duration - lastKey.time;

                    float totalDistance = firstDistance + lastDistance;

                    float newValue = (firstKey.value * lastDistance + lastKey.value * firstDistance) / totalDistance;

                    curve.AddKey(new Keyframe(0, newValue, 0, 0, 0, 0));
                    curve.AddKey(new Keyframe(duration, newValue, 0, 0, 0, 0));
                }    
            }
        }        
        
        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyBroken(curve, i, true);
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
    }

    void NormalizeRotation(AnimationCurve curve)
    {
        if (curve.length == 0)
            return;

        float lastValue = curve.keys[0].value;

        for(int i = 1; i < curve.length; i++)
        {
            float current = curve.keys[i].value;

            while (current - lastValue < -180)
                current += 360;
            while (current - lastValue > 180)
                current -= 360;

            lastValue = current;

            curve.MoveKey(i, new Keyframe(curve.keys[i].time, current, 0, 0, 0, 0));

            AnimationUtility.SetKeyBroken(curve, i, true);
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
    }

    string GetNodeFullPath(CraftstudioModel model, int index)
    {
        string path = "";

        var node = model.nodes[index];
        path += node.name;
        while(node.parentID != CraftstudioEntityInfo.invalidID)
        {
            CraftstudioModelNode newNode = null;
            foreach(var n in model.nodes)
            {
                if(n.ID == node.parentID)
                {
                    newNode = n;
                    break;
                }
            }

            if (newNode == null)
                break;

            path = newNode.name + '/' + path;
            node = newNode;
        }

        return "Root/" + path;
    }

    List<ExportNodeData> AddExportNode(GameObject parent, CraftstudioModelNodeTree tree)
    {
        GameObject obj = null;

        if (tree.nodeIndex < 0)
        {
            obj = new GameObject("Root");
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
        else
        {
            var node = m_currentModel.nodes[tree.nodeIndex];

            obj = new GameObject(node.name);
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = new Vector3(-node.pos.x, node.pos.y, node.pos.z);
            obj.transform.rotation = node.rot;
        }

        List<ExportNodeData> nodeDatas = new List<ExportNodeData>();
        var nodeData = new ExportNodeData();
        nodeData.node = obj;
        nodeData.index = tree.nodeIndex;
        nodeDatas.Add(nodeData);

        foreach (var t in tree.childrens)
        {
            var newNodes = AddExportNode(obj, t);
            foreach (var n in newNodes)
                nodeDatas.Add(n);
        }

        return nodeDatas;
    }

    Mesh MakeMeshExport(MeshParamData<NormalUVBoneVertexDefinition> data)
    {
        Mesh mesh = new Mesh();

        MeshEx.SetNormalUVBoneMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        Bounds b = GetBounds(data, data.verticesSize);

        mesh.bounds = b;

        return mesh;
    }

    Material GetExportMaterial(Texture2D texture)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.SetTexture("_MainTex", texture);
        //mat.SetOverrideTag("RenderType", "TransparentCutout");
        //mat.EnableKeyword("_ALPHATEST_ON");
        //mat.renderQueue = 2449;
        return mat;
    }

    MeshParamData<NormalUVBoneVertexDefinition> Clone(MeshParamData<NormalUVColorVertexDefinition> oldDatas)
    {
        SimpleMeshParam<NormalUVBoneVertexDefinition> meshData = new SimpleMeshParam<NormalUVBoneVertexDefinition>();
        var data = meshData.Allocate(oldDatas.verticesSize, oldDatas.indexesSize);

        for(int i = 0; i < oldDatas.verticesSize; i++)
        {
            data.vertices[i].pos = oldDatas.vertices[i].pos;
            data.vertices[i].normal = oldDatas.vertices[i].normal;
            data.vertices[i].uv = oldDatas.vertices[i].uv;
        }

        for (int i = 0; i < oldDatas.indexesSize; i++)
            data.indexes[i] = oldDatas.indexes[i];

        data.verticesSize = oldDatas.verticesSize;
        data.indexesSize = oldDatas.indexesSize;

        return data;
    }

    string GetEntityPath(int index)
    {
        var entity = m_entities[index];
        string path = entity.name;
        while(entity.parentID != CraftstudioEntityInfo.invalidID)
        {
            int nextIndex = GetEntityIndexFromID(entity.parentID);
            if (nextIndex <= 0)
                break;

            entity = m_entities[nextIndex];
            path = entity.name + "/" + path;
        }

        return path;
    }

    void OnOpenEntity(int entityIndex)
    {
        if(m_currentMesh != null)
            m_currentMesh.Clear();
        
        LoadModel(entityIndex);
        
        if(m_currentModel != null)
            m_currentMesh = MakeMesh(m_currentMesh, m_currentModel, m_currentPreview == 0);

        MakeCurrentGameobject();
        SetObjectMeshAndMaterials();
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

    int GetEntityIndexFromID(UInt16 ID)
    {
        for(int i = 0; i < m_entities.Count; i++)
        {
            if (m_entities[i].entryID == ID)
                return i;
        }

        return -1;
    }

    void LoadModel(int modelIndex)
    {
        var entity = m_entities[modelIndex];

        string path = m_path + "/Assets/Models/" + entity.entryID + "/Current.csmodel";
        m_currentIndex = modelIndex;
        m_currentModel = LoadModel(path);

        m_currentModelAnimations.Clear();

        if (m_currentModel != null)
        {
            foreach(var anim in m_currentModel.animations)
            {
                int index = GetEntityIndexFromID(anim);
                if (index < 0)
                    continue;

                string pathAnim = m_path + "/Assets/ModelAnimations/" + anim + "/Current.csmodelanim";
                var animation = LoadAnimation(pathAnim);

                if (animation != null)
                {
                    animation.index = index;
                    animation.export = true;
                    m_currentModelAnimations.Add(animation);
                }
            }
        }

    }

    CraftstudioModel LoadModel(string path)
    {
        var data = Json.LoadBinaryFile(path);
        if(data == null)
        {
            Debug.LogError("Unable to load model " + path);
            return null;
        }

        int index = 0;

        CraftstudioModel model = new CraftstudioModel();

        index += 5; //1 Asset type (always 0 for Models)
                    //2 Format version(currently 5)
                    //2 Next unused node ID

        int nbNodes = BitConverter.ToUInt16(data, index);
        index += 2;
        for(int i = 0; i < nbNodes; i++)
        {
            CraftstudioModelNode node;
            index = ReadNode(data, index, out node);
            if (node != null)
                model.nodes.Add(node);
        }

        index = ReadTexture(data, index, out model.texture);

        int nbAnim = BitConverter.ToUInt16(data, index);
        index += 2;

        for(int i = 0; i < nbAnim; i++)
        {
            model.animations.Add(BitConverter.ToUInt16(data, index));
            index += 2;
        }

        MakeNodeTree(model);

        return model;
    }

    int ReadNode(byte[] data, int index, out CraftstudioModelNode node)
    {
        node = new CraftstudioModelNode();

        node.ID = BitConverter.ToUInt16(data, index);
        index += 2;

        node.parentID = BitConverter.ToUInt16(data, index);
        index += 2;

        index = ExtractString(data, index, out node.name);

        for(int i = 0; i < 3; i++)
        {
            node.pos[i] = BitConverter.ToSingle(data, index);
            index += 4;
        }

        for (int i = 0; i < 3; i++)
        {
            node.offset[i] = BitConverter.ToSingle(data, index);
            index += 4;
        }

        for (int i = 0; i < 3; i++)
        {
            node.scale[i] = BitConverter.ToSingle(data, index);
            index += 4;
        }

        index = ReadQuaternion(data, index, out node.rot);

        for (int i = 0; i < 3; i++)
        {
            node.size[i] = BitConverter.ToInt16(data, index);
            index += 2;
        }

        node.unwrapMode = (CraftstudioModelCubeUnwrap)data[index];
        index++;

        for(int i = 0; i < 6; i++)
        {
            node.unwrapOffsetQuads[i].x = BitConverter.ToInt32(data, index);
            index += 4;
            node.unwrapOffsetQuads[i].y = BitConverter.ToInt32(data, index);
            index += 4;
        }

        for(int i = 0; i < 6; i++)
        {
            node.unwrapFlags[i] = data[index];
            index++;
        }

        return index;
    }

    int ReadQuaternion(byte[] data, int index, out Quaternion quaternion)
    {
        quaternion.w = BitConverter.ToSingle(data, index);
        index += 4;
        quaternion.x = BitConverter.ToSingle(data, index);
        index += 4;
        quaternion.y = BitConverter.ToSingle(data, index);
        index += 4;
        quaternion.z = BitConverter.ToSingle(data, index);
        index += 4;

        return index;
    }

    int ReadTexture(byte[] data, int index, out Texture texture)
    {
        int size = BitConverter.ToInt32(data, index);
        index += 4;

        byte[] textureData = new byte[size];
        Array.Copy(data, index, textureData, 0, size);

        Texture2D newTexture = new Texture2D(2, 2);
        if (!ImageConversion.LoadImage(newTexture, textureData))
            Debug.LogError("Error when loading texture");

        texture = newTexture;
        texture.filterMode = FilterMode.Point;

        return index + size;
    }

    void MakeNodeTree(CraftstudioModel model)
    {
        model.tree = new CraftstudioModelNodeTree();
        model.tree.nodeIndex = -1;

        bool somethingChanged = false;
        int deep = 0;
        bool[] addedOnTree = new bool[model.nodes.Count];
        do
        {
            deep++;
            somethingChanged = false;

            for (int i = 0; i < model.nodes.Count; i++)
            {
                var node = model.nodes[i];
                if (addedOnTree[i])
                    continue;

                if (AddOnNodeTree(model, model.tree, i))
                {
                    somethingChanged = true;
                    addedOnTree[i] = true;
                }
            }

        } while (somethingChanged && deep < 20);
    }

    bool AddOnNodeTree(CraftstudioModel model, CraftstudioModelNodeTree tree, int nodeIndex)
    {
        var currentnode = tree.nodeIndex >= 0 ? model.nodes[tree.nodeIndex] : null;
        var newNode = model.nodes[nodeIndex];

        bool add = false;
        if (newNode.parentID == CraftstudioEntityInfo.invalidID && currentnode == null)
            add = true;
        else if (currentnode != null && currentnode.ID == newNode.parentID)
            add = true;

        if (add)
        {
            CraftstudioModelNodeTree child = new CraftstudioModelNodeTree();
            child.nodeIndex = nodeIndex;
            tree.childrens.Add(child);
            return true;
        }

        foreach (var child in tree.childrens)
        {
            if (AddOnNodeTree(model, child, nodeIndex))
                return true;
        }

        return false;
    }

    CraftstudioModelAnimation LoadAnimation(string path)
    {
        var data = Json.LoadBinaryFile(path);
        if (data == null)
        {
            Debug.LogError("Unable to load model animation " + path);
            return null;
        }

        CraftstudioModelAnimation animation = new CraftstudioModelAnimation();

        int index = 0;

        index += 3; //1 Asset type(always 6 for Model Animations)
                    //2 Format version(currently 3)

        animation.duration = BitConverter.ToUInt16(data, index);
        index += 2;

        animation.holdLastKey = BitConverter.ToBoolean(data, index);
        index++;

        int nbNode = BitConverter.ToInt16(data, index);
        index += 2;
        for(int i = 0; i < nbNode; i++)
        {
            CraftstudioModelAnimationNode node;
            index = ReadAnimationNode(data, index, out node);
            if (node != null)
                animation.nodes.Add(node);
        }

        return animation;
    }

    int ReadAnimationNode(byte[] data, int index, out CraftstudioModelAnimationNode node)
    {
        node = new CraftstudioModelAnimationNode();

        index = ExtractString(data, index, out node.name);

        int nbKeyPos = BitConverter.ToUInt16(data, index);
        index += 2;
        for(int i = 0; i < nbKeyPos; i++)
        {
            CraftstudioModelAnimationNodeVector3 nodePos;
            index = ReadAnimationNodeVector3(data, index, out nodePos);
            if (nodePos != null)
                node.pos.Add(nodePos);
        }

        int nbKeyRot = BitConverter.ToUInt16(data, index);
        index += 2;
        for(int i = 0; i < nbKeyRot; i++)
        {
            CraftstudioModelAnimationNodeQuaternion nodeRot;
            index = ReadAnimationNodeQuaternion(data, index, out nodeRot);
            if (nodeRot != null)
                node.rot.Add(nodeRot);
        }

        int nbKeySize = BitConverter.ToUInt16(data, index);
        index += 2;
        for(int i = 0; i < nbKeySize; i++)
        {
            CraftstudioModelAnimationNodeVector3Int nodeSize;
            index = ReadAnimationNodeVector3Int(data, index, out nodeSize);
            if (nodeSize != null)
                node.size.Add(nodeSize);
        }

        int nbKeyPivot = BitConverter.ToUInt16(data, index);
        index += 2;
        for(int i = 0; i < nbKeyPivot; i++)
        {
            CraftstudioModelAnimationNodeVector3 nodePivot;
            index = ReadAnimationNodeVector3(data, index, out nodePivot);
            if (nodePivot != null)
                node.offset.Add(nodePivot);
        }

        int nbKeyScale = BitConverter.ToUInt16(data, index);
        index += 2;
        for (int i = 0; i < nbKeyScale; i++)
        {
            CraftstudioModelAnimationNodeVector3 nodeScale;
            index = ReadAnimationNodeVector3(data, index, out nodeScale);
            if (nodeScale != null)
                node.offset.Add(nodeScale);
        }

        return index;
    }

    int ReadAnimationNodeVector3(byte[] data, int index, out CraftstudioModelAnimationNodeVector3 node)
    {
        node = new CraftstudioModelAnimationNodeVector3();

        node.keyTime = BitConverter.ToUInt16(data, index);
        index += 2;

        index++; //1 Interpolation mode (currently ignored)

        for (int i = 0; i < 3; i++)
        {
            node.value[i] = BitConverter.ToSingle(data, index);
            index += 4;
        }

        return index;
    }

    int ReadAnimationNodeVector3Int(byte[] data, int index, out CraftstudioModelAnimationNodeVector3Int node)
    {
        node = new CraftstudioModelAnimationNodeVector3Int();

        node.keyTime = BitConverter.ToUInt16(data, index);
        index += 2;

        index++; //1 Interpolation mode (currently ignored)

        for (int i = 0; i < 3; i++)
        {
            node.value[i] = BitConverter.ToInt32(data, index);
            index += 4;
        }

        return index;
    }

    int ReadAnimationNodeQuaternion(byte[] data, int index, out CraftstudioModelAnimationNodeQuaternion node)
    {
        node = new CraftstudioModelAnimationNodeQuaternion();

        node.keyTime = BitConverter.ToUInt16(data, index);
        index += 2;

        index++; //1 Interpolation mode (currently ignored)

        index = ReadQuaternion(data, index, out node.value);

        return index;
    }

    Color SubmeshToColor(int index)
    {
        Color[] colors = new Color[]
        {
            GUI.backgroundColor,

            new Color(1, 0, 0),
            new Color(1, 1, 0),
            new Color(0, 1, 0),
            new Color(0, 1, 1),
            new Color(0, 0, 1),
            new Color(1, 0, 1),

            new Color(1, 0.5f, 0),
            new Color(0.5f, 1, 0),
            new Color(0, 1, 0.5f),
            new Color(0, 0.5f, 1),
            new Color(0.5f, 0, 1),
            new Color(1, 0, 0.5f),

            new Color(1, 0.5f, 0.5f),
            new Color(1, 1, 0.5f),
            new Color(0.5f, 1, 0.5f),
            new Color(0.5f, 1, 1),
            new Color(0.5f, 0.5f, 1),
            new Color(1, 0.5f, 1),
        };

        if (index < 0 || index >= colors.Length)
            return colors[0];
        return colors[index];
    }

    int GetNodeIndexFromID(CraftstudioModel model, UInt16 ID)
    {
        for (int i = 0; i < model.nodes.Count; i++)
        {
            if (model.nodes[i].ID == ID)
                return i;
        }

        return -1;
    }

    MeshParamData<NormalUVColorVertexDefinition> MakeMeshDatas(CraftstudioModel model, bool textured)
    {
        SimpleMeshParam<NormalUVColorVertexDefinition> meshData = new SimpleMeshParam<NormalUVColorVertexDefinition>();
        var data = meshData.Allocate(model.nodes.Count * 24, model.nodes.Count * 36);

        for (int i = 0; i < model.nodes.Count; i++)
        {
            GenerateVertices(data, model, i, textured);
        }

        data.indexesSize = model.nodes.Count * 36;
        data.verticesSize = model.nodes.Count * 24;

        for (int i = 0; i < data.verticesSize; i++)
        {
            var pos = data.vertices[i].pos;
            pos.x *= -1;
            data.vertices[i].pos = pos;
        }

        return data;
    }

    Mesh MakeMesh(Mesh mesh, CraftstudioModel model, bool textured)
    {
        var data = MakeMeshDatas(model, textured);

        if (mesh == null)
            mesh = new Mesh();

        MeshEx.SetNormalUVColorMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        Bounds b = GetBounds(data, data.verticesSize);

        mesh.bounds = b;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    Bounds GetBounds(MeshParamData<NormalUVBoneVertexDefinition> data, int vertexNb)
    {
        if (vertexNb == 0)
            return new Bounds();

        Bounds b = new Bounds(data.vertices[0].pos, Vector3.zero);

        for (int i = 1; i < vertexNb; i++)
            b.Encapsulate(data.vertices[i].pos);

        return b;
    }

    Bounds GetBounds(MeshParamData<NormalUVColorVertexDefinition> data, int vertexNb)
    {
        if (vertexNb == 0)
            return new Bounds();

        Bounds b = new Bounds(data.vertices[0].pos, Vector3.zero);

        for(int i = 1; i < vertexNb; i++)
            b.Encapsulate(data.vertices[i].pos);

        return b;
    }

    void GenerateVertices(MeshParamData<NormalUVColorVertexDefinition> data, CraftstudioModel model, int index, bool textured)
    {
        int v = index * 24;
        int i = index * 36;

        GenerateBaseCube(data, i, v);

        var node = model.nodes[index];
        if (textured)
            ApplyUVToCube(data, v, node, new Vector2Int(model.texture.width, model.texture.height));
        else ApplySubmeshColorToCube(data, v, node);

        float pixelPerUnit = 16;

        ScaleVertices(data, v, 24, new Vector3(node.size.x, node.size.y, node.size.z) / pixelPerUnit);
        ScaleVertices(data, v, 24, node.scale);
        MoveVertices(data, v, 24, node.offset);
        RotateVertices(data, v, 24, node.rot);
        MoveVertices(data, v, 24, node.pos);

        while(node.parentID != CraftstudioEntityInfo.invalidID)
        {
            int parentIndex = GetNodeIndexFromID(model, node.parentID);
            if (parentIndex < 0)
                break;

            node = model.nodes[parentIndex];

            MoveVertices(data, v, 24, node.offset);
            RotateVertices(data, v, 24, node.rot);
            MoveVertices(data, v, 24, node.pos);
        }
    }

    void GenerateBaseCube(MeshParamData<NormalUVColorVertexDefinition> data, int index, int vertex)
    {
        Vector3 min = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 max = new Vector3(0.5f, 0.5f, 0.5f);

        //top
        int v = vertex + (int)CraftstudioFace.top * 4;
        data.vertices[v + 0].pos = new Vector3(min.x, max.y, min.z);
        data.vertices[v + 1].pos = new Vector3(max.x, max.y, min.z);
        data.vertices[v + 2].pos = new Vector3(max.x, max.y, max.z);
        data.vertices[v + 3].pos = new Vector3(min.x, max.y, max.z);

        //down
        v = vertex + (int)CraftstudioFace.down * 4;
        data.vertices[v + 0].pos = new Vector3(min.x, min.y, min.z);
        data.vertices[v + 1].pos = new Vector3(max.x, min.y, min.z);
        data.vertices[v + 2].pos = new Vector3(max.x, min.y, max.z);
        data.vertices[v + 3].pos = new Vector3(min.x, min.y, max.z);

        //left
        v = vertex + (int)CraftstudioFace.left * 4;
        data.vertices[v + 0].pos = new Vector3(min.x, min.y, min.z);
        data.vertices[v + 1].pos = new Vector3(min.x, max.y, min.z);
        data.vertices[v + 2].pos = new Vector3(min.x, max.y, max.z);
        data.vertices[v + 3].pos = new Vector3(min.x, min.y, max.z);

        //right
        v = vertex + (int)CraftstudioFace.right * 4;
        data.vertices[v + 0].pos = new Vector3(max.x, min.y, min.z);
        data.vertices[v + 1].pos = new Vector3(max.x, max.y, min.z);
        data.vertices[v + 2].pos = new Vector3(max.x, max.y, max.z);
        data.vertices[v + 3].pos = new Vector3(max.x, min.y, max.z);

        //front
        v = vertex + (int)CraftstudioFace.front * 4;
        data.vertices[v + 0].pos = new Vector3(min.x, min.y, min.z);
        data.vertices[v + 1].pos = new Vector3(max.x, min.y, min.z);
        data.vertices[v + 2].pos = new Vector3(max.x, max.y, min.z);
        data.vertices[v + 3].pos = new Vector3(min.x, max.y, min.z);

        //back
        v = vertex + (int)CraftstudioFace.back * 4;
        data.vertices[v + 0].pos = new Vector3(min.x, min.y, max.z);
        data.vertices[v + 1].pos = new Vector3(max.x, min.y, max.z);
        data.vertices[v + 2].pos = new Vector3(max.x, max.y, max.z);
        data.vertices[v + 3].pos = new Vector3(min.x, max.y, max.z);

        for (int j = 0; j < 6; j++)
        {
            int rI = index + j * 6;
            int rV = vertex + j * 4;

            bool reverse = j == (int)CraftstudioFace.down || j == (int)CraftstudioFace.right || j == (int)CraftstudioFace.back;

            if (!reverse)
            {
                data.indexes[rI + 0] = (ushort)(rV + 0);
                data.indexes[rI + 1] = (ushort)(rV + 1);
                data.indexes[rI + 2] = (ushort)(rV + 2);
                data.indexes[rI + 3] = (ushort)(rV + 0);
                data.indexes[rI + 4] = (ushort)(rV + 2);
                data.indexes[rI + 5] = (ushort)(rV + 3);
            }
            else
            {
                data.indexes[rI + 0] = (ushort)(rV + 0);
                data.indexes[rI + 1] = (ushort)(rV + 2);
                data.indexes[rI + 2] = (ushort)(rV + 1);
                data.indexes[rI + 3] = (ushort)(rV + 0);
                data.indexes[rI + 4] = (ushort)(rV + 3);
                data.indexes[rI + 5] = (ushort)(rV + 2);
            }
        }
    }

    void ApplyUVToCube(MeshParamData<NormalUVColorVertexDefinition> data, int vertex, CraftstudioModelNode node, Vector2Int textureSize)
    {
        for(int i = 0; i < 6; i++)
        {
            var uv = GetUV(node, i);

            data.vertices[vertex + i * 4].uv = new Vector2(uv.downLeft.x / (float)textureSize.x, 1 - uv.downLeft.y / (float)textureSize.y);
            data.vertices[vertex + i * 4 + 1].uv = new Vector2(uv.downRight.x / (float)textureSize.x, 1 - uv.downRight.y / (float)textureSize.y);
            data.vertices[vertex + i * 4 + 2].uv = new Vector2(uv.topRight.x / (float)textureSize.x, 1 - uv.topRight.y / (float)textureSize.y);
            data.vertices[vertex + i * 4 + 3].uv = new Vector2(uv.topLeft.x / (float)textureSize.x, 1 - uv.topLeft.y / (float)textureSize.y); 
        }
    }

    void ApplySubmeshColorToCube(MeshParamData<NormalUVColorVertexDefinition> data, int vertex, CraftstudioModelNode node)
    {
        Color32 color = node.submeshIndex == 0 ? Color.white : SubmeshToColor(node.submeshIndex);

        for (int i = 0; i < 24; i++)
            data.vertices[vertex + i].color = color;
    }

    struct RectUV
    {
        public Vector2Int topLeft;
        public Vector2Int topRight;
        public Vector2Int downLeft;
        public Vector2Int downRight;
    }

    RectUV GetUV(CraftstudioModelNode node, int faceIndex)
    {
        var uv = new RectUV();

        Vector2Int size = Vector2Int.zero;
        Vector2Int pos = node.unwrapOffsetQuads[faceIndex];

        CraftstudioFace face = (CraftstudioFace)faceIndex;
        switch (face)
        {
            case CraftstudioFace.top:
            case CraftstudioFace.down:
                size.x = node.size.x;
                size.y = node.size.z;
                break;
            case CraftstudioFace.front:
            case CraftstudioFace.back:
                size.x = node.size.x;
                size.y = node.size.y;
                break;
            case CraftstudioFace.left:
            case CraftstudioFace.right:
                size.x = node.size.z;
                size.y = node.size.y;
                break;
        }

        int flags = node.unwrapFlags[faceIndex];

        int rot = 0;
        if ((flags & (int)CraftstudioModelNodeUnwrap.Rotate90) != 0)
            rot = 1;
        if ((flags & (int)CraftstudioModelNodeUnwrap.Rotate180) != 0)
            rot = 2;
        if ((flags & (int)CraftstudioModelNodeUnwrap.Rotate270) != 0)
            rot = 3;

        bool flipX = (flags & (int)CraftstudioModelNodeUnwrap.MirrorHorizontal) != 0;
        bool flipY = (flags & (int)CraftstudioModelNodeUnwrap.MirrorVertical) != 0;

        if (flipX)
            size.x *= -1;

        if (flipY)
            size.y *= -1;

        for (int i = 0; i < rot; i++)
        {
            var temp = size.x;
            size.x = -size.y;
            size.y = temp;
        }


        uv.downLeft = pos;
        uv.downRight = new Vector2Int(pos.x + size.x, pos.y);
        uv.topLeft = new Vector2Int(pos.x, pos.y + size.y);
        uv.topRight = pos + size;

        bool correctFlipX = false;
        bool correctFlipY = false;
        int correctRotate = 0;

        if(rot == 0 || rot == 2)
        {
            if (face == CraftstudioFace.right || face == CraftstudioFace.back)
                correctFlipX = true;
            if (face == CraftstudioFace.down)
                correctFlipY = true;
            if (face == CraftstudioFace.right || face == CraftstudioFace.left)
                correctRotate = 1;
            if (face == CraftstudioFace.front || face == CraftstudioFace.back)
                correctRotate = 2;
        }
        else if (rot == 1 || rot == 3)
        {
            if (face == CraftstudioFace.top || face == CraftstudioFace.front || face == CraftstudioFace.back)
                correctRotate = 1;
            if (face == CraftstudioFace.right)
                correctRotate = 2;
            if (face == CraftstudioFace.down)
                correctRotate = 3;
            if (face == CraftstudioFace.front || face == CraftstudioFace.top)
                correctFlipY = true;
            if (face == CraftstudioFace.left)
                correctFlipX = true;
            if (face == CraftstudioFace.front || face == CraftstudioFace.back)
                correctRotate = 2;
        }

        if (correctFlipX)
        {
            var temp = uv.topLeft;
            uv.topLeft = uv.topRight;
            uv.topRight = temp;
            temp = uv.downLeft;
            uv.downLeft = uv.downRight;
            uv.downRight = temp;
        }

        if(correctFlipY)
        {
            var temp = uv.topLeft;
            uv.topLeft = uv.downLeft;
            uv.downLeft = temp;
            temp = uv.topRight;
            uv.topRight = uv.downRight;
            uv.downRight = temp;
        }

        for(int i = 0; i < correctRotate; i++)
        {
            var temp = uv.topLeft;
            uv.topLeft = uv.topRight;
            uv.topRight = uv.downRight;
            uv.downRight = uv.downLeft;
            uv.downLeft = temp;
        }

        return uv;
    }

    void MoveVertices(MeshParamData<NormalUVColorVertexDefinition> data, int vertex, int size, Vector3 offset)
    {
        for(int i = vertex; i < vertex + size; i++)
        {
            data.vertices[i].pos += offset;
        }
    }

    void ScaleVertices(MeshParamData<NormalUVColorVertexDefinition> data, int vertex, int size, Vector3 scale)
    {
        ScaleVertices(data, vertex, size, scale, Vector3.zero);
    }

    void ScaleVertices(MeshParamData<NormalUVColorVertexDefinition> data, int vertex, int size, Vector3 scale, Vector3 center)
    {
        for(int i = vertex; i < vertex + size; i++)
        {
            Vector3 relativePos = data.vertices[i].pos - center;
            relativePos.x *= scale.x;
            relativePos.y *= scale.y;
            relativePos.z *= scale.z;
            relativePos += center;
            data.vertices[i].pos = relativePos;
        }
    }

    void RotateVertices(MeshParamData<NormalUVColorVertexDefinition> data, int vertex, int size, Quaternion rot)
    {
        RotateVertices(data, vertex, size, rot, Vector3.zero);
    }

    void RotateVertices(MeshParamData<NormalUVColorVertexDefinition> data, int vertex, int size, Quaternion rot, Vector3 center)
    {
        for(int i = vertex; i < vertex + size; i++)
        {
            Vector3 relativePos = data.vertices[i].pos - center;
            relativePos = rot * relativePos;
            relativePos += center;
            data.vertices[i].pos = relativePos;
        }
    }

    void MakeCurrentGameobject()
    {
        if (m_currentObject != null)
            DestroyImmediate(m_currentObject);

        if (gameObjectEditor != null) 
            DestroyImmediate(gameObjectEditor);

        m_currentObject = new GameObject();
        m_currentObject.name = "Craftstudio preview";
        m_currentObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
        m_filter = m_currentObject.AddComponent<MeshFilter>();
        m_renderer = m_currentObject.AddComponent<MeshRenderer>();
    }

    void SetObjectMeshAndMaterials()
    {
        if (m_currentObject == null || m_filter == null || m_renderer == null)
            return;

        m_filter.mesh = m_currentMesh;

        var mat = new Material(Shader.Find("Unlit/Transparent Cutout"));
        mat.SetTexture("_MainTex", m_currentModel.texture);
        m_renderer.material = mat;
    }
}
 enum CraftstudioEntityType
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
    public const UInt16 invalidID = 0xFFFF;

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

class CraftstudioEntityTree
{
    public int entityIndex;
    public bool folded = false;
    public List<CraftstudioEntityTree> childrens = new List<CraftstudioEntityTree>();
}

class CraftstudioModel
{
    public List<CraftstudioModelNode> nodes = new List<CraftstudioModelNode>();
    public CraftstudioModelNodeTree tree;
    public int submeshNb;
    public List<UInt16> animations = new List<UInt16>();
    public Texture texture;
}

class CraftstudioModelNode
{
    public UInt16 ID;
    public UInt16 parentID;
    public string name;
    public Vector3 pos;
    public Vector3 offset;
    public Vector3 scale;
    public Quaternion rot;
    public Vector3Int size;
    public CraftstudioModelCubeUnwrap unwrapMode; //
    public Vector2Int[] unwrapOffsetQuads = new Vector2Int[6];
    public int[] unwrapFlags = new int[6];

    public int submeshIndex;
    public bool bone = true;
}

class CraftstudioModelNodeTree
{
    public int nodeIndex;
    public bool folded = true;
    public List<CraftstudioModelNodeTree> childrens = new List<CraftstudioModelNodeTree>();
}

enum CraftstudioFace
{
    back,
    front,
    right,
    down,
    left,
    top,
}

enum CraftstudioModelCubeUnwrap
{
    collapsed = 0,
    full = 1,
    custom = 2,
}

enum CraftstudioModelNodeUnwrap
{
    None = 0,
    Rotate90 = 1 << 0,
    Rotate180 =	1 << 1,
    Rotate270 = 1 << 2,
    MirrorHorizontal = 1 << 3,
    MirrorVertical = 1 << 4,
}

class CraftstudioModelAnimation
{
    public int index;
    public bool export;

    public UInt16 duration;
    public bool holdLastKey;
    public List<CraftstudioModelAnimationNode> nodes = new List<CraftstudioModelAnimationNode>();
}

class CraftstudioModelAnimationNode
{
    public string name;
    public List<CraftstudioModelAnimationNodeVector3> pos = new List<CraftstudioModelAnimationNodeVector3>();
    public List<CraftstudioModelAnimationNodeVector3> offset = new List<CraftstudioModelAnimationNodeVector3>();
    public List<CraftstudioModelAnimationNodeVector3> scale = new List<CraftstudioModelAnimationNodeVector3>();
    public List<CraftstudioModelAnimationNodeQuaternion> rot = new List<CraftstudioModelAnimationNodeQuaternion>();
    public List<CraftstudioModelAnimationNodeVector3Int> size = new List<CraftstudioModelAnimationNodeVector3Int>();
}

class CraftstudioModelAnimationNodeVector3
{
    public UInt16 keyTime;
    public Vector3 value;
}

class CraftstudioModelAnimationNodeVector3Int
{
    public UInt16 keyTime;
    public Vector3Int value;
}

class CraftstudioModelAnimationNodeQuaternion
{
    public UInt16 keyTime;
    public Quaternion value;
}
