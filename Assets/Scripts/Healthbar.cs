using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Healthbar : MonoBehaviour
{
    GameObject m_parent;
    GameObject m_meshObj;
    Material m_mat;
    Camera m_camera;

    private void Start()
    {
        m_parent = transform.parent.gameObject;

        var renderer = GetComponentInChildren<MeshRenderer>();
        if(renderer != null)
        {
            m_meshObj = renderer.gameObject;
            m_mat = renderer.material; //clone material to not share with other lifebar
            renderer.material = m_mat;
        }
        m_camera = Camera.main;
    }

    private void Update()
    {
        UpdateLife();
        UpdateRot();
    }

    void UpdateLife()
    {
        GetLifeEvent e = new GetLifeEvent();
        Event<GetLifeEvent>.Broadcast(e, m_parent);

        if (e.maxLife <= 0)
            e.maxLife = 1;

        float lifePercent = e.life / e.maxLife;

        m_mat.SetFloat("_FillPercent", lifePercent);

        bool show = lifePercent < 0.99f && lifePercent > 0.01f;
        m_meshObj.SetActive(show);
    }

    void UpdateRot()
    {
        Vector3 lookDir = -m_camera.transform.forward;
        lookDir.y = 0;
        lookDir.Normalize();

        Quaternion rot = Quaternion.LookRotation(lookDir);

        transform.rotation = rot;
    }
}