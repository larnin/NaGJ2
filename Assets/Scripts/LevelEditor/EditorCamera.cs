using System.Collections;
using UnityEngine;

public class EditorCamera : MonoBehaviour
{
    [SerializeField] float m_zoomMultiplier = 0.1f;
    [SerializeField] float m_moveSpeed = 1;
    [SerializeField] float m_rotationSpeed = 1;

    Vector3 m_target;
    Vector2 m_oldMousePos;
    Camera m_camera;

    void Start()
    {
        Plane p = new Plane(Vector3.up, Vector3.zero);
        float enter;
        var ray = new Ray(transform.position, transform.forward);
        bool casted = p.Raycast(ray, out enter);
        if (casted)
            m_target = ray.GetPoint(enter);
        else m_target = transform.position + transform.forward * 10;

        m_camera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(2) && !Utility.MouseOverUI())
        {
            m_oldMousePos = Input.mousePosition;
        }
        else if(Input.GetMouseButton(2))
        {
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                Vector3 dir = m_target - transform.position;

                float angleX = Mathf.Atan2(dir.z, dir.x);

                float dirH = new Vector2(dir.x, dir.z).magnitude;
                float angleY = Mathf.Atan2(dir.y, dirH);

                Vector2 offset = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - m_oldMousePos;

                angleX -= offset.x * m_rotationSpeed * Time.deltaTime;
                angleY += offset.y * m_rotationSpeed * Time.deltaTime;

                const float maxAngle = Mathf.PI * 0.45f;
                if (angleY < -Mathf.PI)
                    angleY += Mathf.PI * 2;
                if (angleY > Mathf.PI)
                    angleY -= Mathf.PI * 2;
                if (angleY < -maxAngle)
                    angleY = -maxAngle;
                if (angleY > maxAngle)
                    angleY = maxAngle;

                float dist = dir.magnitude;

                dirH = Mathf.Cos(angleY);
                dir.y = Mathf.Sin(angleY);
                dir.x = Mathf.Cos(angleX) * dirH;
                dir.z = Mathf.Sin(angleX) * dirH;
                dir *= dist;

                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    transform.position = m_target - dir;
                else m_target = transform.position + dir;

                transform.LookAt(m_target, Vector3.up);
            }
            else
            {
                Plane p = new Plane(Vector3.up, m_target);

                var ray1 = m_camera.ScreenPointToRay(Input.mousePosition);
                var ray2 = m_camera.ScreenPointToRay(m_oldMousePos);

                float enter1, enter2;
                bool b1 = p.Raycast(ray1, out enter1);
                bool b2 = p.Raycast(ray2, out enter2);

                if (b1 && b2)
                {
                    Vector3 pos1 = ray1.GetPoint(enter1);
                    Vector3 pos2 = ray2.GetPoint(enter2);

                    Vector3 offset = pos2 - pos1;
                    transform.position = transform.position + offset * m_moveSpeed;
                }
            }
        }
        else if (Input.mouseScrollDelta.y != 0)
        {
            float zoom = Mathf.Pow(m_zoomMultiplier, Input.mouseScrollDelta.y);

            Vector3 dir = m_target - transform.position;
            dir *= zoom;
            transform.position = m_target - dir;
        }

        m_oldMousePos = Input.mousePosition;
    }
}