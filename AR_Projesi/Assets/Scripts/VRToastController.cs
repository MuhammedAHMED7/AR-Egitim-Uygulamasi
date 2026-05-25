using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.VR
{
    /// <summary>
    /// Sağ üst köşede (kullanıcının görüş alanının üst-sağında) geçici mesajlar gösterir.
    /// Çok hızlı yanıp sönmemesi için kuyruğa alır.
    /// </summary>
    public class VRToastController : MonoBehaviour
    {
        [Header("Bağlantılar")]
        public Transform headTransform;
        public CanvasGroup canvasGroup;
        public Text messageText;
        public Image background;

        [Header("Konum")]
        [Tooltip("Kameranın önündeki ofset (x sağ, y yukarı, z ileri)")]
        public Vector3 offset = new Vector3(0.4f, 0.3f, 1.2f);

        [Tooltip("Toast başı takip etsin mi?")]
        public bool followHead = true;

        [Tooltip("Toast'un kullanıcıya bakma yumuşatması")]
        [Range(1f, 20f)] public float followSpeed = 8f;

        [Header("Renkler")]
        public Color infoColor    = new Color(0.2f, 0.5f, 0.9f, 0.95f);
        public Color successColor = new Color(0.2f, 0.7f, 0.3f, 0.95f);
        public Color warningColor = new Color(0.95f, 0.7f, 0.2f, 0.95f);
        public Color errorColor   = new Color(0.9f, 0.3f, 0.3f, 0.95f);

        [Header("Animasyon")]
        [Range(0.05f, 1f)] public float fadeInDuration = 0.18f;
        [Range(0.05f, 1f)] public float fadeOutDuration = 0.25f;

        private struct ToastRequest
        {
            public string message;
            public float duration;
            public ToastType type;
        }

        private readonly Queue<ToastRequest> _queue = new Queue<ToastRequest>();
        private bool _showing;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            gameObject.SetActive(true);
        }

        private void LateUpdate()
        {
            if (!followHead || headTransform == null) return;

            Vector3 targetPos = headTransform.position
                              + headTransform.right   * offset.x
                              + headTransform.up      * offset.y
                              + headTransform.forward * offset.z;

            transform.position = Vector3.Lerp(transform.position, targetPos,
                Time.deltaTime * followSpeed);

            Vector3 lookDir = transform.position - headTransform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target,
                    Time.deltaTime * followSpeed);
            }
        }

        public void Show(string message, float duration = 2.5f, ToastType type = ToastType.Info)
        {
            _queue.Enqueue(new ToastRequest { message = message, duration = duration, type = type });
            if (!_showing) StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            _showing = true;
            while (_queue.Count > 0)
            {
                var req = _queue.Dequeue();
                yield return ShowOne(req);
            }
            _showing = false;
        }

        private IEnumerator ShowOne(ToastRequest req)
        {
            if (messageText != null) messageText.text = req.message;
            if (background != null)  background.color = ColorFor(req.type);

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(req.duration);
            yield return Fade(1f, 0f, fadeOutDuration);
        }

        private IEnumerator Fade(float from, float to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(from, to, t / dur);
                if (canvasGroup != null) canvasGroup.alpha = a;
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = to;
        }

        private Color ColorFor(ToastType type)
        {
            return type switch
            {
                ToastType.Success => successColor,
                ToastType.Warning => warningColor,
                ToastType.Error   => errorColor,
                _ => infoColor,
            };
        }
    }
}
