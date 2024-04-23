using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public struct NormalUVVertexDefinition
{
    public Vector3 pos;
    public Vector3 normal;
    public Vector4 tangent;
    public Vector2 uv;
}

public struct NormalUVColorVertexDefinition
{
    public Vector3 pos;
    public Vector3 normal;
    public Vector4 tangent;
    public Color32 color;
    public Vector2 uv;
}

public struct NormalUVBoneVertexDefinition
{
    public Vector3 pos;
    public Vector3 normal;
    public Vector4 tangent;
    public Vector2 uv;
    public float boneWeight;
    public int boneIndex;
}

public static class MeshEx
{
    static public void SetNormalUVMeshParams(Mesh mesh, int vertexNb, int indexNb)
    {
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        mesh.SetVertexBufferParams(vertexNb, layout);

        mesh.SetIndexBufferParams(indexNb, IndexFormat.UInt16);
    }

    static public void SetNormalUVColorMeshParams(Mesh mesh, int vertexNb, int indexNb)
    {
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        mesh.SetVertexBufferParams(vertexNb, layout);

        mesh.SetIndexBufferParams(indexNb, IndexFormat.UInt16);
    }

    static public void SetNormalUVBoneMeshParams(Mesh mesh, int vertexNb, int indexNb)
    {
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.Float32, 1),
            new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.SInt32, 1)
        };

        mesh.SetVertexBufferParams(vertexNb, layout);

        mesh.SetIndexBufferParams(indexNb, IndexFormat.UInt16);
    }
}
