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
    [SerializeField] Transform m_ship = null;
    [SerializeField] float m_shipAppear = 3;

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

        m_ship.gameObject.SetActive(false);

        DOVirtual.DelayedCall(m_delay, () =>
        {
            DOVirtual.Float(-1, 0.2f, m_duration, x => m_material.SetFloat(radius, x)).SetEase(Ease.OutSine);
        });

        DOVirtual.DelayedCall(m_delay + m_duration, () =>
        {
            m_ship.gameObject.SetActive(true);
            float y = m_ship.transform.position.y;
            m_ship.DOMoveY(y + 0.6f, m_shipAppear).SetEase(Ease.OutSine);
        });

        DOVirtual.DelayedCall(m_delay + m_duration + m_shipAppear, () =>
        {
            DOVirtual.Float(0.2f, -1, m_duration, x => m_material.SetFloat(radius, x)).SetEase(Ease.InSine);
        });
    }
}
