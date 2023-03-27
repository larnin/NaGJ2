using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CutMeshTest : MonoBehaviour
{
    [SerializeField] MeshFilter m_cutMesh;
    [SerializeField] float m_thickness = 1;

    Mesh m_mesh;
    MeshFilter m_meshFilter;

    private void Start()
    {
        m_meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        var shape = CutMesh.CutSurface(m_cutMesh, new Plane(transform.up, transform.position));

        for(int i = 0; i < shape.GetNbPoints(); i++)
        {
            int i2 = i == 0 ? shape.GetNbPoints() - 1 : i - 1;

            var p1 = shape.GetPoint(i);
            var p2 = shape.GetPoint(i2);
        }

        if(shape.GetNbPoints() < 3)
        {
            m_meshFilter.mesh = null;
            return;
        }

        if (m_mesh == null)
            m_mesh = new Mesh();
        CutMesh.MakeShape(m_mesh, shape, transform.position, m_thickness);
        m_meshFilter.mesh = m_mesh;
    }
}
