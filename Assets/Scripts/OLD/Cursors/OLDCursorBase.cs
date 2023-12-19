using UnityEngine;

public enum OLDCursorValidation
{
    hidden,
    neutral,
    allowed,
    disallowed,
    warning,
}

public abstract class OLDCursorBase : MonoBehaviour
{
    [SerializeField] GameObject m_cursorPrefab;
    [SerializeField] Color m_neutralColor = Color.white;
    [SerializeField] Color m_allowedColor = Color.green;
    [SerializeField] Color m_disallowedColor = Color.red;
    [SerializeField] Color m_warningColor = Color.yellow;

    GameObject m_cursor;
    MeshRenderer m_cursorRenderer;
    Camera m_camera;

    protected abstract OLDCursorValidation ValidatePos(int x, int y);
    protected abstract void OnLeftClick(int x, int y);
    protected abstract void OnRightClick(int x, int y);
    protected abstract void OnMiddleClick(int x, int y);

    private void Awake()
    {
        m_cursor = Instantiate(m_cursorPrefab);
        m_cursor.SetActive(false);
        m_cursor.transform.parent = transform;
        m_cursorRenderer = m_cursor.GetComponentInChildren<MeshRenderer>();
        m_camera = Camera.main; //keep it main please
    }

    private void OnDestroy()
    {
        Destroy(m_cursor);
    }

    private void Update()
    {
        if (OLDWorldHolder.Instance() == null)
            return;

        OnUpdate();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var pos = OLDWorldHolder.Instance().RayToPos(ray);

        OLDCursorValidation state = ValidatePos(pos.x, pos.y);
        UpdateCursor(state, pos.x, pos.y);

        if (Input.GetMouseButtonUp(0))
            OnLeftClick(pos.x, pos.y);
        if (Input.GetMouseButtonUp(1))
            OnRightClick(pos.x, pos.y);
        if (Input.GetMouseButtonUp(2))
            OnMiddleClick(pos.x, pos.y);
    }

    protected abstract void OnUpdate();

    void UpdateCursor(OLDCursorValidation state, int x, int y)
    {
        if(state == OLDCursorValidation.hidden)
        {
            m_cursor.SetActive(false);
            return;
        }

        m_cursor.SetActive(true);

        Color c = m_neutralColor;
        if (state == OLDCursorValidation.allowed)
            c = m_allowedColor;
        else if (state == OLDCursorValidation.disallowed)
            c = m_disallowedColor;
        else if (state == OLDCursorValidation.warning)
            c = m_warningColor;

        var mat = m_cursorRenderer.material;
        mat.SetColor("_Color", c);
        m_cursorRenderer.material = mat;

        var pos = OLDWorldHolder.Instance().GetElemPos(x, y);
        m_cursor.transform.position = pos;
    }
}
