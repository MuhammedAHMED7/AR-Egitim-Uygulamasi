using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.VR
{
    /// <summary>
    /// Dünya uzayında duran VR panellerinin temel sınıfı. Her panel kendi Canvas'ını
    /// barındırır; VRUIManager bu panelleri açıp kapatır, gerekirse takip ettirir.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class VRFloatingPanel : MonoBehaviour
    {
        [Header("Görünüm")]
        [Tooltip("Panel açıldığında kullanıcının başını sürekli takip etsin mi?")]
        public bool followHead = false;

        [Tooltip("Panel yüksekliği — başın altı/üstü için ofset (metre).")]
        public float verticalOffset = 0f;

        [Header("Açılış Animasyonu")]
        [Tooltip("Açıldığında küçükten büyüğe scale animasyonu uygulansın mı?")]
        public bool useOpenAnimation = true;

        [Tooltip("Açılış animasyon süresi (saniye)")]
        [Range(0.05f, 1f)] public float openDuration = 0.18f;

        [Header("İçerik")]
        [Tooltip("Panel başlığı (header text varsa atayın)")]
        public Text titleText;

        private Vector3 _targetScale;
        private float _animTimer;
        private bool _animating;

        protected virtual void Awake()
        {
            _targetScale = transform.localScale;
            // VR'da world-space Canvas zorunlu.
            var canvas = GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            {
                canvas.renderMode = RenderMode.WorldSpace;
            }
        }

        public virtual void OnPanelOpened()
        {
            if (useOpenAnimation)
            {
                transform.localScale = _targetScale * 0.01f;
                _animTimer = 0f;
                _animating = true;
            }
            else
            {
                transform.localScale = _targetScale;
            }
        }

        public virtual void OnPanelClosed()
        {
            _animating = false;
            transform.localScale = _targetScale;
        }

        protected virtual void Update()
        {
            if (!_animating) return;

            _animTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_animTimer / openDuration);
            // Kolay bir ease-out: 1 - (1 - t)^3
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.Lerp(_targetScale * 0.01f, _targetScale, eased);

            if (t >= 1f) _animating = false;
        }

        public void SetTitle(string title)
        {
            if (titleText != null) titleText.text = title;
        }
    }
}
