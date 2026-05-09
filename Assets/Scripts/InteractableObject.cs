using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AREducationApp
{
    /// <summary>
    /// AR sahnesindeki objelerin dokunma (Touch) ile döndürülmesini (Rotate) ve boyutlandırılmasını (Scale) sağlar.
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        [Header("Ayarlar")]
        public float RotationSpeed = 0.2f;
        public float ScaleSpeed = 0.01f;
        public float MinScale = 0.1f;
        public float MaxScale = 5.0f;

        private float _initialDistance;
        private Vector3 _initialScale;

        void Update()
        {
            // Eğer 1 parmakla dokunuluyorsa, döndürme (Rotation) yap
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                // Dokunma işlemi sürüklendiğinde objeyi Y ekseninde döndür
                if (touch.phase == TouchPhase.Moved)
                {
                    float rotationX = touch.deltaPosition.x * RotationSpeed;
                    transform.Rotate(Vector3.up, -rotationX, Space.World);
                }
            }
            // Eğer 2 parmakla dokunuluyorsa, büyütme/küçültme (Pinch to Scale) yap
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    _initialDistance = Vector2.Distance(touch1.position, touch2.position);
                    _initialScale = transform.localScale;
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                    
                    if (Mathf.Approximately(_initialDistance, 0)) return; // Sıfıra bölme hatasını önle
                    
                    float distanceFactor = currentDistance / _initialDistance;
                    Vector3 newScale = _initialScale * distanceFactor;

                    // Ölçeği sınırla (Clamp)
                    newScale.x = Mathf.Clamp(newScale.x, MinScale, MaxScale);
                    newScale.y = Mathf.Clamp(newScale.y, MinScale, MaxScale);
                    newScale.z = Mathf.Clamp(newScale.z, MinScale, MaxScale);

                    transform.localScale = newScale;
                }
            }
        }
    }
}
