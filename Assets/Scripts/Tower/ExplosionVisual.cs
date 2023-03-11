using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ExplosionVisual : MonoBehaviour
{
    const string colorName = "_RimColor";

    [SerializeField] float m_maxTime = 1;
    [SerializeField] float m_sphereTime = 1;

    Transform m_sphere;
    Renderer m_sphereRender;
    Material m_sphereMaterial;

    float m_sphereTimer = 0;
    float m_sphereScale = 1;
    Color m_initialColor;

    private void Awake()
    {
        m_sphere = transform.Find("Sphere");
        if (m_sphere == null)
            return;

        m_sphereScale = m_sphere.localScale.x;
        m_sphere.localScale = Vector3.zero;

        m_sphereRender = m_sphere.GetComponent<Renderer>();
        m_sphereMaterial = m_sphereRender.material;
        m_sphereRender.material = m_sphereMaterial;

        m_initialColor = m_sphereMaterial.GetColor(colorName);

        Destroy(gameObject, m_maxTime);
    }

    private void Start()
    {
        var particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
            particles.transform.localScale = transform.localScale;
    }

    private void Update()
    {
        m_sphereTimer += Time.deltaTime;

        float normTime = m_sphereTimer / m_sphereTime;
        float scale = m_sphereScale * Mathf.Sqrt(normTime);
        m_sphere.localScale = new Vector3(scale, scale, scale);

        normTime *= 4;
        normTime -= 3;

        if (normTime >= 0 && normTime < 1)
        {
            normTime = 1 - normTime;
            Color c = m_initialColor;
            c *= normTime;
            m_sphereMaterial.SetColor(colorName, c);
        }
        else if (normTime >= 1)
            m_sphereRender.gameObject.SetActive(false);
    }
}
