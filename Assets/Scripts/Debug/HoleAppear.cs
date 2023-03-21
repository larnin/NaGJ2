using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class HoleAppear : MonoBehaviour
{
    [SerializeField] float m_delay = 2;
    [SerializeField] float m_duration = 5;

    Renderer m_renderer;
    Material m_material;

    const string radius = "_HoleSize";

    private void Start()
    {
        m_renderer = GetComponent<Renderer>();
        if(m_renderer != null)
        {
            m_material = m_renderer.material;
            m_renderer.material = m_material;
        }

        m_material.SetFloat(radius, -1);

        DOVirtual.DelayedCall(m_delay, () =>
        {
            DOVirtual.Float(-1, 0.2f, m_duration, x => m_material.SetFloat(radius, x)).SetEase(Ease.OutSine);
        });
    }
}
