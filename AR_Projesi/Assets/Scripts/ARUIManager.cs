using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// AR Arayüz Ana Yöneticisi
/// Sahneye eklediğin boş bir GameObject'e bu scripti sürükle.
/// </summary>
public class ARUIManager : MonoBehaviour
{
    [Header("AR Foundation")]
    public ARRaycastManager   raycastManager;
    public ARPlaneManager     planeManager;
    public Camera             arCamera;

    [Header("Model Prefabları")]
    public GameObject[] modelPrefabs;
    public Sprite[]     modelIcons;
    public string[]     modelNames;

    [Header("Carousel")]
    public Transform   carouselContent;
    public GameObject  carouselItemPrefab;

    [Header("HUD Üst Bar")]
    public TextMeshProUGUI modelNameText;
    public Button          settingsButton;

    [Header("Aksiyon Butonları")]
    public Button placeButton;
    public Button resetButton;
    public Button voiceButton;

    [Header("World Space Panel")]
    public GameObject worldSpacePanel;
    public Button     rotateBtn;
    public Button     scalePlusBtn;
    public Button     scaleMinusBtn;
    public Button     deleteBtn;

    [Header("Ayarlar Paneli")]
    public GameObject      settingsPanel;
    public Button          settingsCloseBtn;
    public Toggle          lightEstimationToggle;
    public Toggle          occlusionToggle;
    public Toggle          spatialAudioToggle;
    public Toggle          voiceCommandToggle;

    [Header("Onboarding")]
    public GameObject      onboardingOverlay;
    public Slider          scanProgressSlider;
    public Button          onboardingStartBtn;
    public TextMeshProUGUI onboardingText;

    [Header("Reticle")]
    public GameObject reticlePrefab;

    [Header("Bildirim")]
    public GameObject      notificationPanel;
    public TextMeshProUGUI notificationText;

    // ---------- Dahili değişkenler ----------
    private int                _selectedModel = 0;
    private float              _modelScale    = 1f;
    private bool               _isSpinning    = false;
    private GameObject         _placedModel;
    private GameObject         _reticleInstance;
    private List<ARRaycastHit> _hits          = new List<ARRaycastHit>();
    private Coroutine          _notifRoutine;
    private List<GameObject>   _carouselItems = new List<GameObject>();

    // =====================================================
    void Start()
    {
        BuildCarousel();
        BindButtons();

        if (worldSpacePanel) worldSpacePanel.SetActive(false);
        if (settingsPanel)   settingsPanel.SetActive(false);

        if (reticlePrefab)
            _reticleInstance = Instantiate(reticlePrefab);

        StartCoroutine(RunOnboarding());
    }

    // =====================================================
    void Update()
    {
        UpdateReticle();
        BillboardWorldPanel();

        if (_isSpinning && _placedModel)
            _placedModel.transform.Rotate(Vector3.up, 60f * Time.deltaTime);
    }

    // =====================================================
    #region Carousel

    void BuildCarousel()
    {
        if (!carouselContent || !carouselItemPrefab) return;

        foreach (var old in _carouselItems)
            if (old) Destroy(old);
        _carouselItems.Clear();

        for (int i = 0; i < modelNames.Length; i++)
        {
            int idx  = i;
            var item = Instantiate(carouselItemPrefab, carouselContent);
            var ci   = item.GetComponent<CarouselItem>();
            if (ci != null)
            {
                ci.Setup(modelIcons[i], modelNames[i], idx == _selectedModel);
                ci.OnClicked = () => SelectModel(idx);
            }
            _carouselItems.Add(item);
        }
    }

    void SelectModel(int index)
    {
        _selectedModel = index;
        _modelScale    = 1f;
        _isSpinning    = false;
        BuildCarousel();

        if (modelNameText)
            modelNameText.text = modelNames[index] + ".glb";

        ShowNotification(modelNames[index] + " secildi");
    }

    #endregion

    // =====================================================
    #region Buton Bağlama

    void BindButtons()
    {
        placeButton?.onClick.AddListener(PlaceModel);
        resetButton?.onClick.AddListener(ResetModel);
        voiceButton?.onClick.AddListener(SimulateVoice);

        settingsButton?.onClick.AddListener(()   => settingsPanel?.SetActive(true));
        settingsCloseBtn?.onClick.AddListener(() => settingsPanel?.SetActive(false));

        onboardingStartBtn?.onClick.AddListener(FinishOnboarding);

        rotateBtn?.onClick.AddListener(ToggleRotate);
        scalePlusBtn?.onClick.AddListener(ScaleUp);
        scaleMinusBtn?.onClick.AddListener(ScaleDown);
        deleteBtn?.onClick.AddListener(DeleteModel);

        // Toggle bildirimleri
        lightEstimationToggle?.onValueChanged.AddListener(v =>
            ShowNotification("Isik tahmini: " + (v ? "acik" : "kapali")));
        occlusionToggle?.onValueChanged.AddListener(v =>
            ShowNotification("Okluzyon: " + (v ? "acik" : "kapali")));
        spatialAudioToggle?.onValueChanged.AddListener(v =>
            ShowNotification("Uzamsal ses: " + (v ? "acik" : "kapali")));
        voiceCommandToggle?.onValueChanged.AddListener(v =>
            ShowNotification("Ses komutu: " + (v ? "acik" : "kapali")));
    }

