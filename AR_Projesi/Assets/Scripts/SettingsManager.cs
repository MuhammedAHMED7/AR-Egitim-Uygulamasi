using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ayarlar panelini yönetir.
/// Canvas/SettingsPanel objesine ekle.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("AR Bileşenleri")]
    public AROcclusionManager occlusionManager;
    public ARCameraManager    cameraManager;

    [Header("Toggle'lar")]
    public Toggle lightEstimationToggle;
    public Toggle occlusionToggle;

    [Header("Kalite")]
    public Slider          qualitySlider;
    public TextMeshProUGUI qualityLabel;

    [Header("Bildirim")]
    public ARUIManager uiManager;

    private static readonly string[] QualityLabels =
        { "Cok dusuk", "Dusuk", "Orta", "Yuksek", "Ultra" };

    void Start()
    {
        // Başlangıç değerleri
        if (lightEstimationToggle) lightEstimationToggle.isOn = true;
        if (occlusionToggle)       occlusionToggle.isOn       = true;
        if (qualitySlider)         qualitySlider.value        = 2;

        // Listener bağla
        lightEstimationToggle?.onValueChanged.AddListener(SetLightEstimation);
        occlusionToggle?.onValueChanged.AddListener(SetOcclusion);
        qualitySlider?.onValueChanged.AddListener(SetQuality);

        UpdateQualityLabel(2);
    }

    void SetLightEstimation(bool value)
    {
        if (cameraManager)
            cameraManager.requestedLightEstimation = value
                ? LightEstimation.AmbientColor | LightEstimation.AmbientIntensity
                : LightEstimation.None;

        uiManager?.ShowNotification("Isik tahmini: " + (value ? "acik" : "kapali"));
    }

    void SetOcclusion(bool value)
    {
        if (occlusionManager)
            occlusionManager.requestedHumanDepthMode = value
                ? HumanSegmentationDepthMode.Fastest
                : HumanSegmentationDepthMode.Disabled;

        uiManager?.ShowNotification("Okluzyon: " + (value ? "acik" : "kapali"));
    }

    void SetQuality(float value)
    {
        int index = Mathf.RoundToInt(value);
        QualitySettings.SetQualityLevel(index);
        UpdateQualityLabel(index);
    }

    void UpdateQualityLabel(int index)
    {
        if (qualityLabel)
            qualityLabel.text = QualityLabels[Mathf.Clamp(index, 0, QualityLabels.Length - 1)];
    }
}
