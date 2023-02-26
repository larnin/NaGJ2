using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] float m_minZoom = 25;
    [SerializeField] float m_maxZoom = 1;
    [SerializeField] float m_stepZoom = 1.1f;

    Camera m_camera;
    float m_initialZoom;
    Vector3 m_initialPosition;

    Vector3 m_oldMousePos;

    private void Start()
    {
        m_camera = Camera.main;
        m_initialPosition = transform.position;
        m_initialZoom = m_camera.orthographicSize;
    }

    private void Update()
    {
        float scrollY = Input.mouseScrollDelta.y;
        float multiplier = MathF.Pow(m_stepZoom, MathF.Abs(scrollY));
        if (MathF.Sign(scrollY) < 0)
            multiplier = 1/multiplier;

        float newSize = m_camera.orthographicSize * multiplier;
        if (newSize > m_maxZoom && newSize < m_minZoom)
            m_camera.orthographicSize = newSize;

        if(Input.GetMouseButton(2))
        {
            var oldRay = m_camera.ScreenPointToRay(m_oldMousePos);
            var newRay = m_camera.ScreenPointToRay(Input.mousePosition);

            Plane p = new Plane(Vector3.up, Vector3.zero);

            float enter;
            p.Raycast(oldRay, out enter);
            Vector3 oldPos = oldRay.GetPoint(enter);

            p.Raycast(newRay, out enter);
            Vector3 newPos = newRay.GetPoint(enter);

            Vector3 delta = newPos - oldPos;

            Vector3 currentPos = m_camera.transform.position;
            currentPos -= delta;
            m_camera.transform.position = currentPos;
        }

        m_oldMousePos = Input.mousePosition;
    }
}
