using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AREducationApp
{
    /// <summary>
    /// Öğrenme Yönetim Sistemi (LMS - Learning Management System) entegrasyonu için temel ağ iletişim sınıfı.
    /// Öğrencinin uygulamadaki ilerlemesini ve hangi modelleri incelediğini bir sunucuya gönderir.
    /// (Bu sınıf opsiyonel bir API taslağıdır.)
    /// </summary>
    public class LMSIntegration : MonoBehaviour
    {
        public static LMSIntegration Instance { get; private set; }

        [Header("API Ayarları")]
        public string ServerURL = "https://api.lms-sisteminiz.com/progress";
        public string StudentID = "STD-12345"; // Normalde giriş ekranından alınır

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Öğrencinin bir konuyu (modeli) incelediğini LMS sunucusuna bildirir.
        /// </summary>
        /// <param name="contentName">İncelenen konunun adı (Örn: Hücre)</param>
        public void ReportProgress(string contentName)
        {
            StartCoroutine(SendProgressCoroutine(contentName));
        }

        private IEnumerator SendProgressCoroutine(string contentName)
        {
            // Gönderilecek veriyi JSON formatına hazırlayalım
            string jsonBody = $"{{\"studentId\":\"{StudentID}\", \"contentViewed\":\"{contentName}\", \"timestamp\":\"{System.DateTime.UtcNow.ToString("o")}\"}}";

            using (UnityWebRequest request = new UnityWebRequest(ServerURL, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                // İsteği gönder ve bekle
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"[LMS Hata] Veri gönderilemedi: {request.error}");
                }
                else
                {
                    Debug.Log($"[LMS Başarılı] '{contentName}' konusu için ilerleme kaydedildi.");
                }
            }
        }
    }
}
