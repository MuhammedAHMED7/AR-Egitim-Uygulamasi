using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace AREgitim.UI
{
    /// <summary>
    /// Sahne açılır açılmaz tüm AR UI'ını programatik olarak inşa eder.
    /// Bu yaklaşım sayede sahne dosyası minimal kalır ve Unity'de açılırken
    /// kırılgan referans veya prefab GUID hatası oluşmaz.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class ARUIBootstrapper : MonoBehaviour
    {
        [Header("Renkler")]
        public Color colorBackgroundDim   = new Color(0f, 0f, 0f, 0.55f);
        public Color colorPanelTint       = new Color(0.06f, 0.07f, 0.10f, 0.78f);
        public Color colorAccent          = new Color(0.30f, 0.78f, 1f, 1f);
        public Color colorText            = new Color(1f, 1f, 1f, 0.95f);
        public Color colorSubtle          = new Color(1f, 1f, 1f, 0.55f);

        ARUIManager _manager;

        void Awake()
        {
            // EventSystem yoksa oluştur
            EnsureEventSystem();

            // SettingsManager yoksa oluştur
            if (SettingsManager.Instance == null)
            {
                var smGO = new GameObject("SettingsManager");
                smGO.transform.SetParent(transform, false);
                smGO.AddComponent<SettingsManager>();
            }

            // ARUIManager hazırla
            _manager = GetComponent<ARUIManager>();
            if (_manager == null) _manager = gameObject.AddComponent<ARUIManager>();

            // Ana Canvas
            var canvas = CreateMainCanvas();

            // Onboarding (en üstte, ilk gösterilen)
            var onboarding = CreateOnboarding(canvas.transform);
            _manager.onboarding = onboarding;

            // Bildirimler
            var notif = CreateNotifications(canvas.transform);
            _manager.notifications = notif;

            // Top bar
            var top = CreateTopBar(canvas.transform);
            _manager.topBar = top;

            // Carousel
            var car = CreateCarousel(canvas.transform);
            _manager.carousel = car;

            // Action bar
            var action = CreateActionBar(canvas.transform);
            _manager.actionBar = action;

            // Settings panel
            var settings = CreateSettingsPanel(canvas.transform);
            _manager.settingsPanel = settings;

            // Reticle (3D)
            var reticle = CreateReticle();
            _manager.reticle = reticle;

            // World-Space kontrol paneli
            var worldPanel = CreateWorldSpacePanel();

            // Touch gestures
            var gestures = gameObject.AddComponent<TouchGestureController>();

            // Placement controller
            var placement = gameObject.AddComponent<ModelPlacementController>();
            placement.reticle = reticle;
            placement.worldPanel = worldPanel;
            placement.gestureController = gestures;
            var placedRootGO = new GameObject("ARPlacedModels");
            placement.placedRoot = placedRootGO.transform;
        }

        // ────────────── EventSystem ──────────────
        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ────────────── Canvas ──────────────
        Canvas CreateMainCanvas()
        {
            var go = new GameObject("ARCanvas");
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ────────────── Top Bar ──────────────
        TopBarController CreateTopBar(Transform parent)
        {
            var go = NewUIObject("TopBar", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0, 140);
            rt.anchoredPosition = new Vector2(0, 0);

            // Arka plan
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.06f, 0.09f, 0.72f);

            // Session dot
            var dotGO = NewUIObject("SessionDot", go.transform);
            var dotRT = dotGO.GetComponent<RectTransform>();
            dotRT.anchorMin = new Vector2(0, 0.5f);
            dotRT.anchorMax = new Vector2(0, 0.5f);
            dotRT.pivot = new Vector2(0, 0.5f);
            dotRT.sizeDelta = new Vector2(24, 24);
            dotRT.anchoredPosition = new Vector2(48, 0);
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.sprite = CreateCircleSprite();
            dotImg.color = new Color(0.95f, 0.4f, 0.4f);

            // Model name label
            var labelGO = NewUIObject("ModelName", go.transform);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.offsetMin = new Vector2(96, 0);
            labelRT.offsetMax = new Vector2(-160, 0);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = "Model Yok";
            label.fontSize = 42;
            label.color = colorText;
            label.alignment = TextAlignmentOptions.Left;
            label.fontStyle = FontStyles.Bold;

            // Settings button
            var btnGO = NewUIObject("SettingsButton", go.transform);
            var btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(1, 0.5f);
            btnRT.anchorMax = new Vector2(1, 0.5f);
            btnRT.pivot = new Vector2(1, 0.5f);
            btnRT.sizeDelta = new Vector2(110, 90);
            btnRT.anchoredPosition = new Vector2(-32, 0);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(1f, 1f, 1f, 0.10f);
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelGO = NewUIObject("Icon", btnGO.transform);
            var btnLabelRT = btnLabelGO.GetComponent<RectTransform>();
            btnLabelRT.anchorMin = Vector2.zero;
            btnLabelRT.anchorMax = Vector2.one;
            btnLabelRT.offsetMin = Vector2.zero;
            btnLabelRT.offsetMax = Vector2.zero;
            var btnLabel = btnLabelGO.AddComponent<TextMeshProUGUI>();
            btnLabel.text = "⚙";
            btnLabel.fontSize = 52;
            btnLabel.color = colorText;
            btnLabel.alignment = TextAlignmentOptions.Center;

            var ctrl = go.AddComponent<TopBarController>();
            ctrl.modelNameLabel = label;
            ctrl.sessionDot = dotImg;
            ctrl.settingsButton = btn;
            return ctrl;
        }

        // ────────────── Carousel ──────────────
        CarouselController CreateCarousel(Transform parent)
        {
            var go = NewUIObject("Carousel", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, 220);
            rt.anchoredPosition = new Vector2(0, 260);

            // ScrollRect arka plan (Mask için Image gerekli — sprite atanmazsa varsayılan kullanılır)
            var scrollGO = NewUIObject("ScrollRect", go.transform);
            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(24, 12);
            scrollRT.offsetMax = new Vector2(-24, -12);
            var scrollImg = scrollGO.AddComponent<Image>();
            scrollImg.color = new Color(0.05f, 0.06f, 0.09f, 0.55f);

            var scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;

            // Mask
            scrollGO.AddComponent<Mask>().showMaskGraphic = true;

            // Viewport (content parent)
            var viewportGO = NewUIObject("Viewport", scrollGO.transform);
            var vpRT = viewportGO.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;

            // Content
            var contentGO = NewUIObject("Content", viewportGO.transform);
            var contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 0.5f);
            contentRT.anchorMax = new Vector2(0, 0.5f);
            contentRT.pivot = new Vector2(0, 0.5f);
            contentRT.sizeDelta = new Vector2(0, 180);
            var hlg = contentGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.padding = new RectOffset(16, 16, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;
            scroll.viewport = vpRT;

            // Item template (prefab gibi davranır)
            var template = BuildCarouselItemTemplate();

            var ctrl = go.AddComponent<CarouselController>();
            ctrl.itemContainer = contentRT;
            ctrl.itemPrefab = template;

            return ctrl;
        }

        GameObject BuildCarouselItemTemplate()
        {
            var go = new GameObject("CarouselItem", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(170, 170);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.08f);

            // Ring
            var ringGO = NewUIObject("Ring", go.transform);
            var ringRT = ringGO.GetComponent<RectTransform>();
            ringRT.anchorMin = new Vector2(0.5f, 0.65f);
            ringRT.anchorMax = new Vector2(0.5f, 0.65f);
            ringRT.pivot = new Vector2(0.5f, 0.5f);
            ringRT.sizeDelta = new Vector2(70, 70);
            ringRT.anchoredPosition = Vector2.zero;
            var ring = ringGO.AddComponent<Image>();
            ring.sprite = CreateCircleSprite();
            ring.color = colorAccent;

            // Label
            var labelGO = NewUIObject("Label", go.transform);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0.35f);
            labelRT.offsetMin = new Vector2(4, 4);
            labelRT.offsetMax = new Vector2(-4, 0);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = "—";
            label.fontSize = 28;
            label.alignment = TextAlignmentOptions.Center;
            label.color = colorText;

            var item = go.AddComponent<CarouselItem>();
            item.background = bg;
            item.iconRing = ring;
            item.label = label;

            // Sahnede aktif olmayan template
            go.SetActive(true);
            return go;
        }

        // ────────────── Action Bar ──────────────
        ActionBarController CreateActionBar(Transform parent)
        {
            var go = NewUIObject("ActionBar", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, 200);
            rt.anchoredPosition = new Vector2(0, 40);

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 32;
            hlg.padding = new RectOffset(64, 64, 24, 24);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var voiceBtn = BuildActionButton(go.transform, "🎤", "Ses", new Color(0.40f, 0.85f, 0.55f));
            var placeBtn = BuildActionButton(go.transform, "✚", "Yerleştir", colorAccent);
            var resetBtn = BuildActionButton(go.transform, "↻", "Sıfırla", new Color(0.95f, 0.55f, 0.30f));

            var ctrl = go.AddComponent<ActionBarController>();
            ctrl.voiceButton = voiceBtn;
            ctrl.placeButton = placeBtn;
            ctrl.resetButton = resetBtn;
            return ctrl;
        }

        Button BuildActionButton(Transform parent, string icon, string text, Color accent)
        {
            var go = NewUIObject(text + "Btn", parent);
            var img = go.AddComponent<Image>();
            img.color = new Color(accent.r, accent.g, accent.b, 0.18f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // İkon
            var iconGO = NewUIObject("Icon", go.transform);
            var iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.4f);
            iconRT.anchorMax = new Vector2(1, 1);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconText = iconGO.AddComponent<TextMeshProUGUI>();
            iconText.text = icon;
            iconText.fontSize = 56;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.color = colorText;

            // Etiket
            var labelGO = NewUIObject("Label", go.transform);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0.4f);
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 26;
            label.alignment = TextAlignmentOptions.Center;
            label.color = colorText;

            return btn;
        }

        // ────────────── Settings Panel ──────────────
        SettingsPanelController CreateSettingsPanel(Transform parent)
        {
            var go = NewUIObject("SettingsPanel", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Dim arka plan
            var dim = go.AddComponent<Image>();
            dim.color = colorBackgroundDim;

            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // Panel kart
            var cardGO = NewUIObject("Card", go.transform);
            var cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(880, 1100);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = colorPanelTint;

            // Başlık
            var titleGO = NewUIObject("Title", cardGO.transform);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 1);
            titleRT.sizeDelta = new Vector2(0, 96);
            titleRT.anchoredPosition = new Vector2(0, -36);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text = "Ayarlar";
            title.fontSize = 56;
            title.fontStyle = FontStyles.Bold;
            title.color = colorText;
            title.alignment = TextAlignmentOptions.Center;

            // Toggle'lar
            var lightToggle    = BuildToggle(cardGO.transform, "Işık Tahmini", new Vector2(0, -180));
            var occlToggle     = BuildToggle(cardGO.transform, "Oklüzyon",      new Vector2(0, -280));
            var spatialToggle  = BuildToggle(cardGO.transform, "Uzamsal Ses",   new Vector2(0, -380));
            var voiceToggle    = BuildToggle(cardGO.transform, "Ses Komutu",    new Vector2(0, -480));

            // Slider
            var sliderLabelGO = NewUIObject("QualityLabel", cardGO.transform);
            var slRT = sliderLabelGO.GetComponent<RectTransform>();
            slRT.anchorMin = new Vector2(0.5f, 1);
            slRT.anchorMax = new Vector2(0.5f, 1);
            slRT.pivot = new Vector2(0.5f, 1);
            slRT.sizeDelta = new Vector2(760, 60);
            slRT.anchoredPosition = new Vector2(0, -600);
            var slLabel = sliderLabelGO.AddComponent<TextMeshProUGUI>();
            slLabel.text = "Render Kalitesi: Orta";
            slLabel.fontSize = 32;
            slLabel.color = colorText;
            slLabel.alignment = TextAlignmentOptions.Center;

            var slider = BuildSlider(cardGO.transform, new Vector2(0, -700));

            // Kapat
            var closeGO = NewUIObject("CloseBtn", cardGO.transform);
            var clRT = closeGO.GetComponent<RectTransform>();
            clRT.anchorMin = new Vector2(0.5f, 0);
            clRT.anchorMax = new Vector2(0.5f, 0);
            clRT.pivot = new Vector2(0.5f, 0);
            clRT.sizeDelta = new Vector2(360, 110);
            clRT.anchoredPosition = new Vector2(0, 60);
            var clImg = closeGO.AddComponent<Image>();
            clImg.color = new Color(colorAccent.r, colorAccent.g, colorAccent.b, 0.4f);
            var clBtn = closeGO.AddComponent<Button>();
            clBtn.targetGraphic = clImg;

            var clTextGO = NewUIObject("Label", closeGO.transform);
            var clTextRT = clTextGO.GetComponent<RectTransform>();
            clTextRT.anchorMin = Vector2.zero;
            clTextRT.anchorMax = Vector2.one;
            clTextRT.offsetMin = Vector2.zero;
            clTextRT.offsetMax = Vector2.zero;
            var clText = clTextGO.AddComponent<TextMeshProUGUI>();
            clText.text = "Kapat";
            clText.fontSize = 36;
            clText.color = colorText;
            clText.fontStyle = FontStyles.Bold;
            clText.alignment = TextAlignmentOptions.Center;

            var ctrl = go.AddComponent<SettingsPanelController>();
            ctrl.canvasGroup = cg;
            ctrl.lightToggle = lightToggle;
            ctrl.occlusionToggle = occlToggle;
            ctrl.spatialAudioToggle = spatialToggle;
            ctrl.voiceToggle = voiceToggle;
            ctrl.qualitySlider = slider;
            ctrl.qualityLabel = slLabel;
            ctrl.closeButton = clBtn;

            return ctrl;
        }

        Toggle BuildToggle(Transform parent, string labelText, Vector2 anchoredPos)
        {
            var go = NewUIObject(labelText, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(760, 80);
            rt.anchoredPosition = anchoredPos;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.05f);

            var toggle = go.AddComponent<Toggle>();

            // Label
            var labelGO = NewUIObject("Label", go.transform);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(0.7f, 1);
            labelRT.offsetMin = new Vector2(24, 0);
            labelRT.offsetMax = Vector2.zero;
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = 32;
            label.color = colorText;
            label.alignment = TextAlignmentOptions.Left;

            // Switch background
            var switchBgGO = NewUIObject("SwitchBg", go.transform);
            var sbRT = switchBgGO.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(1, 0.5f);
            sbRT.anchorMax = new Vector2(1, 0.5f);
            sbRT.pivot = new Vector2(1, 0.5f);
            sbRT.sizeDelta = new Vector2(100, 50);
            sbRT.anchoredPosition = new Vector2(-24, 0);
            var sbImg = switchBgGO.AddComponent<Image>();
            sbImg.color = new Color(1f, 1f, 1f, 0.15f);

            // Checkmark
            var checkGO = NewUIObject("Check", switchBgGO.transform);
            var chRT = checkGO.GetComponent<RectTransform>();
            chRT.anchorMin = new Vector2(0, 0);
            chRT.anchorMax = new Vector2(1, 1);
            chRT.offsetMin = new Vector2(4, 4);
            chRT.offsetMax = new Vector2(-4, -4);
            var chImg = checkGO.AddComponent<Image>();
            chImg.color = colorAccent;

            toggle.targetGraphic = sbImg;
            toggle.graphic = chImg;
            toggle.isOn = true;

            return toggle;
        }

        Slider BuildSlider(Transform parent, Vector2 anchoredPos)
        {
            var go = NewUIObject("Slider", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(760, 50);
            rt.anchoredPosition = anchoredPos;

            var slider = go.AddComponent<Slider>();

            // Background
            var bgGO = NewUIObject("Background", go.transform);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.4f);
            bgRT.anchorMax = new Vector2(1, 0.6f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.12f);

            // Fill area
            var fillAreaGO = NewUIObject("FillArea", go.transform);
            var faRT = fillAreaGO.GetComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0, 0.4f);
            faRT.anchorMax = new Vector2(1, 0.6f);
            faRT.offsetMin = new Vector2(10, 0);
            faRT.offsetMax = new Vector2(-10, 0);

            var fillGO = NewUIObject("Fill", fillAreaGO.transform);
            var fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = colorAccent;

            // Handle area
            var handleAreaGO = NewUIObject("HandleSlideArea", go.transform);
            var haRT = handleAreaGO.GetComponent<RectTransform>();
            haRT.anchorMin = new Vector2(0, 0);
            haRT.anchorMax = new Vector2(1, 1);
            haRT.offsetMin = new Vector2(10, 0);
            haRT.offsetMax = new Vector2(-10, 0);

            var handleGO = NewUIObject("Handle", handleAreaGO.transform);
            var hRT = handleGO.GetComponent<RectTransform>();
            hRT.sizeDelta = new Vector2(40, 40);
            var hImg = handleGO.AddComponent<Image>();
            hImg.color = Color.white;
            hImg.sprite = CreateCircleSprite();

            slider.fillRect = fillRT;
            slider.handleRect = hRT;
            slider.targetGraphic = hImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 1;
            slider.maxValue = 5;
            slider.wholeNumbers = true;
            slider.value = 3;

            return slider;
        }

        // ────────────── Notifications ──────────────
        NotificationController CreateNotifications(Transform parent)
        {
            var go = NewUIObject("Notifications", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(760, 90);
            rt.anchoredPosition = new Vector2(0, -180);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.07f, 0.10f, 0.85f);

            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            var labelGO = NewUIObject("Label", go.transform);
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(24, 0);
            labelRT.offsetMax = new Vector2(-24, 0);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = "";
            label.fontSize = 32;
            label.color = colorText;
            label.alignment = TextAlignmentOptions.Center;

            var ctrl = go.AddComponent<NotificationController>();
            ctrl.canvasGroup = cg;
            ctrl.label = label;
            return ctrl;
        }

        // ────────────── Onboarding ──────────────
        OnboardingController CreateOnboarding(Transform parent)
        {
            var go = NewUIObject("Onboarding", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.78f);

            var cg = go.AddComponent<CanvasGroup>();

            // Card
            var cardGO = NewUIObject("Card", go.transform);
            var cardRT = cardGO.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(820, 640);
            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = colorPanelTint;

            // Title
            var titleGO = NewUIObject("Title", cardGO.transform);
            var tRT = titleGO.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 1);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.sizeDelta = new Vector2(0, 96);
            tRT.anchoredPosition = new Vector2(0, -36);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text = "Zemini Tarayın";
            title.fontSize = 56;
            title.fontStyle = FontStyles.Bold;
            title.color = colorText;
            title.alignment = TextAlignmentOptions.Center;

            // Instruction
            var instGO = NewUIObject("Instruction", cardGO.transform);
            var iRT = instGO.GetComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0, 1);
            iRT.anchorMax = new Vector2(1, 1);
            iRT.pivot = new Vector2(0.5f, 1);
            iRT.sizeDelta = new Vector2(-60, 160);
            iRT.anchoredPosition = new Vector2(0, -160);
            var inst = instGO.AddComponent<TextMeshProUGUI>();
            inst.text = "Kameranızı yavaşça hareket ettirin.\nDüz bir yüzeye doğrultun.";
            inst.fontSize = 32;
            inst.color = colorSubtle;
            inst.alignment = TextAlignmentOptions.Center;

            // Progress
            var slider = BuildSlider(cardGO.transform, new Vector2(0, -380));
            slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false; slider.value = 0;
            slider.interactable = false;

            // Start button
            var btnGO = NewUIObject("StartBtn", cardGO.transform);
            var btRT = btnGO.GetComponent<RectTransform>();
            btRT.anchorMin = new Vector2(0.5f, 0);
            btRT.anchorMax = new Vector2(0.5f, 0);
            btRT.pivot = new Vector2(0.5f, 0);
            btRT.sizeDelta = new Vector2(420, 110);
            btRT.anchoredPosition = new Vector2(0, 60);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(colorAccent.r, colorAccent.g, colorAccent.b, 0.5f);
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabelGO = NewUIObject("Label", btnGO.transform);
            var blRT = btnLabelGO.GetComponent<RectTransform>();
            blRT.anchorMin = Vector2.zero;
            blRT.anchorMax = Vector2.one;
            blRT.offsetMin = Vector2.zero;
            blRT.offsetMax = Vector2.zero;
            var btnLabel = btnLabelGO.AddComponent<TextMeshProUGUI>();
            btnLabel.text = "Başlat";
            btnLabel.fontSize = 40;
            btnLabel.color = colorText;
            btnLabel.fontStyle = FontStyles.Bold;
            btnLabel.alignment = TextAlignmentOptions.Center;

            var ctrl = go.AddComponent<OnboardingController>();
            ctrl.canvasGroup = cg;
            ctrl.titleText = title;
            ctrl.instructionText = inst;
            ctrl.progressBar = slider;
            ctrl.startButton = btn;
            return ctrl;
        }

        // ────────────── 3D Reticle ──────────────
        ReticleController CreateReticle()
        {
            var rootGO = new GameObject("ARReticle");
            var visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.name = "Visual";
            visual.transform.SetParent(rootGO.transform, false);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            visual.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            // Collider'i sil — UI ile etkileşmesin
            var col = visual.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var r = visual.GetComponent<Renderer>();
            if (r != null)
            {
                var shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    Color c = new Color(colorAccent.r, colorAccent.g, colorAccent.b, 0.7f);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                    if (mat.HasProperty("_Color"))     mat.SetColor("_Color", c);
                    r.sharedMaterial = mat;
                }
            }

            var ctrl = rootGO.AddComponent<ReticleController>();
            ctrl.reticleVisual = visual.transform;
            ctrl.arCamera = Camera.main;
            return ctrl;
        }

        // ────────────── World-Space Panel ──────────────
        WorldSpacePanel CreateWorldSpacePanel()
        {
            var rootGO = new GameObject("WorldSpacePanel");
            rootGO.transform.localScale = Vector3.one * 0.004f;

            var canvas = rootGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rt = rootGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 120);

            rootGO.AddComponent<GraphicRaycaster>();

            var bg = rootGO.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.07f, 0.10f, 0.85f);

            var hlg = rootGO.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(12, 12, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var rotBtn = BuildMiniButton(rootGO.transform, "↻", new Color(0.40f, 0.85f, 0.55f));
            var upBtn  = BuildMiniButton(rootGO.transform, "+", colorAccent);
            var dnBtn  = BuildMiniButton(rootGO.transform, "−", colorAccent);
            var delBtn = BuildMiniButton(rootGO.transform, "✕", new Color(0.95f, 0.40f, 0.40f));

            var panel = rootGO.AddComponent<WorldSpacePanel>();
            panel.rotateButton = rotBtn;
            panel.scaleUpButton = upBtn;
            panel.scaleDownButton = dnBtn;
            panel.deleteButton = delBtn;
            panel.referenceCamera = Camera.main;

            rootGO.SetActive(false);
            return panel;
        }

        Button BuildMiniButton(Transform parent, string icon, Color accent)
        {
            var go = NewUIObject(icon, parent);
            var img = go.AddComponent<Image>();
            img.color = new Color(accent.r, accent.g, accent.b, 0.3f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var lGO = NewUIObject("Label", go.transform);
            var lRT = lGO.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero;
            lRT.anchorMax = Vector2.one;
            lRT.offsetMin = Vector2.zero;
            lRT.offsetMax = Vector2.zero;
            var lTxt = lGO.AddComponent<TextMeshProUGUI>();
            lTxt.text = icon;
            lTxt.fontSize = 40;
            lTxt.color = colorText;
            lTxt.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        // ────────────── Helpers ──────────────
        static GameObject NewUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static Sprite _circleSprite;
        static Sprite CreateCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - dist);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _circleSprite.name = "RuntimeCircle";
            return _circleSprite;
        }
    }
}
