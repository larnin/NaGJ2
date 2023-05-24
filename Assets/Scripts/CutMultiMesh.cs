using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class CutMultiMesh : MonoBehaviour
{
    [SerializeField] MeshFilter m_meshFilter;
    [SerializeField] float m_moveDistance = 1;
    [SerializeField] float m_moveDuration = 1;
    [SerializeField] float m_thickness = 0.1f;

    class EntityData
    {
        public GameObject entity;
        public MeshFilter filter;
        public Vector3 start;
        public Vector3 end;
        public float timer;

        public EntityData(GameObject _entity, float _distance, float _height)
        {
            entity = _entity;
            filter = entity.GetComponentInChildren<MeshFilter>();
            timer = 0;

            start = entity.transform.position;
            end = entity.transform.position;

            start.y = _height - _distance;
            end.y = _height + _distance;
        }
    }

    List<EntityData> m_entities = new List<EntityData>();

    Mesh m_mesh;

    public void AddEntity(GameObject entity)
    {
        Event<SetBehaviourEnabledEvent>.Broadcast(new SetBehaviourEnabledEvent(false), entity);

        var data = new EntityData(entity, m_moveDistance, transform.position.y);

        m_entities.Add(data);
    }

    public bool HaveEntity()
    {
        return m_entities.Count > 0;
    }

    private void Update()
    {
        List<EntityData> nextEntities = new List<EntityData>();

        foreach(var e in m_entities)
        {
            if (ProcessOneEntity(e))
                nextEntities.Add(e);
        }

        m_entities = nextEntities;

        Draw();
    }

    bool ProcessOneEntity(EntityData e)
    {
        e.timer += Time.deltaTime;

        float percent = e.timer / m_moveDuration;
        var pos = DOVirtual.EasedValue(e.start, e.end, percent, Ease.OutSine);

        e.entity.transform.position = pos;

        if (e.timer >= m_moveDuration)
            Event<SetBehaviourEnabledEvent>.Broadcast(new SetBehaviourEnabledEvent(true), e.entity);

        return e.timer < m_moveDuration;
    }

    void Draw()
    {
        SimpleMeshParam<NormalUVVertexDefinition> vertices = new SimpleMeshParam<NormalUVVertexDefinition>();

        Vector3 center = transform.position;
        bool haveShape = false;

        foreach(var e in m_entities)
        {
            if (e.filter == null)
                continue;

            var shape = CutMesh.CutSurface(e.filter, new Plane(transform.up, center));

            if (shape.GetNbPoints() < 3)
                continue;

            CutMesh.AddShape(vertices, shape, center, m_thickness);
            haveShape = true;
        }

        if (!haveShape)
        {
            m_meshFilter.mesh = null;
            return;
        }

        if (m_mesh == null)
            m_mesh = new Mesh();
        CutMesh.MakeShape(m_mesh, vertices, center);
        m_meshFilter.mesh = m_mesh;


        CutMesh.MakeShape(m_mesh, vertices, center);
    }
}
