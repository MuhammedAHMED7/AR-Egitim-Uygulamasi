using UnityEngine;
using UnityEngine.EventSystems;

namespace AREgitim.UI
{
    /// <summary>
    /// İki parmak pinch-to-zoom ve rotasyon jestleri.
    /// Yerleştirilen modeli ölçekler ve döndürür.
    /// UI üzerine yapılan dokunuşları görmezden gelir.
    /// </summary>
    public class TouchGestureController : MonoBehaviour
    {
        [Tooltip("Hedef modelin Transform'u. Boşsa AR_Placed_Model adlı GameObject aranır.")]
        public Transform target;

        [Range(0.05f, 5f)] public float minScale = 0.1f;
        [Range(0.05f, 10f)] public float maxScale = 4f;
        public float zoomSensitivity = 0.01f;
        public float rotationSensitivity = 1f;

        float _prevDistance;
        float _prevAngle;

        void Update()
        {
            if (target == null) return;

            if (Input.touchCount < 2) return;

            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // UI üzerinde dokunuluyorsa atla
            if (IsTouchOverUI(t0.fingerId) || IsTouchOverUI(t1.fingerId)) return;

            if (t1.phase == TouchPhase.Began)
            {
                _prevDistance = Vector2.Distance(t0.position, t1.position);
                _prevAngle = AngleBetween(t0.position, t1.position);
                return;
            }

            // Pinch — ölçekleme
            float currDistance = Vector2.Distance(t0.position, t1.position);
            float deltaDist = currDistance - _prevDistance;
            if (Mathf.Abs(deltaDist) > 1f)
            {
                float scaleFactor = 1f + deltaDist * zoomSensitivity * 0.1f;
                Vector3 s = target.localScale * scaleFactor;
                float clamped = Mathf.Clamp(s.x, minScale, maxScale);
                target.localScale = new Vector3(clamped, clamped, clamped);
                _prevDistance = currDistance;
            }

            // Rotasyon
            float currAngle = AngleBetween(t0.position, t1.position);
            float deltaAngle = Mathf.DeltaAngle(_prevAngle, currAngle);
            if (Mathf.Abs(deltaAngle) > 0.3f)
            {
                target.Rotate(Vector3.up, -deltaAngle * rotationSensitivity, Space.World);
                _prevAngle = currAngle;
            }
        }

        static float AngleBetween(Vector2 a, Vector2 b)
        {
            Vector2 diff = b - a;
            return Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        }

        static bool IsTouchOverUI(int fingerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
        }
    }
}
