using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.UI
{
    /// <summary>
    /// Tek seferlik zemin tarama rehberi.
    /// Progress bar AR zemini tespit edildikçe ilerler.
    /// </summary>
    public class OnboardingController : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public TMP_Text titleText;
        public TMP_Text instructionText;
        public Slider progressBar;
        public Button startButton;

        [Tooltip("Zemin tarama simülasyon süresi (gerçek AR Foundation entegrasyonu için yeniden bağlanabilir)")]
        public float simulatedScanDuration = 3.5f;

        const string K_DONE = "ar_onboarding_done";

        void Start()
        {
            // Daha önce tamamlandıysa atla
            if (PlayerPrefs.GetInt(K_DONE, 0) == 1)
            {
                gameObject.SetActive(false);
                if (ARUIManager.Instance != null) ARUIManager.Instance.SetSessionReady(true);
                return;
            }

            if (progressBar != null)
            {
                progressBar.minValue = 0f;
                progressBar.maxValue = 1f;
                progressBar.value = 0f;
            }

            if (titleText != null) titleText.text = "Zemini Tarayın";
            if (instructionText != null) instructionText.text = "Kameranızı yavaşça hareket ettirin. Düz bir yüzeye doğrultun.";

            if (startButton != null)
            {
                startButton.interactable = false;
                startButton.onClick.AddListener(OnStart);
            }

            if (canvasGroup != null) canvasGroup.alpha = 1f;
            StartCoroutine(ScanRoutine());
        }

        IEnumerator ScanRoutine()
        {
            float t = 0f;
            while (t < simulatedScanDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / simulatedScanDuration);
                if (progressBar != null) progressBar.value = p;
                if (instructionText != null && p > 0.5f && p < 0.95f)
                    instructionText.text = "Harika gidiyorsunuz, devam edin...";
                yield return null;
            }

            if (instructionText != null) instructionText.text = "Zemin tespit edildi. Hazır olduğunuzda başlayın.";
            if (startButton != null) startButton.interactable = true;
        }

        void OnStart()
        {
            PlayerPrefs.SetInt(K_DONE, 1);
            PlayerPrefs.Save();
            StartCoroutine(FadeOut());
        }

        IEnumerator FadeOut()
        {
            float t = 0f;
            float dur = 0.4f;
            while (t < dur)
            {
                t += Time.deltaTime;
                if (canvasGroup != null) canvasGroup.alpha = 1f - (t / dur);
                yield return null;
            }
            gameObject.SetActive(false);
            if (ARUIManager.Instance != null)
            {
                ARUIManager.Instance.SetSessionReady(true);
                ARUIManager.Instance.ShowNotification("AR oturumu hazır");
            }
        }
    }
}
