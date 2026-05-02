using UnityEngine;

/// <summary>
/// Yerleştirilen modele dokunulunca
/// iki parmak pinch (scale) ve döndürme jestlerini yakalar.
/// Model prefabına ekle.
/// </summary>
public class TouchGestureController : MonoBehaviour
{
    [Header("Ayarlar")]
    public float scaleSpeed  = 0.005f;
    public float rotateSpeed = 0.5f;
    public float minScale    = 0.2f;
    public float maxScale    = 3.0f;

    [Header("World Space Panel")]
    public GameObject worldSpacePanel;

    private bool  _selected       = false;
    private float _prevPinchDist  = 0f;
    private float _prevAngle      = 0f;

    void Update()
    {
        if (!_selected) return;

        // Tek parmak: başka yere dokunulursa seçimi kaldır
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(t.position);
                if (!Physics.Raycast(ray, out RaycastHit hit) || hit.transform != transform)
                    Deselect();
            }
        }

        // İki parmak: scale + rotate
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            float dist  = Vector2.Distance(t0.position, t1.position);
            float angle = Mathf.Atan2(
                t1.position.y - t0.position.y,
                t1.position.x - t0.position.x) * Mathf.Rad2Deg;

            if (t1.phase == TouchPhase.Began)
            {
                _prevPinchDist = dist;
                _prevAngle     = angle;
                return;
            }

            // Scale
            float newScale = Mathf.Clamp(
                transform.localScale.x + (dist - _prevPinchDist) * scaleSpeed,
                minScale, maxScale);
            transform.localScale = Vector3.one * newScale;

            // Rotate
            transform.Rotate(Vector3.up, -(angle - _prevAngle) * rotateSpeed, Space.World);

            _prevPinchDist = dist;
            _prevAngle     = angle;
        }
    }

    public void Select()
    {
        _selected = true;
        if (worldSpacePanel)
        {
            worldSpacePanel.SetActive(true);
            worldSpacePanel.transform.position =
                transform.position + Vector3.right * 0.35f + Vector3.up * 0.2f;
        }
    }

    public void Deselect()
    {
        _selected = false;
    }
}
