using System;
using System.Collections.Generic;
using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>
    /// VR Kullanıcı Arayüzü Merkezi Koordinatörü. Singleton.
    /// Tüm VR UI bileşenleri arasındaki iletişimi yönetir, panellerin
    /// gösterim/gizleme durumunu izler, olay yayını yapar.
    ///
    /// VRInteractionManager hareket/etkileşim mantığını;
    /// VRUIManager ise arayüz görünürlüğünü ve kullanıcı geri bildirimini yönetir.
    /// </summary>
    public class VRUIManager : MonoBehaviour
    {
        public static VRUIManager Instance { get; private set; }

        // ───────── Olaylar ─────────
        public event Action<bool> OnMainMenuToggled;
        public event Action<bool> OnSettingsToggled;
        public event Action<bool> OnHelpToggled;
        public event Action OnExitRequested;
        public event Action OnRecenterRequested;

        // ───────── Referanslar (Bootstrapper tarafından atanır) ─────────
        [Header("UI Bileşenleri (runtime'da atanır)")]
        public VRWristMenu wristMenu;
        public VRMainMenuPanel mainMenu;
        public VRSettingsPanel settingsPanel;
        public VRNotificationController notifications;
        public VRTooltipController leftTooltip;
        public VRTooltipController rightTooltip;
        public VRRadialMenu radialMenu;
        public VRPointerVisuals leftPointerVisuals;
        public VRPointerVisuals rightPointerVisuals;
        public VRHelpPanel helpPanel;

        [Header("Sahne referansları")]
        public Transform headTransform;          // VR kamerası — yön referansı
        public Transform leftControllerTransform;
        public Transform rightControllerTransform;

        // ───────── Durum ─────────
        public bool IsMainMenuOpen { get; private set; }
        public bool IsSettingsOpen { get; private set; }
        public bool IsHelpOpen { get; private set; }
        public bool IsAnyPanelOpen => IsMainMenuOpen || IsSettingsOpen || IsHelpOpen;

        // Onboarding ipuçları sadece bir kez gösterilsin
        const string K_TIPS_SHOWN = "vr_tips_shown";
        public bool HasShownInitialTips
        {
            get => PlayerPrefs.GetInt(K_TIPS_SHOWN, 0) == 1;
            set { PlayerPrefs.SetInt(K_TIPS_SHOWN, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            // Etkileşim olaylarını dinle — kullanıcıya geri bildirim sağla
            VRInteractionManager.OnObjectGrabbed += HandleObjectGrabbed;
            VRInteractionManager.OnObjectReleased += HandleObjectReleased;
            VRInteractionManager.OnObjectUsed += HandleObjectUsed;
            VRInteractionManager.OnMovementModeChanged += HandleMovementModeChanged;

            // İlk açılışta yardım ipuçlarını göster
            if (!HasShownInitialTips && notifications != null)
            {
                // Biraz gecikme — kullanıcı sahnenin yüklendiğini hissetsin
                Invoke(nameof(ShowWelcomeTips), 1.0f);
            }
        }

        void OnDestroy()
        {
            VRInteractionManager.OnObjectGrabbed -= HandleObjectGrabbed;
            VRInteractionManager.OnObjectReleased -= HandleObjectReleased;
            VRInteractionManager.OnObjectUsed -= HandleObjectUsed;
            VRInteractionManager.OnMovementModeChanged -= HandleMovementModeChanged;
            if (Instance == this) Instance = null;
        }

        // ───────── Panel kontrolü ─────────
        public void ToggleMainMenu()
        {
            if (IsMainMenuOpen) CloseMainMenu();
            else OpenMainMenu();
        }

        public void OpenMainMenu()
        {
            if (IsMainMenuOpen) return;
            // Diğer panelleri kapat — sadece bir tane görünür kalsın
            CloseSettings();
            CloseHelp();

            IsMainMenuOpen = true;
            if (mainMenu != null)
            {
                mainMenu.PositionInFrontOfUser();
                mainMenu.Show();
            }
            OnMainMenuToggled?.Invoke(true);
        }

        public void CloseMainMenu()
        {
            if (!IsMainMenuOpen) return;
            IsMainMenuOpen = false;
            if (mainMenu != null) mainMenu.Hide();
            OnMainMenuToggled?.Invoke(false);
        }

        public void OpenSettings()
        {
            CloseMainMenu();
            CloseHelp();
            IsSettingsOpen = true;
            if (settingsPanel != null)
            {
                settingsPanel.PositionInFrontOfUser();
                settingsPanel.Show();
                settingsPanel.RefreshFromManager();
            }
            OnSettingsToggled?.Invoke(true);
        }

        public void CloseSettings()
        {
            if (!IsSettingsOpen) return;
            IsSettingsOpen = false;
            if (settingsPanel != null) settingsPanel.Hide();
            OnSettingsToggled?.Invoke(false);
        }

        public void OpenHelp()
        {
            CloseMainMenu();
            CloseSettings();
            IsHelpOpen = true;
            if (helpPanel != null)
            {
                helpPanel.PositionInFrontOfUser();
                helpPanel.Show();
            }
            OnHelpToggled?.Invoke(true);
        }

        public void CloseHelp()
        {
            if (!IsHelpOpen) return;
            IsHelpOpen = false;
            if (helpPanel != null) helpPanel.Hide();
            OnHelpToggled?.Invoke(false);
        }

        public void RequestExit()
        {
            OnExitRequested?.Invoke();
            ShowNotification("Uygulamadan çıkılıyor…", VRNotificationController.NotificationType.Info);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void RequestRecenter()
        {
            OnRecenterRequested?.Invoke();
            ShowNotification("Görüş yeniden ortalandı", VRNotificationController.NotificationType.Success);
        }

        // ───────── Bildirim Yardımcıları ─────────
        public void ShowNotification(string message, VRNotificationController.NotificationType type = VRNotificationController.NotificationType.Info)
        {
            if (notifications != null) notifications.Show(message, type);
        }

        // ───────── Etkileşim olaylarını yakala — UI geri bildirimi üret ─────────
        void HandleObjectGrabbed(VRGrabbable obj)
        {
            if (obj == null) return;
            ShowNotification($"{obj.displayName} tutuldu", VRNotificationController.NotificationType.Info);
            if (!string.IsNullOrEmpty(obj.grabHint))
                ShowNotification(obj.grabHint, VRNotificationController.NotificationType.Hint);
        }

        void HandleObjectReleased(VRGrabbable obj)
        {
            // Çok sık olduğu için bildirim yok — sessiz ol
        }

        void HandleObjectUsed(VRGrabbable obj)
        {
            if (obj == null) return;
            ShowNotification($"{obj.displayName} kullanıldı", VRNotificationController.NotificationType.Success);
        }

        void HandleMovementModeChanged(VRInteractionManager.MovementMode mode)
        {
            string label = mode switch
            {
                VRInteractionManager.MovementMode.Teleport => "Hareket modu: Işınlanma",
                VRInteractionManager.MovementMode.Continuous => "Hareket modu: Sürekli",
                VRInteractionManager.MovementMode.Both => "Hareket modu: Karışık",
                _ => "Hareket modu güncellendi"
            };
            ShowNotification(label, VRNotificationController.NotificationType.Info);
        }

        void ShowWelcomeTips()
        {
            ShowNotification("AR Eğitim VR'a hoş geldin", VRNotificationController.NotificationType.Success);
            // Birkaç saniye sonra ikinci ipucu
            Invoke(nameof(ShowMenuTip), 3.5f);
        }

        void ShowMenuTip()
        {
            ShowNotification("Menüyü açmak için Menu (sol) düğmesine bas", VRNotificationController.NotificationType.Hint);
            Invoke(nameof(ShowWristTip), 4f);
        }

        void ShowWristTip()
        {
            ShowNotification("Hızlı erişim: sol bileği çevir", VRNotificationController.NotificationType.Hint);
            HasShownInitialTips = true;
        }
    }
}
