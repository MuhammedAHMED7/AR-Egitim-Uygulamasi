using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace AREgitim.VR
{
    /// <summary>
    /// Sol kumandanın bileğine (üstüne) yapışan küçük menü.
    /// Kullanıcı bileğini yüzüne çevirdiğinde otomatik açılır, çevirmeyince kapanır.
    /// Hızlı erişim butonları içerir (Ana Menü, Ayarlar, Yardım, Çıkış).
    /// </summary>
    public class VRWristMenu : MonoBehaviour
    {
        [Header("Bağlantılar")]
        public Transform attachTo;          // Sol kumanda
        public Transform headTransform;     // XR Camera
        public Canvas canvas;

        [Header("Görünürlük")]
        [Tooltip("Bileğin kameraya bakma yönü ile kamera arasındaki maksimum açı (derece).")]
        [Range(15f, 90f)] public float visibilityAngle = 50f;

        [Tooltip("Bileğin kameraya olan mesafesi şu değerden küçük olduğunda menü açılabilir.")]
        public float visibilityMaxDistance = 1.2f;

        [Tooltip("Açılma/kapanma yumuşatması")]
        [Range(1f, 20f)] public float fadeSpeed = 10f;

        [Header("Konum")]
        public Vector3 localPosition = new Vector3(0f, 0.04f, -0.02f);
        public Vector3 localEuler    = new Vector3(60f, 0f, 0f);
        public Vector3 localScale    = new Vector3(0.003f, 0.003f, 0.003f);

        [Header("Butonlar (atomatik bağlanır)")]
        public Button mainMenuButton;
        public Button settingsButton;
        public Button helpButton;
        public Button closeAllButton;

        [Header("Manuel Açma (opsiyonel)")]
        [Tooltip("Bu input action tetiklenirse menü zorla açılır (örn: sol kumanda menu button).")]
        public InputActionProperty toggleAction;

        private CanvasGroup _group;
        private float _currentAlpha;
        private bool _forceOpen;

        private void Awake()
        {
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (canvas != null) canvas.renderMode = RenderMode.WorldSpace;

            _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            if (toggleAction.action != null)
            {
                toggleAction.action.performed += OnToggle;
                toggleAction.action.Enable();
            }
            BindButtons();
        }

        private void OnDisable()
        {
            if (toggleAction.action != null)
                toggleAction.action.performed -= OnToggle;
        }

        private void OnToggle(InputAction.CallbackContext ctx)
        {
            _forceOpen = !_forceOpen;
        }

        private void BindButtons()
        {
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() =>
                {
                    if (VRUIManager.Instance != null && VRUIManager.Instance.mainMenuPanel != null)
                        VRUIManager.Instance.TogglePanel(VRUIManager.Instance.mainMenuPanel);
                });
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(() =>
                {
                    if (VRUIManager.Instance != null && VRUIManager.Instance.settingsPanel != null)
                        VRUIManager.Instance.TogglePanel(VRUIManager.Instance.settingsPanel);
                });
            }
            if (helpButton != null)
            {
                helpButton.onClick.RemoveAllListeners();
                helpButton.onClick.AddListener(() =>
                {
                    if (VRUIManager.Instance != null && VRUIManager.Instance.infoPanel != null)
                        VRUIManager.Instance.TogglePanel(VRUIManager.Instance.infoPanel);
                });
            }
            if (closeAllButton != null)
            {
                closeAllButton.onClick.RemoveAllListeners();
                closeAllButton.onClick.AddListener(() =>
                {
                    if (VRUIManager.Instance != null)
                        VRUIManager.Instance.CloseAllPanels();
                });
            }
        }

        private void LateUpdate()
        {
            if (attachTo == null) return;

            // Kumandaya yapış
            transform.SetPositionAndRotation(
                attachTo.TransformPoint(localPosition),
                attachTo.rotation * Quaternion.Euler(localEuler));
            transform.localScale = localScale;

            // Görünürlük hesaplama
            float targetAlpha = 0f;
            if (_forceOpen)
            {
                targetAlpha = 1f;
            }
            else if (headTransform != null)
            {
                Vector3 toHead = headTransform.position - transform.position;
                float dist = toHead.magnitude;
                if (dist <= visibilityMaxDistance)
                {
                    // Bilek menüsünün "yukarı"sı (Y ekseni) kameraya ne kadar bakıyor?
                    float angle = Vector3.Angle(transform.up, toHead.normalized);
                    if (angle < visibilityAngle) targetAlpha = 1f;
                }
            }

            _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            if (_group != null)
            {
                _group.alpha = _currentAlpha;
                bool active = _currentAlpha > 0.5f;
                _group.interactable = active;
                _group.blocksRaycasts = active;
            }
        }
    }
}
