using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.UI
{
    /// <summary>
    /// Yerleştirilen modelin yanında 3D uzayda asılı duran kontrol paneli.
    /// Her zaman AR kamerasına döner (billboard).
    /// Döndürme, büyütme, küçültme, silme işlevlerini destekler.
    /// </summary>
    public class WorldSpacePanel : MonoBehaviour
    {
        public Transform target;            // Kontrol edilen model
        public Camera referenceCamera;      // Genelde AR Camera
        public Button rotateButton;
        public Button scaleUpButton;
        public Button scaleDownButton;
        public Button deleteButton;

        [Tooltip("Modelin üstünde panelin asılı duracağı yükseklik")]
        public float verticalOffset = 0.35f;

        [Range(0.05f, 5f)] public float minScale = 0.1f;
        [Range(0.05f, 10f)] public float maxScale = 4f;
        public float scaleStep = 1.15f;
        public float rotationStep = 30f;

        void Awake()
        {
            if (referenceCamera == null) referenceCamera = Camera.main;
        }

        void Start()
        {
            if (rotateButton != null)     rotateButton.onClick.AddListener(OnRotate);
            if (scaleUpButton != null)    scaleUpButton.onClick.AddListener(OnScaleUp);
            if (scaleDownButton != null)  scaleDownButton.onClick.AddListener(OnScaleDown);
            if (deleteButton != null)     deleteButton.onClick.AddListener(OnDelete);
        }

        void LateUpdate()
        {
            if (target == null)
            {
                gameObject.SetActive(false);
                return;
            }
            // Konum: modelin biraz üstü
            transform.position = target.position + Vector3.up * verticalOffset;

            // Billboard: kameraya dön
            if (referenceCamera != null)
            {
                Vector3 toCam = referenceCamera.transform.position - transform.position;
                toCam.y = 0f;
                if (toCam.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
            }
        }

        void OnRotate()
        {
            if (target != null) target.Rotate(Vector3.up, rotationStep, Space.World);
        }

        void OnScaleUp()
        {
            if (target == null) return;
            float s = Mathf.Min(target.localScale.x * scaleStep, maxScale);
            target.localScale = new Vector3(s, s, s);
        }

        void OnScaleDown()
        {
            if (target == null) return;
            float s = Mathf.Max(target.localScale.x / scaleStep, minScale);
            target.localScale = new Vector3(s, s, s);
        }

        void OnDelete()
        {
            if (target != null)
            {
                Destroy(target.gameObject);
                target = null;
            }
            if (ARUIManager.Instance != null)
                ARUIManager.Instance.ShowNotification("Model silindi");
            gameObject.SetActive(false);
        }
    }
}
