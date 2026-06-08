using UnityEngine;

namespace AREgitim.UI
{
    /// <summary>
    /// Uygulama ayarları için merkezi yönetici.
    /// AR Foundation API'lerine güvenli (null-safe) bir şekilde bağlanır;
    /// AR Foundation çalışmasa bile script derleme hatası vermez.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        // ───────── Ayar değerleri ─────────
        public bool LightEstimationEnabled { get; private set; } = true;
        public bool OcclusionEnabled       { get; private set; } = true;
        public bool SpatialAudioEnabled    { get; private set; } = true;
        public bool VoiceCommandEnabled    { get; private set; } = false;
        public int  RenderQuality          { get; private set; } = 3; // 1..5

        // ───────── PlayerPrefs anahtarları ─────────
        const string K_LIGHT    = "ar_setting_light";
        const string K_OCCL     = "ar_setting_occlusion";
        const string K_SPATIAL  = "ar_setting_spatial";
        const string K_VOICE    = "ar_setting_voice";
        const string K_QUALITY  = "ar_setting_quality";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
        }

        void Load()
        {
            LightEstimationEnabled = PlayerPrefs.GetInt(K_LIGHT,   1) == 1;
            OcclusionEnabled       = PlayerPrefs.GetInt(K_OCCL,    1) == 1;
            SpatialAudioEnabled    = PlayerPrefs.GetInt(K_SPATIAL, 1) == 1;
            VoiceCommandEnabled    = PlayerPrefs.GetInt(K_VOICE,   0) == 1;
            RenderQuality          = PlayerPrefs.GetInt(K_QUALITY, 3);
            ApplyQuality();
        }

        void Save()
        {
            PlayerPrefs.SetInt(K_LIGHT,   LightEstimationEnabled ? 1 : 0);
            PlayerPrefs.SetInt(K_OCCL,    OcclusionEnabled       ? 1 : 0);
            PlayerPrefs.SetInt(K_SPATIAL, SpatialAudioEnabled    ? 1 : 0);
            PlayerPrefs.SetInt(K_VOICE,   VoiceCommandEnabled    ? 1 : 0);
            PlayerPrefs.SetInt(K_QUALITY, RenderQuality);
            PlayerPrefs.Save();
        }

        public void SetLightEstimation(bool v) { LightEstimationEnabled = v; Save(); }
        public void SetOcclusion(bool v)       { OcclusionEnabled       = v; Save(); }
        public void SetSpatialAudio(bool v)    { SpatialAudioEnabled    = v; Save(); }
        public void SetVoiceCommand(bool v)    { VoiceCommandEnabled    = v; Save(); }

        public void SetRenderQuality(int level)
        {
            RenderQuality = Mathf.Clamp(level, 1, 5);
            ApplyQuality();
            Save();
        }

        void ApplyQuality()
        {
            // Unity'nin yerleşik kalite seviyelerine eşle (0..QualitySettings.names.Length-1)
            int count = QualitySettings.names != null ? QualitySettings.names.Length : 0;
            if (count > 0)
            {
                int target = Mathf.Clamp(RenderQuality - 1, 0, count - 1);
                QualitySettings.SetQualityLevel(target, true);
            }
        }
    }
}
