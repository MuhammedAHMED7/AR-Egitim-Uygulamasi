using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.VR
{
    /// <summary>
    /// Lazerin gösterdiği obje üzerinde küçük bir bilgi balonu gösterir.
    /// VRRayHoverHinter veya başka scriptler bunu çağırabilir.
    /// </summary>
    public class VRTooltipController : MonoBehaviour
    {
        [Header("Bağlantılar")]
        public CanvasGroup canvasGroup;
        public Text label;
        public Transform headTransform;

        [Header("Davranış")]
        [Tooltip("Tooltip her zaman kameraya bakar (billboard)")]
        public bool billboard = true;

        [Range(1f, 20f)] public float fadeSpeed = 12f;
        [Tooltip("Tooltip dünyada hedef konumun üstünde gösterilir (metre).")]
        public float verticalOffset = 0.15f;

        private bool _visible;
        private float _targetAlpha;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, _targetAlpha,
                    Time.deltaTime * fadeSpeed);
            }

            if (billboard && headTransform != null && _visible)
            {
                Vector3 dir = transform.position - headTransform.position;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        public void Show(string text, Vector3 worldPos)
        {
            if (label != null) label.text = text;
            transform.position = worldPos + Vector3.up * verticalOffset;
            _visible = true;
            _targetAlpha = 1f;
        }

        public void Hide()
        {
            _visible = false;
            _targetAlpha = 0f;
        }
    }
}
