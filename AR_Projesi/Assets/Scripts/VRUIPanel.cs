using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.VR
{
    /// <summary>
    /// Dünya uzayında duran VR panelleri için ortak davranış.
    /// Fade in/out, kullanıcıya doğru konumlandırma, isteğe bağlı billboard.
    ///
    /// Türetilmiş paneller Build() metodlarında içeriği oluşturur ve
    /// PositionInFrontOfUser() ile konumlandırma yaparlar.
    /// </summary>
    public abstract class VRUIPanel : MonoBehaviour
    {
        [Header("Davranış")]
        [Tooltip("Açıldığında kullanıcının önüne konumlansın mı?")]
        public bool repositionOnShow = true;

        [Tooltip("Her karede kullanıcıya bakacak şekilde dönsün mü?")]
        public bool billboardToUser = false;

        [Tooltip("Açıldığında / kapandığında fade animasyonu uygulansın mı?")]
        public bool useFadeAnimation = true;

        [Tooltip("Açıkken kullanıcı uzaklaştığında otomatik kapansın mı?")]
        public bool autoCloseWhenFar = false;

        [Tooltip("Otomatik kapanma mesafesi.")]
        public float autoCloseDistance = 5f;

        protected CanvasGroup _canvasGroup;
        protected Canvas _canvas;
        protected Coroutine _fadeRoutine;
        protected bool _isVisible;

        public bool IsVisible => _isVisible;

        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        protected virtual void Update()
        {
            if (!_isVisible) return;

            if (billboardToUser)
            {
                var head = HeadTransform();
                if (head != null) FaceTowards(head.position);
            }

            if (autoCloseWhenFar)
            {
                var head = HeadTransform();
                if (head != null && Vector3.Distance(transform.position, head.position) > autoCloseDistance)
                {
                    Hide();
                }
            }
        }

        public virtual void Show()
        {
            if (_isVisible) return;
            _isVisible = true;
            gameObject.SetActive(true);
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            if (useFadeAnimation) _fadeRoutine = StartCoroutine(FadeTo(1f));
            else _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            if (!_isVisible) return;
            _isVisible = false;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            if (useFadeAnimation) _fadeRoutine = StartCoroutine(FadeTo(0f, deactivateOnComplete: true));
            else
            {
                _canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        }

        IEnumerator FadeTo(float target, bool deactivateOnComplete = false)
        {
            float start = _canvasGroup.alpha;
            float t = 0f;
            float dur = VRUITheme.FadeDuration;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(start, target, t / dur);
                yield return null;
            }
            _canvasGroup.alpha = target;
            if (deactivateOnComplete) gameObject.SetActive(false);
            _fadeRoutine = null;
        }

        /// <summary>
        /// Paneli kullanıcının önüne, belirtilen mesafe ve yükseklikte konumlandırır.
        /// </summary>
        public virtual void PositionInFrontOfUser(float distance = VRUITheme.MainMenuDistance,
                                                  float heightFromFloor = VRUITheme.MainMenuHeightFromFloor)
        {
            var head = HeadTransform();
            if (head == null) return;

            // Kafanın baktığı yatay yön
            Vector3 forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            forward.Normalize();

            // Paneli kullanıcının önüne, sabit yere göre yükseklikte yerleştir
            Vector3 origin = new Vector3(head.position.x, 0f, head.position.z);
            transform.position = origin + forward * distance + Vector3.up * heightFromFloor;

            // Yüzü kullanıcıya çevir
            FaceTowards(head.position);
        }

        protected void FaceTowards(Vector3 worldPoint)
        {
            Vector3 toViewer = worldPoint - transform.position;
            toViewer.y = 0f;
            if (toViewer.sqrMagnitude < 0.0001f) return;
            // Canvas önyüzü +Z bakar; kullanıcıya çevirmek için -toViewer yönüne bakmalı
            transform.rotation = Quaternion.LookRotation(-toViewer.normalized, Vector3.up);
        }

        protected Transform HeadTransform()
        {
            var m = VRUIManager.Instance;
            if (m != null && m.headTransform != null) return m.headTransform;
            var cam = Camera.main;
            return cam != null ? cam.transform : null;
        }
    }
}