    #endregion

    // =====================================================
    #region Model İşlemleri

    void PlaceModel()
    {
        if (_reticleInstance == null || !_reticleInstance.activeSelf)
        {
            ShowNotification("Once zemini tarat!");
            return;
        }

        if (_placedModel) Destroy(_placedModel);

        _placedModel = Instantiate(
            modelPrefabs[_selectedModel],
            _reticleInstance.transform.position,
            _reticleInstance.transform.rotation
        );
        _placedModel.transform.localScale = Vector3.one * _modelScale;

        if (worldSpacePanel)
        {
            worldSpacePanel.SetActive(true);
            PositionWorldPanel();
        }

        ShowNotification("Model yerlestirildi");
    }

    void ResetModel()
    {
        _modelScale  = 1f;
        _isSpinning  = false;
        if (_placedModel)
            _placedModel.transform.localScale = Vector3.one;
        ShowNotification("Sifirlandi");
    }

    void ToggleRotate()
    {
        _isSpinning = !_isSpinning;
        ShowNotification(_isSpinning ? "Dondurme acik" : "Dondurme durdu");
    }

    void ScaleUp()
    {
        if (_modelScale >= 1.8f) return;
        _modelScale = Mathf.Round((_modelScale + 0.2f) * 10f) / 10f;
        if (_placedModel)
            _placedModel.transform.localScale = Vector3.one * _modelScale;
        ShowNotification("Buyutuldu: %" + Mathf.RoundToInt(_modelScale * 100));
    }

    void ScaleDown()
    {
        if (_modelScale <= 0.4f) return;
        _modelScale = Mathf.Round((_modelScale - 0.2f) * 10f) / 10f;
        if (_placedModel)
            _placedModel.transform.localScale = Vector3.one * _modelScale;
        ShowNotification("Kucultuldu: %" + Mathf.RoundToInt(_modelScale * 100));
    }

    void DeleteModel()
    {
        if (_placedModel) Destroy(_placedModel);
        if (worldSpacePanel) worldSpacePanel.SetActive(false);
        _modelScale = 1f;
        ShowNotification("Model silindi");
    }

    void SimulateVoice()
    {
        ShowNotification("Ses komutu bekleniyor...");
        StartCoroutine(VoiceRoutine());
    }

    IEnumerator VoiceRoutine()
    {
        yield return new WaitForSeconds(2f);
        ScaleUp();
        ShowNotification("Komut algilandi: Buyut");
    }

    #endregion

    // =====================================================
    #region Reticle

    void UpdateReticle()
    {
        if (!_reticleInstance || !raycastManager) return;

        var center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(center, _hits, TrackableType.PlaneWithinPolygon))
        {
            _reticleInstance.SetActive(true);
            _reticleInstance.transform.SetPositionAndRotation(
                _hits[0].pose.position,
                _hits[0].pose.rotation
            );
        }
        else
        {
            _reticleInstance.SetActive(false);
        }
    }

    void PositionWorldPanel()
    {
        if (!_placedModel || !worldSpacePanel) return;
        worldSpacePanel.transform.position =
            _placedModel.transform.position + Vector3.right * 0.35f + Vector3.up * 0.2f;
    }

    #endregion

    // =====================================================
    #region Billboard

    void BillboardWorldPanel()
    {
        if (!worldSpacePanel || !worldSpacePanel.activeSelf || !arCamera) return;

        worldSpacePanel.transform.LookAt(
            worldSpacePanel.transform.position + arCamera.transform.rotation * Vector3.forward,
            arCamera.transform.rotation * Vector3.up
        );
    }

    #endregion

    // =====================================================
    #region Onboarding

    IEnumerator RunOnboarding()
    {
        if (!onboardingOverlay) yield break;
        onboardingOverlay.SetActive(true);

        if (onboardingText)
            onboardingText.text = "Kameranizi yavascа hareket ettirerek zemini tараyin.";

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 3f;
            if (scanProgressSlider) scanProgressSlider.value = t;
            yield return null;
        }

        if (onboardingText)
            onboardingText.text = "Zemin bulundu! Baslayabilirsiniz.";
    }

    void FinishOnboarding()
    {
        if (onboardingOverlay) onboardingOverlay.SetActive(false);
        ShowNotification("Zemin tarama tamamlandi");
    }

    #endregion

    // =====================================================
    #region Bildirim

    public void ShowNotification(string msg)
    {
        if (!notificationPanel || !notificationText) return;
        if (_notifRoutine != null) StopCoroutine(_notifRoutine);
        _notifRoutine = StartCoroutine(NotifRoutine(msg));
    }

    IEnumerator NotifRoutine(string msg)
    {
        notificationText.text = msg;
        notificationPanel.SetActive(true);

        var cg = notificationPanel.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.alpha = 0f;
            float t = 0f;
            while (t < 0.3f) { t += Time.deltaTime; cg.alpha = t / 0.3f; yield return null; }
        }

        yield return new WaitForSeconds(2f);

        if (cg)
        {
            float t = 0f;
            while (t < 0.3f) { t += Time.deltaTime; cg.alpha = 1f - t / 0.3f; yield return null; }
        }

        notificationPanel.SetActive(false);
    }

    #endregion
}
