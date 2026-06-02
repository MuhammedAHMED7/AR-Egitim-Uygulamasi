using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// Bir kumandanın hemen üstünde duran küçük ipucu paneli.
    /// Bağlama göre kısa metin gösterir (örn. "Trigger: Tut", "A: Menü").
    /// Kullanıcı yeni eylemleri öğrenirken kumandaya baktığında görünür.
    ///
    /// Bootstrapper bunu her kumandaya çocuk olarak yerleştirir; uzaklık ve
    /// dönüş offset'i ayarlanabilir.
    /// </summary>
    public class VRTooltipController : MonoBehaviour
    {
        [Header("Görünürlük")]
        [Tooltip("İpucu sadece kullanıcı kumandaya bakıyorsa görünür mü?")]
        public bool onlyWhenLookedAt = true;

        [Tooltip("Bakış açısı eşiği (dot product). Yüksek = daha keskin bakış gerekir.")]
        [Range(0f, 1f)] public float lookDotThreshold = 0.55f;

        [Tooltip("İpucu mesajı bu süre boyunca üzerinde fareyle durulmazsa solar.")]
        public float autoHideAfter = 6f;

        [Header("İçerik")]
        [TextArea] public string defaultMessage = "Trigger: Seç  •  Grip: Tut";

        CanvasGroup _canvasGroup;
        TextMeshProUGUI _text;
        Transform _head;
        float _timeShown;
        bool _muted;

        protected void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            Build();
        }

        void Start()
        {
            if (VRUIManager.Instance != null)
                _head = VRUIManager.Instance.headTransform;
            if (_head == null && Camera.main != null) _head = Camera.main.transform;
            _timeShown = Time.unscaledTime;
        }

        void LateUpdate()
        {
            if (_head == null || _muted)
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 0f, Time.unscaledDeltaTime * 6f);
                return;
            }

            // Otomatik gizleme süresi geçti mi?
            bool timedOut = (Time.unscaledTime - _timeShown) > autoHideAfter;

            float target = 1f;
            if (onlyWhenLookedAt)
            {
                Vector3 toController = (transform.position - _head.position).normalized;
                float dot = Vector3.Dot(_head.forward, toController);
                target = dot >= lookDotThreshold ? 1f : 0f;
            }
            if (timedOut) target = 0f;

            // Herhangi bir UI paneli açıksa kapat — odak dağıtmasın
            if (VRUIManager.Instance != null && VRUIManager.Instance.IsAnyPanelOpen)
                target = 0f;

            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, target,
                                            Time.unscaledDeltaTime * 8f);
        }

        public void SetMessage(string msg)
        {
            if (_text != null) _text.text = msg;
            _timeShown = Time.unscaledTime;
        }

        /// <summary>Tooltip'i tamamen kapat — başka bir UI açıkken vb.</summary>
        public void Mute(bool mute)
        {
            _muted = mute;
        }

        public void ResetShowTimer()
        {
            _timeShown = Time.unscaledTime;
        }

        void Build()
        {
            VRUIFactory.CreatePanelBackground(transform, VRUITheme.PanelBackgroundDim);

            var msgGO = new GameObject("Message", typeof(RectTransform));
            msgGO.transform.SetParent(transform, false);
            var rt = (RectTransform)msgGO.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12, 8);
            rt.offsetMax = new Vector2(-12, -8);
            _text = msgGO.AddComponent<TextMeshProUGUI>();
            _text.text = defaultMessage;
            _text.fontSize = VRUITheme.FontTooltip;
            _text.color = VRUITheme.TextPrimary;
            _text.alignment = TextAlignmentOptions.Center;
            _text.enableWordWrapping = true;
            _text.raycastTarget = false;
        }
    }
}
