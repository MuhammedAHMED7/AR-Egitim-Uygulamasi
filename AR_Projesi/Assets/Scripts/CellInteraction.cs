using UnityEngine;

public class CellInteraction : MonoBehaviour
{
    private float rotationSpeed = 0.2f;
    private float zoomSpeed = 0.005f;
    private float minScale = 0.05f;
    private float maxScale = 0.3f;

    void Update()
    {
        // 1. Handle Rotation (Single Touch)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                // Swipe left/right rotates around the Y axis, up/down around the X axis
                transform.Rotate(touch.deltaPosition.y * rotationSpeed, -touch.deltaPosition.x * rotationSpeed, 0f, Space.World);
            }
        }

        // 2. Handle Zoom (Two-Finger Pinch)
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the distance between the touches
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;

            ScaleObject(difference * zoomSpeed);
        }
    }

    void ScaleObject(float increment)
    {
        Vector3 newScale = transform.localScale + new Vector3(increment, increment, increment);

        // Prevent the model from becoming too small or too large
        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
        newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
        newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

        transform.localScale = newScale;
    }
}
