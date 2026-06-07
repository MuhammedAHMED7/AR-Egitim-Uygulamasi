using UnityEngine;

public class CellInteraction : MonoBehaviour
{
    private float initialDistance;
    private Vector3 initialScale;

    void Update()
    {
        // One finger touch: Rotate the cell
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                transform.Rotate(0f, -touch.deltaPosition.x * 0.5f, 0f, Space.World);
            }
        }
        // Two finger touch: Pinch to zoom
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(t1.position, t2.position);
                initialScale = transform.localScale;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(t1.position, t2.position);
                if (Mathf.Approximately(initialDistance, 0)) return;

                float factor = currentDistance / initialDistance;
                transform.localScale = initialScale * factor;
            }
        }
    }
}
