using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// Tutulabilir VR nesnelerinin üzerinde, billboard mantığıyla duran
    /// küçük bilgi paneli. Nesnenin adı + bağlam ipucu metnini gösterir.
    /// Kullanıcı yaklaşınca açılır, uzaklaşınca solar.
    ///
    /// VRGrabbable üzerinde bu bileşen yoksa Bootstrapper otomatik ekler.
    /// </summary>
    [RequireComponent(typeof(VRGrabbable))]
    public class VRInfoPanel : MonoBehaviour
    {
        [Header("Görünürlük")]
        [Tooltip("Bilgi panelinin görünmeye başladığı mesafe (m).")]
        public float showDistance = 1.2f;

        [Tooltip("Tamamen görünmesi gereken mesafe (m). Daha yakınsa tam belirir.")]
        public float fullVisibleDistance = 0.6f;

        [Tooltip("Panelin nesneye göre yüksekliği (m).")]
        public float verticalOffset = 0.22f;

        [Tooltip("Nesne tutuluyorken her zaman görünür.")]
        public bool alwaysVisibleWhenHeld = true;

        VRGrabbable _grab;
        Transform _head;
        GameObject _panelRoot;
        Canvas _canvas;
        CanvasGroup _canvasGroup;
        TextMeshProUGUI _title;
        TextMeshProUGUI _hint;
        bool _isHeld;

        void Awake()
        {
            _grab = GetComponent<VRGrabbable>();
            _grab.selectEntered.AddListener(_ => _isHeld = true);
            _grab.selectExited.AddListener(_ => _isHeld = false);
            BuildPanel();
        }

        void Start()
        {
            if (VRUIManager.Instance != null)
                _head = VRUIManager.Instance.headTransform;
            if (_head == null && Camera.main != null) _head = Camera.main.transform;
            UpdateContent();
        }

        void BuildPanel()
        {
            _panelRoot = new GameObject("InfoPanel", typeof(RectTransform));
            _panelRoot.transform.SetParent(transform, false);
            _panelRoot.transform.localPosition = Vector3.up * verticalOffset;

            _canvas = _panelRoot.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.sortingOrder = 80;
            var scaler = _panelRoot.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 2f;
            scaler.referencePixelsPerUnit = 100f;
            var raycaster = _panelRoot.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
            var rt = _panelRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(VRUITheme.InfoPanelWidth, VRUITheme.InfoPanelHeight);
            // World-space ölçek 1:1000
            _panelRoot.transform.localScale = Vector3.one * VRUITheme.CanvasScale;

            _canvasGroup = _panelRoot.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            VRUIFactory.CreatePanelBackground(_panelRoot.transform);

            _title = VRUIFactory.CreateText(_panelRoot.transform, _grab.displayName,
                VRUITheme.FontHeading, VRUITheme.Accent,
                TextAlignmentOptions.Top, "Title");
            var trt = _title.rectTransform;
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -16);
            trt.sizeDelta = new Vector2(-32, 60);
            _title.fontStyle = FontStyles.Bold;

            _hint = VRUIFactory.CreateText(_panelRoot.transform,
                string.IsNullOrEmpty(_grab.grabHint) ? "Grip ile tut, Trigger ile kullan" : _grab.grabHint,
                VRUITheme.FontBody, VRUITheme.TextSecondary,
                TextAlignmentOptions.Center, "Hint");
            var hrt = _hint.rectTransform;
            hrt.anchorMin = new Vector2(0, 0);
            hrt.anchorMax = new Vector2(1, 0);
            hrt.pivot = new Vector2(0.5f, 0);
            hrt.anchoredPosition = new Vector2(0, 16);
            hrt.sizeDelta = new Vector2(-32, 120);
        }

        public void UpdateContent()
        {
            if (_title != null) _title.text = _grab.displayName;
            if (_hint != null)
                _hint.text = string.IsNullOrEmpty(_grab.grabHint)
                    ? "Grip: Tut  •  Trigger: Kullan"
                    : _grab.grabHint;
        }

        void LateUpdate()
        {
            if (_head == null || _panelRoot == null) return;

            // Konum: nesnenin üzerinde sabit offset
            _panelRoot.transform.position = transform.position + Vector3.up * verticalOffset;

            // Yüzü kameraya çevir
            Vector3 toHead = _head.position - _panelRoot.transform.position;
            toHead.y = 0f;
            if (toHead.sqrMagnitude > 0.0001f)
                _panelRoot.transform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);

            // Mesafeye göre alpha (yakın = görünür)
            float dist = Vector3.Distance(_head.position, transform.position);
            float target;
            if (_isHeld && alwaysVisibleWhenHeld) target = 1f;
            else if (dist <= fullVisibleDistance) target = 1f;
            else if (dist >= showDistance) target = 0f;
            else target = 1f - Mathf.InverseLerp(fullVisibleDistance, showDistance, dist);

            // Eğer herhangi bir ana UI paneli açıksa info paneli kapat
            if (VRUIManager.Instance != null && VRUIManager.Instance.IsAnyPanelOpen && !_isHeld)
                target = 0f;

            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, target, Time.unscaledDeltaTime * 6f);
        }
    }
}
