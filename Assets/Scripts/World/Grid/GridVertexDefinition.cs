using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public struct GridVertexDefinition
{
    public Vector3 pos;
    public Vector3 normal;
    public Vector2 uv;
}

public static class GridMeshEx
{
    static public void SetGridMeshParams(Mesh mesh, int vertexNb, int indexNb)
    {
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        mesh.SetVertexBufferParams(vertexNb, layout);

        mesh.SetIndexBufferParams(indexNb, IndexFormat.UInt16);
    }
}
