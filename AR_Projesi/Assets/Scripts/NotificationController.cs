using System.Collections;
using UnityEngine;
using TMPro;

namespace AREgitim.UI
{
    /// <summary>
    /// Ekranın üstünde geçici bildirimler. Fade-in / fade-out animasyonlu.
    /// Yeni bildirim öncekini iptal eder.
    /// </summary>
    public class NotificationController : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public TMP_Text label;

        [Tooltip("Bildirimin tamamen görünür kaldığı süre (saniye)")]
        public float visibleDuration = 1.6f;
        [Tooltip("Fade in/out süresi (saniye)")]
        public float fadeDuration = 0.25f;

        Coroutine _routine;

        void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        public void Show(string message)
        {
            if (label != null) label.text = message;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FadeRoutine());
        }

        IEnumerator FadeRoutine()
        {
            if (canvasGroup == null) yield break;

            // Fade in
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(visibleDuration);

            // Fade out
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            _routine = null;
        }
    }
}
