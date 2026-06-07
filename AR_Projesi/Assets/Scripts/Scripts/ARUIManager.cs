using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.UI
{
    /// <summary>
    /// AR Kullanıcı Arayüzü Merkezi Koordinatörü.
    /// Tüm UI bileşenleri arasındaki iletişimi yönetir.
    /// Singleton pattern kullanılır.
    /// </summary>
    public class ARUIManager : MonoBehaviour
    {
        public static ARUIManager Instance { get; private set; }

        // ───────── Model verisi ─────────
        [Serializable]
        public class ModelData
        {
            public string id;
            public string displayName;
            public Color accentColor = Color.white;
            // Gerçek bir prefab atanmazsa runtime'da basit bir primitive oluşturulur
            public GameObject prefab;
        }

        [Header("Model Kütüphanesi")]
        public List<ModelData> availableModels = new List<ModelData>();

        // ───────── Olay (Event) sistemi ─────────
        public event Action<ModelData> OnModelSelected;
        public event Action<ModelData> OnModelPlaceRequested;
        public event Action OnResetRequested;
        public event Action OnVoiceCommandRequested;
        public event Action<bool> OnSettingsToggled;

        // ───────── UI referansları ─────────
        [Header("UI Bileşenleri (Runtime'da atanır)")]
        public TopBarController topBar;
        public CarouselController carousel;
        public ActionBarController actionBar;
        public SettingsPanelController settingsPanel;
        public NotificationController notifications;
        public OnboardingController onboarding;
        public ReticleController reticle;

        // ───────── Durum ─────────
        public ModelData CurrentModel { get; private set; }
        public bool IsSessionReady { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Varsayılan model kütüphanesi (kullanıcı kendi prefab'larını sonradan ekleyebilir)
            if (availableModels == null || availableModels.Count == 0)
            {
                availableModels = new List<ModelData>
                {
                    new ModelData { id = "cube",     displayName = "Küp",     accentColor = new Color(0.30f, 0.78f, 1.00f) },
                    new ModelData { id = "sphere",   displayName = "Küre",    accentColor = new Color(1.00f, 0.55f, 0.30f) },
                    new ModelData { id = "cylinder", displayName = "Silindir",accentColor = new Color(0.60f, 0.95f, 0.50f) },
                    new ModelData { id = "capsule",  displayName = "Kapsül",  accentColor = new Color(0.95f, 0.45f, 0.85f) },
                    new ModelData { id = "tree",     displayName = "Ağaç",    accentColor = new Color(0.40f, 0.85f, 0.55f) }
                };
            }
        }

        void Start()
        {
            // İlk modeli varsayılan olarak seç
            if (availableModels.Count > 0)
            {
                SelectModel(availableModels[0]);
            }
        }

        // ───────── Public API ─────────
        public void SelectModel(ModelData model)
        {
            if (model == null) return;
            CurrentModel = model;
            OnModelSelected?.Invoke(model);
            ShowNotification($"Seçildi: {model.displayName}");
        }

        public void RequestPlace()
        {
            if (CurrentModel == null)
            {
                ShowNotification("Önce bir model seçin");
                return;
            }
            OnModelPlaceRequested?.Invoke(CurrentModel);
        }

        public void RequestReset()
        {
            OnResetRequested?.Invoke();
            ShowNotification("Sahne sıfırlandı");
        }

        public void RequestVoiceCommand()
        {
            OnVoiceCommandRequested?.Invoke();
            ShowNotification("Ses komutu dinleniyor...");
        }

        public void ToggleSettings(bool open)
        {
            OnSettingsToggled?.Invoke(open);
        }

        public void SetSessionReady(bool ready)
        {
            IsSessionReady = ready;
            if (topBar != null) topBar.SetSessionIndicator(ready);
        }

        public void ShowNotification(string message)
        {
            if (notifications != null) notifications.Show(message);
            else Debug.Log("[AR-UI] " + message);
        }
    }
}
