using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace AREgitim.VR
{
    /// <summary>
    /// VR sahnesinin UI katmanını runtime'da oluşturur.
    /// VRSceneBootstrapper sahneyi (XR Origin, kumandalar, hareket) ayağa kaldırdıktan
    /// sonra çalışır; XR Origin'den head/controller referanslarını bulup tüm panelleri
    /// programatik olarak oluşturur. Editor'de prefab kurmaya gerek kalmaz.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public class VRUIBootstrapper : MonoBehaviour
    {
        [Header("Otomatik Kurulum")]
        public bool autoBuildOnStart = true;

        [Header("Stil")]
        public Color panelBackground   = new Color(0.08f, 0.10f, 0.14f, 0.95f);
        public Color panelHeader       = new Color(0.15f, 0.20f, 0.30f, 1f);
        public Color accentColor       = new Color(0.2f, 0.6f, 1f, 1f);
        public Color textPrimary       = Color.white;
        public Color textSecondary     = new Color(0.75f, 0.78f, 0.85f, 1f);
        public int   baseFontSize      = 28;

        [Header("Boyutlar (world units)")]
        [Tooltip("World-space canvas piksel-metre dönüşümü. 0.001 = 1 piksel 1 mm")]
        public float canvasUnitScale = 0.001f;

        public Vector2 mainMenuPanelSize  = new Vector2(900, 700);
        public Vector2 settingsPanelSize  = new Vector2(900, 900);
        public Vector2 infoPanelSize      = new Vector2(1000, 750);
        public Vector2 wristMenuSize      = new Vector2(450, 700);
        public Vector2 toastSize          = new Vector2(700, 140);
        public Vector2 tooltipSize        = new Vector2(500, 100);

        private VRUIManager _ui;

        private void Start()
        {
            if (autoBuildOnStart) BuildUI();
        }

        public void BuildUI()
        {
            EnsureEventSystem();

            var manager = FindOrCreateUIManager();
            _ui = manager;

            // Sahnedeki XR Origin'den head ve kumandaları bul
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("[VRUIBootstrapper] XR Origin sahnede yok. Önce VRSceneBootstrapper çalışmalı.");
                return;
            }
            _ui.headTransform = xrOrigin.Camera != null ? xrOrigin.Camera.transform : null;

            // Sol/Sağ kumanda transformlarını bul
            var controllers = xrOrigin.GetComponentsInChildren<XRBaseController>(true);
            foreach (var c in controllers)
            {
                string n = c.name.ToLowerInvariant();
                if (_ui.leftControllerTransform == null && (n.Contains("left") || n.Contains("sol")))
                    _ui.leftControllerTransform = c.transform;
                else if (_ui.rightControllerTransform == null && (n.Contains("right") || n.Contains("sağ") || n.Contains("sag")))
                    _ui.rightControllerTransform = c.transform;
            }
            // Yedek: sırasıyla 0 ve 1
            if (_ui.leftControllerTransform == null && controllers.Length > 0)
                _ui.leftControllerTransform = controllers[0].transform;
            if (_ui.rightControllerTransform == null && controllers.Length > 1)
                _ui.rightControllerTransform = controllers[1].transform;

            // Panelleri oluştur
            _ui.mainMenuPanel = BuildMainMenuPanel();
            _ui.settingsPanel = BuildSettingsPanel();
            _ui.infoPanel     = BuildInfoPanel();
            _ui.wristMenu     = BuildWristMenu(_ui.leftControllerTransform);
            _ui.toast         = BuildToast(_ui.headTransform);
            _ui.tooltip       = BuildTooltip(_ui.headTransform);

            // Başlangıçta paneller kapalı, sadece toast/tooltip ve bilek menüsü aktif kalır
            _ui.mainMenuPanel.gameObject.SetActive(false);
            _ui.settingsPanel.gameObject.SetActive(false);
            _ui.infoPanel.gameObject.SetActive(false);

            // Hoş geldin toast'u
            _ui.ShowToast("VR moduna hoş geldiniz. Sol bileğinizi kaldırın.", 4f, ToastType.Info);
        }

        // ----------------------- Yardımcı Kurulum -----------------------

        private void EnsureEventSystem()
        {
            var es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<EventSystem>();
            }

            // StandaloneInputModule varsa kaldır — XR ile çakışır.
            var sim = es.GetComponent<StandaloneInputModule>();
            if (sim != null) Destroy(sim);

            // XR Interaction Toolkit UI Input Module — ray interactor ile UI etkileşimi için zorunlu
            if (es.GetComponent<XRUIInputModule>() == null)
            {
                es.gameObject.AddComponent<XRUIInputModule>();
            }
        }

        private VRUIManager FindOrCreateUIManager()
        {
            var existing = FindObjectOfType<VRUIManager>();
            if (existing != null) return existing;
            var go = new GameObject("VR_UI_Manager");
            return go.AddComponent<VRUIManager>();
        }

        // ----------------------- Ortak Stil Yardımcıları -----------------------

        private GameObject NewWorldCanvas(string name, Vector2 sizeDelta, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            // XR Interaction Toolkit'in UI'a ulaşması için TrackedDeviceGraphicRaycaster lazım.
            go.AddComponent<TrackedDeviceGraphicRaycaster>();

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = sizeDelta;
            go.transform.localScale = Vector3.one * canvasUnitScale;
            return go;
        }

        private Image AddBackground(GameObject parent, Color color)
        {
            var img = parent.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private GameObject AddHeader(Transform parent, string title)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            var headerRt = headerGo.AddComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot     = new Vector2(0.5f, 1);
            headerRt.sizeDelta = new Vector2(0, 80);
            headerRt.anchoredPosition = Vector2.zero;

            var headerImg = headerGo.AddComponent<Image>();
            headerImg.color = panelHeader;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = baseFontSize + 8;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = textPrimary;
            titleText.alignment = TextAnchor.MiddleCenter;
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = new Vector2(-20, 0);
            return headerGo;
        }

        private GameObject AddTextLabel(Transform parent, string text, int fontSize, Color color,
                                        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
                                        TextAnchor align = TextAnchor.MiddleLeft, bool bold = false)
        {
            var go = new GameObject("Label_" + text.Substring(0, Mathf.Min(10, text.Length)));
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = align;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return go;
        }

        private Button AddButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject("Button_" + label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var img = go.AddComponent<Image>();
            img.color = accentColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor      = accentColor;
            colors.highlightedColor = Color.Lerp(accentColor, Color.white, 0.2f);
            colors.pressedColor     = Color.Lerp(accentColor, Color.black, 0.2f);
            colors.selectedColor    = colors.highlightedColor;
            colors.disabledColor    = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            btn.colors = colors;

            // VR davranışı (haptik + hover scale)
            go.AddComponent<VRUIButton>();

            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(go.transform, false);
            var labelText = labelGo.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = baseFontSize;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontStyle = FontStyle.Bold;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            return btn;
        }

        private Toggle AddToggle(Transform parent, string label, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject("Toggle_" + label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            // Arka plan (kutucuk)
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.5f);
            bgRt.anchorMax = new Vector2(0, 0.5f);
            bgRt.pivot     = new Vector2(0, 0.5f);
            bgRt.sizeDelta = new Vector2(40, 40);
            bgRt.anchoredPosition = new Vector2(0, 0);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // İşaret (check)
            var checkGo = new GameObject("Checkmark");
            checkGo.transform.SetParent(bgGo.transform, false);
            var checkRt = checkGo.AddComponent<RectTransform>();
            checkRt.anchorMin = Vector2.zero;
            checkRt.anchorMax = Vector2.one;
            checkRt.offsetMin = new Vector2(6, 6);
            checkRt.offsetMax = new Vector2(-6, -6);
            var checkImg = checkGo.AddComponent<Image>();
            checkImg.color = accentColor;

            // Etiket
            var labelGo = AddTextLabel(go.transform, label, baseFontSize, textPrimary,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(56, 0), new Vector2(-10, 0),
                TextAnchor.MiddleLeft);

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = false;
            return toggle;
        }

        private Slider AddSlider(Transform parent, Vector2 anchoredPos, Vector2 size,
                                 float min, float max, float value)
        {
            var go = new GameObject("Slider");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.4f);
            bgRt.anchorMax = new Vector2(1, 0.6f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // Fill area
            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(go.transform, false);
            var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0.4f);
            fillAreaRt.anchorMax = new Vector2(1, 0.6f);
            fillAreaRt.offsetMin = new Vector2(10, 0);
            fillAreaRt.offsetMax = new Vector2(-10, 0);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = accentColor;

            // Handle area
            var handleAreaGo = new GameObject("Handle Slide Area");
            handleAreaGo.transform.SetParent(go.transform, false);
            var handleAreaRt = handleAreaGo.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = new Vector2(0, 0);
            handleAreaRt.anchorMax = new Vector2(1, 1);
            handleAreaRt.offsetMin = new Vector2(15, 0);
            handleAreaRt.offsetMax = new Vector2(-15, 0);

            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            var handleRt = handleGo.AddComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(30, 50);
            var handleImg = handleGo.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.targetGraphic = handleImg;
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            return slider;
        }

        // ----------------------- Paneller -----------------------

        private VRMainMenuPanel BuildMainMenuPanel()
        {
            var go = NewWorldCanvas("VR_MainMenu", mainMenuPanelSize);
            AddBackground(go, panelBackground);
            AddHeader(go.transform, "ANA MENÜ");

            var startBtn    = AddButton(go.transform, "Derse Başla",      new Vector2(0, 130),  new Vector2(700, 90));
            var arBtn       = AddButton(go.transform, "AR Moduna Geç",    new Vector2(0, 20),   new Vector2(700, 90));
            var settingsBtn = AddButton(go.transform, "Ayarlar",          new Vector2(0, -90),  new Vector2(700, 90));
            var helpBtn     = AddButton(go.transform, "Yardım",           new Vector2(0, -200), new Vector2(700, 90));
            var quitBtn     = AddButton(go.transform, "Çıkış",            new Vector2(0, -310), new Vector2(700, 90));

            // Çıkış butonunu kırmızıya boya
            var quitImg = quitBtn.GetComponent<Image>();
            if (quitImg != null) quitImg.color = new Color(0.8f, 0.25f, 0.25f, 1f);

            var panel = go.AddComponent<VRMainMenuPanel>();
            panel.startLessonButton = startBtn;
            panel.switchToARButton  = arBtn;
            panel.settingsButton    = settingsBtn;
            panel.helpButton        = helpBtn;
            panel.quitButton        = quitBtn;
            return panel;
        }

        private VRSettingsPanel BuildSettingsPanel()
        {
            var go = NewWorldCanvas("VR_Settings", settingsPanelSize);
            AddBackground(go, panelBackground);
            AddHeader(go.transform, "AYARLAR");

            // Hareket
            AddTextLabel(go.transform, "HAREKET", baseFontSize, accentColor,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(40, -120), new Vector2(-40, -85),
                TextAnchor.MiddleLeft, bold: true);

            var contMove = AddToggle(go.transform, "Sürekli Hareket",  new Vector2(0, 240), new Vector2(700, 50));
            var teleport = AddToggle(go.transform, "Teleport",         new Vector2(0, 180), new Vector2(700, 50));

            AddTextLabel(go.transform, "Hareket Hızı", baseFontSize - 4, textSecondary,
                new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(40, 100), new Vector2(0, 140),
                TextAnchor.MiddleLeft);
            var moveSpeedVal = AddTextLabel(go.transform, "2.0", baseFontSize - 4, textPrimary,
                new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0, 100), new Vector2(-40, 140),
                TextAnchor.MiddleRight, bold: true).GetComponent<Text>();
            var moveSpeed = AddSlider(go.transform, new Vector2(0, 75), new Vector2(750, 40), 0.5f, 5f, 2.5f);

            // Dönüş
            AddTextLabel(go.transform, "DÖNÜŞ", baseFontSize, accentColor,
                new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(40, 0), new Vector2(-40, 30),
                TextAnchor.MiddleLeft, bold: true);

            var snapTurn   = AddToggle(go.transform, "Snap Turn",   new Vector2(-180, -45), new Vector2(340, 50));
            var smoothTurn = AddToggle(go.transform, "Smooth Turn", new Vector2(180,  -45), new Vector2(340, 50));

            AddTextLabel(go.transform, "Snap Açısı", baseFontSize - 4, textSecondary,
                new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(40, -120), new Vector2(0, -80),
                TextAnchor.MiddleLeft);
            var snapAngleVal = AddTextLabel(go.transform, "45", baseFontSize - 4, textPrimary,
                new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0, -120), new Vector2(-40, -80),
                TextAnchor.MiddleRight, bold: true).GetComponent<Text>();
            var snapAngle = AddSlider(go.transform, new Vector2(0, -145), new Vector2(750, 40), 15f, 90f, 45f);

            AddTextLabel(go.transform, "Smooth Turn Hızı", baseFontSize - 4, textSecondary,
                new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(40, -200), new Vector2(0, -160),
                TextAnchor.MiddleLeft);
            var smoothTurnVal = AddTextLabel(go.transform, "60", baseFontSize - 4, textPrimary,
                new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0, -200), new Vector2(-40, -160),
                TextAnchor.MiddleRight, bold: true).GetComponent<Text>();
            var smoothTurnSpeed = AddSlider(go.transform, new Vector2(0, -225), new Vector2(750, 40), 30f, 180f, 60f);

            // Konfor
            AddTextLabel(go.transform, "KONFOR", baseFontSize, accentColor,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(40, 200), new Vector2(-40, 230),
                TextAnchor.MiddleLeft, bold: true);

            var vignette = AddToggle(go.transform, "Konfor Vignette",  new Vector2(0, -325), new Vector2(700, 50));

            AddTextLabel(go.transform, "UI Boyutu", baseFontSize - 4, textSecondary,
                new Vector2(0, 0), new Vector2(0.5f, 0),
                new Vector2(40, 90), new Vector2(0, 130),
                TextAnchor.MiddleLeft);
            var uiScaleVal = AddTextLabel(go.transform, "100%", baseFontSize - 4, textPrimary,
                new Vector2(0.5f, 0), new Vector2(1f, 0),
                new Vector2(0, 90), new Vector2(-40, 130),
                TextAnchor.MiddleRight, bold: true).GetComponent<Text>();
            var uiScale = AddSlider(go.transform, new Vector2(0, 65), new Vector2(750, 40), 0.5f, 1.5f, 1f);

            // Alt butonlar
            var closeBtn = AddButton(go.transform, "Kapat",        new Vector2(-180, -410), new Vector2(330, 80));
            var resetBtn = AddButton(go.transform, "Varsayılana Dön", new Vector2(180, -410),  new Vector2(330, 80));

            var panel = go.AddComponent<VRSettingsPanel>();
            panel.continuousMoveToggle    = contMove;
            panel.teleportToggle          = teleport;
            panel.moveSpeedSlider         = moveSpeed;
            panel.moveSpeedValueText      = moveSpeedVal;
            panel.snapTurnToggle          = snapTurn;
            panel.smoothTurnToggle        = smoothTurn;
            panel.snapAngleSlider         = snapAngle;
            panel.snapAngleValueText      = snapAngleVal;
            panel.smoothTurnSpeedSlider   = smoothTurnSpeed;
            panel.smoothTurnSpeedValueText= smoothTurnVal;
            panel.vignetteToggle          = vignette;
            panel.uiScaleSlider           = uiScale;
            panel.uiScaleValueText        = uiScaleVal;
            panel.closeButton             = closeBtn;
            panel.resetDefaultsButton     = resetBtn;
            return panel;
        }

        private VRInfoPanel BuildInfoPanel()
        {
            var go = NewWorldCanvas("VR_Info", infoPanelSize);
            AddBackground(go, panelBackground);
            AddHeader(go.transform, "YARDIM & BİLGİ");

            // Tab container (üstte yatay bir şerit)
            var tabBarGo = new GameObject("TabBar");
            tabBarGo.transform.SetParent(go.transform, false);
            var tabBarRt = tabBarGo.AddComponent<RectTransform>();
            tabBarRt.anchorMin = new Vector2(0, 1);
            tabBarRt.anchorMax = new Vector2(1, 1);
            tabBarRt.pivot = new Vector2(0.5f, 1);
            tabBarRt.sizeDelta = new Vector2(-40, 70);
            tabBarRt.anchoredPosition = new Vector2(0, -90);

            var tabLayout = tabBarGo.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 10;
            tabLayout.childForceExpandWidth  = true;
            tabLayout.childForceExpandHeight = true;
            tabLayout.childControlWidth  = true;
            tabLayout.childControlHeight = true;

            // Tab button prefab (gizli, klonlanacak)
            var tabBtnGo = new GameObject("TabButtonPrefab");
            tabBtnGo.transform.SetParent(go.transform, false);
            tabBtnGo.SetActive(false);
            var tabBtnRt = tabBtnGo.AddComponent<RectTransform>();
            tabBtnRt.sizeDelta = new Vector2(200, 70);
            var tabBtnImg = tabBtnGo.AddComponent<Image>();
            tabBtnImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            var tabBtn = tabBtnGo.AddComponent<Button>();
            tabBtn.targetGraphic = tabBtnImg;
            tabBtnGo.AddComponent<VRUIButton>();

            var tabBtnLabelGo = new GameObject("Text");
            tabBtnLabelGo.transform.SetParent(tabBtnGo.transform, false);
            var tabBtnLabel = tabBtnLabelGo.AddComponent<Text>();
            tabBtnLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tabBtnLabel.fontSize = baseFontSize - 4;
            tabBtnLabel.color = Color.white;
            tabBtnLabel.alignment = TextAnchor.MiddleCenter;
            tabBtnLabel.fontStyle = FontStyle.Bold;
            var tabBtnLabelRt = tabBtnLabelGo.GetComponent<RectTransform>();
            tabBtnLabelRt.anchorMin = Vector2.zero;
            tabBtnLabelRt.anchorMax = Vector2.one;
            tabBtnLabelRt.offsetMin = Vector2.zero;
            tabBtnLabelRt.offsetMax = Vector2.zero;

            // İçerik başlığı
            var contentTitleGo = AddTextLabel(go.transform, "", baseFontSize + 4, accentColor,
                new Vector2(0, 0.5f), new Vector2(1, 0.5f),
                new Vector2(40, 120), new Vector2(-40, 160),
                TextAnchor.MiddleLeft, bold: true);
            var contentTitle = contentTitleGo.GetComponent<Text>();

            // İçerik metni
            var contentGo = AddTextLabel(go.transform, "", baseFontSize - 4, textPrimary,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(40, 100), new Vector2(-40, -180),
                TextAnchor.UpperLeft);
            var contentText = contentGo.GetComponent<Text>();

            // Sayfa göstergesi + ileri/geri
            var pageGo = AddTextLabel(go.transform, "1 / 1", baseFontSize - 6, textSecondary,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-100, 20), new Vector2(100, 60),
                TextAnchor.MiddleCenter);
            var pageText = pageGo.GetComponent<Text>();

            var prevBtn  = AddButton(go.transform, "<",  new Vector2(-280, 35), new Vector2(120, 70));
            var nextBtn  = AddButton(go.transform, ">",  new Vector2(280, 35),  new Vector2(120, 70));
            var closeBtn = AddButton(go.transform, "Kapat", new Vector2(0, -340), new Vector2(280, 70));

            var panel = go.AddComponent<VRInfoPanel>();
            panel.tabContainer    = tabBarRt;
            panel.tabButtonPrefab = tabBtn;
            panel.contentText     = contentText;
            panel.contentTitleText= contentTitle;
            panel.nextButton      = nextBtn;
            panel.prevButton      = prevBtn;
            panel.closeButton     = closeBtn;
            panel.pageIndicatorText = pageText;

            panel.tabs = new List<VRInfoPanel.InfoTab>
            {
                new VRInfoPanel.InfoTab {
                    title = "Başlangıç",
                    content =
                        "VR Uygulamasına Hoş Geldiniz!\n\n" +
                        "• Sol bileğinizi yüzünüze çevirerek hızlı menüyü açın.\n" +
                        "• Sağ kumanda lazerini kullanarak butonlara tıklayın.\n" +
                        "• İstediğiniz yere ışınlanmak için tetik tuşunu kullanın.\n" +
                        "• Sol joystick ile sürekli hareket edebilirsiniz."
                },
                new VRInfoPanel.InfoTab {
                    title = "Kumandalar",
                    content =
                        "KUMANDA REHBERİ\n\n" +
                        "Sağ Kumanda:\n" +
                        "  • Tetik: Seç / Tıkla\n" +
                        "  • Kavrama: Objeyi tut\n" +
                        "  • Joystick: Yön dön (snap/smooth)\n\n" +
                        "Sol Kumanda:\n" +
                        "  • Joystick: Yürüme yönü\n" +
                        "  • Tetik: Teleport hedef\n" +
                        "  • Menu: Bilek menüsünü aç"
                },
                new VRInfoPanel.InfoTab {
                    title = "Konfor",
                    content =
                        "KONFOR İPUÇLARI\n\n" +
                        "Eğer hareket sırasında mide bulantısı hissediyorsanız:\n" +
                        "• Snap Turn'ü etkinleştirin.\n" +
                        "• Konfor Vignette'i açın (hareket sırasında görüş alanı daralır).\n" +
                        "• Sürekli hareket yerine teleport tercih edin.\n" +
                        "• Kısa molalar verin."
                },
                new VRInfoPanel.InfoTab {
                    title = "Ders İçeriği",
                    content =
                        "EĞİTİM MODÜLLERİ\n\n" +
                        "Bu VR ortamı, çeşitli eğitim modüllerini destekler.\n" +
                        "Ana Menü'den 'Derse Başla' diyerek modül seçim ekranına ulaşabilirsiniz.\n\n" +
                        "Sahnedeki etkileşimli objeleri kavrayarak inceleyebilir, üzerlerindeki bilgi panellerini okuyabilirsiniz."
                }
            };

            return panel;
        }

        private VRWristMenu BuildWristMenu(Transform attach)
        {
            var go = NewWorldCanvas("VR_WristMenu", wristMenuSize);
            // Bilek menüsü kumandaya yapışacağı için scale farklı (daha küçük)
            go.transform.localScale = Vector3.one * 0.003f;
            AddBackground(go, panelBackground);
            AddHeader(go.transform, "HIZLI MENÜ");

            var mainBtn     = AddButton(go.transform, "Ana Menü",   new Vector2(0, 130),  new Vector2(380, 90));
            var settingsBtn = AddButton(go.transform, "Ayarlar",    new Vector2(0, 20),   new Vector2(380, 90));
            var helpBtn     = AddButton(go.transform, "Yardım",     new Vector2(0, -90),  new Vector2(380, 90));
            var closeBtn    = AddButton(go.transform, "Hepsini Kapat", new Vector2(0, -200), new Vector2(380, 90));

            var wrist = go.AddComponent<VRWristMenu>();
            wrist.attachTo       = attach;
            wrist.headTransform  = _ui != null ? _ui.headTransform : null;
            wrist.canvas         = go.GetComponent<Canvas>();
            wrist.mainMenuButton = mainBtn;
            wrist.settingsButton = settingsBtn;
            wrist.helpButton     = helpBtn;
            wrist.closeAllButton = closeBtn;
            return wrist;
        }

        private VRToastController BuildToast(Transform head)
        {
            var go = NewWorldCanvas("VR_Toast", toastSize);
            var bg = AddBackground(go, new Color(0.2f, 0.5f, 0.9f, 0.95f));

            var msgGo = AddTextLabel(go.transform, "", baseFontSize, Color.white,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(30, 0), new Vector2(-30, 0),
                TextAnchor.MiddleCenter, bold: true);

            var cg = go.AddComponent<CanvasGroup>();

            var toast = go.AddComponent<VRToastController>();
            toast.headTransform = head;
            toast.canvasGroup   = cg;
            toast.messageText   = msgGo.GetComponent<Text>();
            toast.background    = bg;
            return toast;
        }

        private VRTooltipController BuildTooltip(Transform head)
        {
            var go = NewWorldCanvas("VR_Tooltip", tooltipSize);
            AddBackground(go, new Color(0.05f, 0.07f, 0.12f, 0.92f));

            var labelGo = AddTextLabel(go.transform, "", baseFontSize - 4, Color.white,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(20, 10), new Vector2(-20, -10),
                TextAnchor.MiddleCenter, bold: false);

            var cg = go.AddComponent<CanvasGroup>();

            var tooltip = go.AddComponent<VRTooltipController>();
            tooltip.canvasGroup   = cg;
            tooltip.label         = labelGo.GetComponent<Text>();
            tooltip.headTransform = head;
            return tooltip;
        }
    }
}
