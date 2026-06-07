using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.UI
{
    /// <summary>
    /// Ayarlar paneli — glassmorphism kaplama.
    /// Toggle'lar ve render kalitesi slider'ı içerir.
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public Toggle lightToggle;
        public Toggle occlusionToggle;
        public Toggle spatialAudioToggle;
        public Toggle voiceToggle;
        public Slider qualitySlider;
        public TMP_Text qualityLabel;
        public Button closeButton;

        bool _isOpen;

        void Start()
        {
            // Başlangıçta kapalı
            SetOpen(false, instant: true);

            // ARUIManager olaylarına bağlan
            if (ARUIManager.Instance != null)
                ARUIManager.Instance.OnSettingsToggled += SetOpen;

            // Mevcut ayarları UI'a yükle
            var sm = SettingsManager.Instance;
            if (sm != null)
            {
                if (lightToggle != null)        lightToggle.isOn        = sm.LightEstimationEnabled;
                if (occlusionToggle != null)    occlusionToggle.isOn    = sm.OcclusionEnabled;
                if (spatialAudioToggle != null) spatialAudioToggle.isOn = sm.SpatialAudioEnabled;
                if (voiceToggle != null)        voiceToggle.isOn        = sm.VoiceCommandEnabled;
                if (qualitySlider != null)
                {
                    qualitySlider.minValue = 1;
                    qualitySlider.maxValue = 5;
                    qualitySlider.wholeNumbers = true;
                    qualitySlider.value = sm.RenderQuality;
                    UpdateQualityLabel(sm.RenderQuality);
                }
            }

            // UI olaylarını bağla
            if (lightToggle != null)        lightToggle.onValueChanged.AddListener(OnLightChanged);
            if (occlusionToggle != null)    occlusionToggle.onValueChanged.AddListener(OnOcclusionChanged);
            if (spatialAudioToggle != null) spatialAudioToggle.onValueChanged.AddListener(OnSpatialChanged);
            if (voiceToggle != null)        voiceToggle.onValueChanged.AddListener(OnVoiceChanged);
            if (qualitySlider != null)      qualitySlider.onValueChanged.AddListener(OnQualityChanged);
            if (closeButton != null)        closeButton.onClick.AddListener(() => SetOpen(false));
        }

        void OnDestroy()
        {
            if (ARUIManager.Instance != null)
                ARUIManager.Instance.OnSettingsToggled -= SetOpen;
        }

        void SetOpen(bool open) => SetOpen(open, false);

        void SetOpen(bool open, bool instant)
        {
            _isOpen = open;
            if (canvasGroup == null) return;
            canvasGroup.alpha = open ? 1f : 0f;
            canvasGroup.interactable = open;
            canvasGroup.blocksRaycasts = open;
        }

        void OnLightChanged(bool v)
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetLightEstimation(v);
            if (ARUIManager.Instance != null) ARUIManager.Instance.ShowNotification(v ? "Işık tahmini açık" : "Işık tahmini kapalı");
        }

        void OnOcclusionChanged(bool v)
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetOcclusion(v);
            if (ARUIManager.Instance != null) ARUIManager.Instance.ShowNotification(v ? "Oklüzyon açık" : "Oklüzyon kapalı");
        }

        void OnSpatialChanged(bool v)
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetSpatialAudio(v);
            if (ARUIManager.Instance != null) ARUIManager.Instance.ShowNotification(v ? "Uzamsal ses açık" : "Uzamsal ses kapalı");
        }

        void OnVoiceChanged(bool v)
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetVoiceCommand(v);
            if (ARUIManager.Instance != null) ARUIManager.Instance.ShowNotification(v ? "Ses komutu açık" : "Ses komutu kapalı");
        }

        void OnQualityChanged(float v)
        {
            int q = Mathf.RoundToInt(v);
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetRenderQuality(q);
            UpdateQualityLabel(q);
        }

        void UpdateQualityLabel(int q)
        {
            if (qualityLabel == null) return;
            string label = q switch
            {
                1 => "Çok Düşük",
                2 => "Düşük",
                3 => "Orta",
                4 => "Yüksek",
                5 => "Çok Yüksek",
                _ => "Orta"
            };
            qualityLabel.text = $"Render Kalitesi: {label}";
        }
    }
}
