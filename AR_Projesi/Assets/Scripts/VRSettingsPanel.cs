using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.VR
{
    /// <summary>
    /// VR ayarları paneli. VRInteractionManager ve VRMovementController ile çift yönlü
    /// senkronize çalışır: kullanıcı UI'dan değişiklik yapar, manager bunları uygular.
    /// </summary>
    public class VRSettingsPanel : VRFloatingPanel
    {
        [Header("Hareket Ayarları")]
        public Toggle continuousMoveToggle;
        public Toggle teleportToggle;
        public Slider moveSpeedSlider;
        public Text moveSpeedValueText;

        [Header("Dönüş Ayarları")]
        public Toggle snapTurnToggle;
        public Toggle smoothTurnToggle;
        public Slider snapAngleSlider;
        public Text snapAngleValueText;
        public Slider smoothTurnSpeedSlider;
        public Text smoothTurnSpeedValueText;

        [Header("Konfor Ayarları")]
        public Toggle vignetteToggle;
        public Slider uiScaleSlider;
        public Text uiScaleValueText;

        [Header("Genel")]
        public Button closeButton;
        public Button resetDefaultsButton;

        protected override void Awake()
        {
            base.Awake();

            if (closeButton != null)
                closeButton.onClick.AddListener(() => VRUIManager.Instance?.ClosePanel(this));

            if (resetDefaultsButton != null)
                resetDefaultsButton.onClick.AddListener(ResetDefaults);
        }

        public override void OnPanelOpened()
        {
            base.OnPanelOpened();
            PullFromManager();
            BindEvents();
        }

        public override void OnPanelClosed()
        {
            UnbindEvents();
            base.OnPanelClosed();
        }

        // Manager -> UI
        private void PullFromManager()
        {
            var mgr = VRInteractionManager.Instance;
            if (mgr == null) return;

            if (continuousMoveToggle != null) continuousMoveToggle.isOn = mgr.continuousMoveEnabled;
            if (teleportToggle != null)       teleportToggle.isOn       = mgr.teleportEnabled;
            if (moveSpeedSlider != null)
            {
                moveSpeedSlider.value = mgr.moveSpeed;
                UpdateText(moveSpeedValueText, mgr.moveSpeed, "0.0");
            }

            if (snapTurnToggle != null)   snapTurnToggle.isOn   = mgr.turnMode == VRTurnMode.Snap;
            if (smoothTurnToggle != null) smoothTurnToggle.isOn = mgr.turnMode == VRTurnMode.Smooth;
            if (snapAngleSlider != null)
            {
                snapAngleSlider.value = mgr.snapTurnAngle;
                UpdateText(snapAngleValueText, mgr.snapTurnAngle, "0") ;
            }
            if (smoothTurnSpeedSlider != null)
            {
                smoothTurnSpeedSlider.value = mgr.smoothTurnSpeed;
                UpdateText(smoothTurnSpeedValueText, mgr.smoothTurnSpeed, "0");
            }

            if (vignetteToggle != null) vignetteToggle.isOn = mgr.comfortVignetteEnabled;
            if (uiScaleSlider != null)
            {
                uiScaleSlider.value = mgr.uiScale;
                UpdateText(uiScaleValueText, mgr.uiScale * 100f, "0", "%");
            }
        }

        // UI -> Manager
        private void BindEvents()
        {
            if (continuousMoveToggle != null) continuousMoveToggle.onValueChanged.AddListener(OnContinuousMove);
            if (teleportToggle != null)       teleportToggle.onValueChanged.AddListener(OnTeleport);
            if (moveSpeedSlider != null)      moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeed);

            if (snapTurnToggle != null)       snapTurnToggle.onValueChanged.AddListener(OnSnapTurn);
            if (smoothTurnToggle != null)     smoothTurnToggle.onValueChanged.AddListener(OnSmoothTurn);
            if (snapAngleSlider != null)      snapAngleSlider.onValueChanged.AddListener(OnSnapAngle);
            if (smoothTurnSpeedSlider != null) smoothTurnSpeedSlider.onValueChanged.AddListener(OnSmoothTurnSpeed);

            if (vignetteToggle != null) vignetteToggle.onValueChanged.AddListener(OnVignette);
            if (uiScaleSlider != null)  uiScaleSlider.onValueChanged.AddListener(OnUIScale);
        }

        private void UnbindEvents()
        {
            if (continuousMoveToggle != null) continuousMoveToggle.onValueChanged.RemoveListener(OnContinuousMove);
            if (teleportToggle != null)       teleportToggle.onValueChanged.RemoveListener(OnTeleport);
            if (moveSpeedSlider != null)      moveSpeedSlider.onValueChanged.RemoveListener(OnMoveSpeed);

            if (snapTurnToggle != null)       snapTurnToggle.onValueChanged.RemoveListener(OnSnapTurn);
            if (smoothTurnToggle != null)     smoothTurnToggle.onValueChanged.RemoveListener(OnSmoothTurn);
            if (snapAngleSlider != null)      snapAngleSlider.onValueChanged.RemoveListener(OnSnapAngle);
            if (smoothTurnSpeedSlider != null) smoothTurnSpeedSlider.onValueChanged.RemoveListener(OnSmoothTurnSpeed);

            if (vignetteToggle != null) vignetteToggle.onValueChanged.RemoveListener(OnVignette);
            if (uiScaleSlider != null)  uiScaleSlider.onValueChanged.RemoveListener(OnUIScale);
        }

        // ---- Olay işleyicileri ----
        private void OnContinuousMove(bool v)
        {
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.continuousMoveEnabled = v;
            mgr.ApplySettings();
        }
        private void OnTeleport(bool v)
        {
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.teleportEnabled = v;
            mgr.ApplySettings();
        }
        private void OnMoveSpeed(float v)
        {
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.moveSpeed = v;
            mgr.ApplySettings();
            UpdateText(moveSpeedValueText, v, "0.0");
        }

        private void OnSnapTurn(bool v)
        {
            if (!v) return;
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.turnMode = VRTurnMode.Snap;
            if (smoothTurnToggle != null) smoothTurnToggle.SetIsOnWithoutNotify(false);
            mgr.ApplySettings();
        }
        private void OnSmoothTurn(bool v)
        {
            if (!v) return;
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.turnMode = VRTurnMode.Smooth;
            if (snapTurnToggle != null) snapTurnToggle.SetIsOnWithoutNotify(false);
            mgr.ApplySettings();
        }
        private void OnSnapAngle(float v)
        {
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.snapTurnAngle = v;
            mgr.ApplySettings();
            UpdateText(snapAngleValueText, v, "0");
        }
        private void OnSmoothTurnSpeed(float v)
        {
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.smoothTurnSpeed = v;
            mgr.ApplySettings();
            UpdateText(smoothTurnSpeedValueText, v, "0");
        }

        private void OnVignette(bool v)
        {
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.comfortVignetteEnabled = v;
            mgr.ApplySettings();
        }

        private void OnUIScale(float v)
        {
            var mgr = VRInteractionManager.Instance; if (mgr == null) return;
            mgr.uiScale = v;
            mgr.ApplySettings();
            UpdateText(uiScaleValueText, v * 100f, "0", "%");
            // UI scale doğrudan VRUIManager'ın panellerine yansıtılabilir:
            ApplyUIScaleToPanels(v);
        }

        private void ApplyUIScaleToPanels(float scale)
        {
            var ui = VRUIManager.Instance;
            if (ui == null) return;
            // Sadece floating paneller; bilek menüsü kumandaya yapışık olduğu için hariç.
            if (ui.mainMenuPanel != null) ScaleKeepRatio(ui.mainMenuPanel.transform, scale);
            if (ui.settingsPanel != null) ScaleKeepRatio(ui.settingsPanel.transform, scale);
            if (ui.infoPanel != null)     ScaleKeepRatio(ui.infoPanel.transform, scale);
        }

        private static void ScaleKeepRatio(Transform t, float factor)
        {
            // Baz scale 1 olarak varsayılıyor (world-space canvas 0.001 birimle çalışıyor olabilir,
            // bu durumda sadece çarpan olarak değiştiriyoruz)
            t.localScale = Vector3.one * factor;
        }

        private void ResetDefaults()
        {
            var mgr = VRInteractionManager.Instance;
            if (mgr == null) return;
            mgr.ResetToDefaults();
            mgr.ApplySettings();
            PullFromManager();
            VRUIManager.Instance?.ShowToast("Ayarlar varsayılana döndü", 2f, ToastType.Success);
        }

        private static void UpdateText(Text t, float v, string fmt, string suffix = "")
        {
            if (t != null) t.text = v.ToString(fmt) + suffix;
        }
    }
}
