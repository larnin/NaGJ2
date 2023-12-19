using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OLDGrid : MonoBehaviour
{
    [SerializeField] int m_oversize = 2;
    [SerializeField] float m_thickness = 0.05f;

    RectInt m_bounds = new RectInt(-100000, -100000, 0, 0);

    MeshFilter m_filter = null;
    Mesh m_mesh = null;

    SimpleMeshParam<NormalUVVertexDefinition> m_gridVertexs = new SimpleMeshParam<NormalUVVertexDefinition>();

    private void Awake()
    {
        m_filter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        var newBounds = OLDWorldHolder.Instance().GetBounds();

        if(newBounds.x != m_bounds.x || newBounds.y != m_bounds.y || newBounds.width != m_bounds.width || newBounds.height != m_bounds.height)
        {
            m_bounds = newBounds;
            UpdateGrid();
        }
    }

    void UpdateGrid()
    {
        m_gridVertexs.Reset();

        int nbLineX = m_bounds.width - 1 + 2 * m_oversize;
        int nbLineY = m_bounds.height - 1 + 2 * m_oversize;

        int minX = m_bounds.x + 1 - m_oversize;
        int minY = m_bounds.y + 1 - m_oversize;

        int nbVertex = (nbLineX + nbLineY) * 4;
        int nbIndex = (nbLineX + nbLineY) * 6;

        var data = m_gridVertexs.Allocate(nbVertex, nbIndex);

        float offset = 0;
        const float increase = 0.2f;
        for (int i = 0; i < nbLineX; i++)
        {
            DrawRect(data, new Rect(minX + i - m_thickness / 2, minY - 1, m_thickness, nbLineY + 1), offset);
            offset += increase;
        }

        for (int i = 0; i < nbLineY; i++)
        {
            DrawRect(data, new Rect(minX - 1, minY + i - m_thickness / 2, nbLineX + 1, m_thickness), offset);
            offset += increase;
        }

        if (m_mesh == null)
            m_mesh = new Mesh();

        MeshEx.SetNormalUVMeshParams(m_mesh, data.verticesSize, data.indexesSize);

        m_mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        m_mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        m_mesh.subMeshCount = 1;
        m_mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        Bounds b = new Bounds(new Vector3(m_bounds.x - m_oversize, -1, m_bounds.y - m_oversize), new Vector3(m_bounds.width + m_oversize * 2, 2, m_bounds.height + m_oversize + 2));

        m_mesh.bounds = b;

        if (m_filter != null)
            m_filter.mesh = m_mesh;
    }

    Vector3 Flip(Vector3 other)
    {
        return new Vector3(other.z, other.y, other.x);
    }

    void DrawRect(MeshParamData<NormalUVVertexDefinition> data, Rect bounds, float uvOffset)
    {
        Vector3 pos1 = new Vector3(bounds.x, 0, bounds.y);
        Vector3 pos2 = new Vector3(bounds.x + bounds.width, 0, bounds.y);
        Vector3 pos3 = new Vector3(bounds.x + bounds.width, 0, bounds.y + bounds.height);
        Vector3 pos4 = new Vector3(bounds.x, 0, bounds.y + bounds.height);

        data.vertices[data.verticesSize].pos = pos1;
        data.vertices[data.verticesSize + 1].pos = pos2;
        data.vertices[data.verticesSize + 2].pos = pos3;
        data.vertices[data.verticesSize + 3].pos = pos4;

        bool larger = bounds.width > bounds.height;
        if(larger)
        {
            float center = bounds.y + bounds.height / 2;
            pos1.z = center;
            pos2.z = center;
            pos3.z = center;
            pos4.z = center;
            pos1 = Flip(pos1);
            pos2 = Flip(pos2);
            pos3 = Flip(pos3);
            pos4 = Flip(pos4);
        }
        else
        {
            float center = bounds.x + bounds.width / 2;
            pos1.x = center;
            pos2.x = center;
            pos3.x = center;
            pos4.x = center;
        }

        data.vertices[data.verticesSize].uv = new Vector2(pos1.x + uvOffset, pos1.z + uvOffset);
        data.vertices[data.verticesSize + 1].uv = new Vector2(pos2.x + uvOffset, pos2.z + uvOffset);
        data.vertices[data.verticesSize + 2].uv = new Vector2(pos3.x + uvOffset, pos3.z + uvOffset);
        data.vertices[data.verticesSize + 3].uv = new Vector2(pos4.x + uvOffset, pos4.z + uvOffset);

        byte v = (byte)(larger ? 1 : 0);
        Vector3 n1 = new Vector3(1, 1-v, 0);
        Vector3 n2 = new Vector3(v, 1, 0);
        Vector3 n3 = new Vector3(0, v, 0);
        Vector3 n4 = new Vector3(1 - v, 0, 0);

        data.vertices[data.verticesSize].normal = n1;
        data.vertices[data.verticesSize + 1].normal = n2;
        data.vertices[data.verticesSize + 2].normal = n3;
        data.vertices[data.verticesSize + 3].normal = n4;

        ushort vertexIndex = (ushort)data.verticesSize;

        data.indexes[data.indexesSize] = vertexIndex;
        data.indexes[data.indexesSize + 1] = (ushort)(vertexIndex + 3);
        data.indexes[data.indexesSize + 2] = (ushort)(vertexIndex + 1);
        data.indexes[data.indexesSize + 3] = (ushort)(vertexIndex + 1);
        data.indexes[data.indexesSize + 4] = (ushort)(vertexIndex + 3);
        data.indexes[data.indexesSize + 5] = (ushort)(vertexIndex + 2);

        data.verticesSize += 4;
        data.indexesSize += 6;
    }
}
