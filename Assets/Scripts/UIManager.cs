using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AREducationApp
{
    /// <summary>
    /// AR Eğitim Uygulamasının kullanıcı arayüzünü (UI) yönetir. 
    /// Modeller arası geçiş ve bilgi ekranlarını kontrol eder.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panelleri")]
        public GameObject MainMenuPanel;
        public GameObject ARViewPanel;
        public GameObject InfoPanel;

        [Header("UI Elemanları")]
        public Text InfoTitleText;
        public Text InfoDescriptionText;

        [Header("İçerik Veritabanı")]
        public EducationalContent[] AvailableContents;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            ShowMainMenu();
        }

        /// <summary>
        /// Ana Menüyü gösterir.
        /// </summary>
        public void ShowMainMenu()
        {
            MainMenuPanel.SetActive(true);
            ARViewPanel.SetActive(false);
            InfoPanel.SetActive(false);
        }

        /// <summary>
        /// Bir konuyu (Örn: Hücre, Güneş Sistemi) seçtiğimizde AR moduna geçer.
        /// </summary>
        /// <param name="contentIndex">AvailableContents dizisindeki indeks</param>
        public void SelectContent(int contentIndex)
        {
            if (contentIndex < 0 || contentIndex >= AvailableContents.Length) return;

            EducationalContent selectedContent = AvailableContents[contentIndex];

            // Seçilen modeli ObjectSpawner'a aktar
            if (ObjectSpawner.Instance != null)
            {
                ObjectSpawner.Instance.SetModelPrefab(selectedContent.ModelPrefab);
            }

            // AR Görüntüleme moduna geç
            MainMenuPanel.SetActive(false);
            ARViewPanel.SetActive(true);
            InfoPanel.SetActive(false);

            // Bilgi paneli verilerini doldur
            InfoTitleText.text = selectedContent.Title;
            InfoDescriptionText.text = selectedContent.Description;
        }

        /// <summary>
        /// AR ekranındayken model hakkında bilgi okumak için bilgi panelini açar/kapatır.
        /// </summary>
        public void ToggleInfoPanel()
        {
            InfoPanel.SetActive(!InfoPanel.activeSelf);
        }

        /// <summary>
        /// AR modundan çıkıp tekrar menüye döner.
        /// </summary>
        public void BackToMainMenu()
        {
            if (ARManager.Instance != null)
            {
                // AR Session'ı sıfırla ki eski düzlemler gitsin
                ARManager.Instance.ResetARSession();
            }

            ShowMainMenu();
        }
    }
}
