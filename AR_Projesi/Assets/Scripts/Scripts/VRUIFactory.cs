using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// VR UI elemanlarını (panel, buton, başlık, slider, toggle, ayraç) tek tip
    /// olarak oluşturan yardımcı sınıf. Tüm UI script'leri bu fabrikadan
    /// elemanlar yaratır — böylece tema değişikliği tek noktada yapılır.
    /// </summary>
    public static class VRUIFactory
    {
        static Sprite _solidSprite;
        static Sprite _roundedSprite;

        public static Sprite SolidSprite
        {
            get
            {
                if (_solidSprite == null) _solidSprite = VRUITheme.CreateSolidSprite();
                return _solidSprite;
            }
        }

        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite == null) _roundedSprite = VRUITheme.CreateRoundedSprite(14);
                return _roundedSprite;
            }
        }

        // ───────── Canvas ─────────
        public static Canvas CreateWorldCanvas(string name, Transform parent,
                                               float widthPx, float heightPx)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 2f;
            scaler.referencePixelsPerUnit = 100f;

            // Grafik raycaster — VR'da TrackedDeviceGraphicRaycaster gerekir
            // (XR ray interactor'lar bunu kullanır)
            var raycaster = go.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();

            var rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(widthPx, heightPx);
            rt.localScale = Vector3.one * VRUITheme.CanvasScale;

            return canvas;
        }

        // ───────── Panel arka plan ─────────
        public static Image CreatePanelBackground(Transform parent, Color? color = null)
        {
            var go = new GameObject("Background", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = color ?? VRUITheme.PanelBackground;
            img.raycastTarget = true;   // arkadaki nesnelere ray geçişini engelle
            return img;
        }

        // ───────── Metin ─────────
        public static TextMeshProUGUI CreateText(Transform parent, string content, int fontSize,
                                                  Color? color = null,
                                                  TextAlignmentOptions align = TextAlignmentOptions.Center,
                                                  string name = "Text")
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color ?? VRUITheme.TextPrimary;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        // ───────── Buton ─────────
        public static Button CreateButton(Transform parent, string label, Vector2 size,
                                          Color? tint = null,
                                          int fontSize = VRUITheme.FontButton,
                                          string name = "Button")
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = tint ?? VRUITheme.ButtonNormal;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = new Color(1.10f, 1.10f, 1.10f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.targetGraphic = img;

            // Etiket
            var labelGO = CreateText(go.transform, label, fontSize, VRUITheme.TextPrimary,
                                     TextAlignmentOptions.Center, "Label");
            var lrt = labelGO.rectTransform;
            // Hafif iç boşluk
            lrt.offsetMin = new Vector2(12, 4);
            lrt.offsetMax = new Vector2(-12, -4);

            // Hover/click haptic + ses geri bildirimi
            var fb = go.AddComponent<VRButtonFeedback>();
            fb.targetGraphic = img;
            fb.normalColor = tint ?? VRUITheme.ButtonNormal;
            fb.hoverColor = VRUITheme.ButtonHover;
            fb.pressedColor = VRUITheme.ButtonPressed;

            return btn;
        }

        // ───────── Slider (Ayar değeri için) ─────────
        public static Slider CreateSlider(Transform parent, float min, float max, float value,
                                          Vector2 size, string name = "Slider")
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            // Arka plan (track)
            var bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(go.transform, false);
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = new Vector2(0, 0.4f);
            bgRT.anchorMax = new Vector2(1, 0.6f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = RoundedSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = VRUITheme.ButtonNormal;

            // Fill alanı
            var fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGO.transform.SetParent(go.transform, false);
            var faRT = (RectTransform)fillAreaGO.transform;
            faRT.anchorMin = new Vector2(0, 0.4f);
            faRT.anchorMax = new Vector2(1, 0.6f);
            faRT.offsetMin = new Vector2(8, 0);
            faRT.offsetMax = new Vector2(-8, 0);

            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fRT = (RectTransform)fillGO.transform;
            fRT.anchorMin = new Vector2(0, 0);
            fRT.anchorMax = new Vector2(1, 1);
            fRT.offsetMin = Vector2.zero;
            fRT.offsetMax = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.sprite = RoundedSprite;
            fillImg.type = Image.Type.Sliced;
            fillImg.color = VRUITheme.Accent;

            // Handle alanı + handle
            var handleAreaGO = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGO.transform.SetParent(go.transform, false);
            var haRT = (RectTransform)handleAreaGO.transform;
            haRT.anchorMin = Vector2.zero;
            haRT.anchorMax = Vector2.one;
            haRT.offsetMin = new Vector2(14, 0);
            haRT.offsetMax = new Vector2(-14, 0);

            var handleGO = new GameObject("Handle", typeof(RectTransform));
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            var hRT = (RectTransform)handleGO.transform;
            hRT.sizeDelta = new Vector2(36, 36);
            var handleImg = handleGO.AddComponent<Image>();
            handleImg.sprite = RoundedSprite;
            handleImg.type = Image.Type.Sliced;
            handleImg.color = Color.white;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fRT;
            slider.handleRect = hRT;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.wholeNumbers = false;
            return slider;
        }

        // ───────── Toggle ─────────
        public static Toggle CreateToggle(Transform parent, string label, bool initial,
                                          Vector2 size, string name = "Toggle")
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size;

            // Görsel kutu (checkbox)
            var boxGO = new GameObject("Box", typeof(RectTransform));
            boxGO.transform.SetParent(go.transform, false);
            var boxRT = (RectTransform)boxGO.transform;
            boxRT.anchorMin = new Vector2(0, 0.5f);
            boxRT.anchorMax = new Vector2(0, 0.5f);
            boxRT.pivot = new Vector2(0, 0.5f);
            boxRT.sizeDelta = new Vector2(36, 36);
            boxRT.anchoredPosition = new Vector2(8, 0);
            var boxImg = boxGO.AddComponent<Image>();
            boxImg.sprite = RoundedSprite;
            boxImg.type = Image.Type.Sliced;
            boxImg.color = VRUITheme.ButtonNormal;

            // İçindeki onay işareti
            var checkGO = new GameObject("Check", typeof(RectTransform));
            checkGO.transform.SetParent(boxGO.transform, false);
            var checkRT = (RectTransform)checkGO.transform;
            checkRT.anchorMin = new Vector2(0.18f, 0.18f);
            checkRT.anchorMax = new Vector2(0.82f, 0.82f);
            checkRT.offsetMin = Vector2.zero;
            checkRT.offsetMax = Vector2.zero;
            var checkImg = checkGO.AddComponent<Image>();
            checkImg.sprite = RoundedSprite;
            checkImg.type = Image.Type.Sliced;
            checkImg.color = VRUITheme.Accent;

            // Etiket
            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(go.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            lblRT.anchorMin = new Vector2(0, 0);
            lblRT.anchorMax = new Vector2(1, 1);
            lblRT.offsetMin = new Vector2(60, 0);
            lblRT.offsetMax = new Vector2(-8, 0);
            var lblTmp = lblGO.AddComponent<TextMeshProUGUI>();
            lblTmp.text = label;
            lblTmp.fontSize = VRUITheme.FontBody;
            lblTmp.color = VRUITheme.TextPrimary;
            lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
            lblTmp.raycastTarget = false;

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = boxImg;
            toggle.graphic = checkImg;
            toggle.isOn = initial;
            return toggle;
        }

        // ───────── Yatay ayraç (separator) ─────────
        public static Image CreateSeparator(Transform parent, float height = 2f)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(-40, height);
            var img = go.AddComponent<Image>();
            img.color = VRUITheme.PanelBorder;
            img.raycastTarget = false;
            return img;
        }

        // ───────── Dikey düzen yardımcısı ─────────
        public static VerticalLayoutGroup AddVerticalLayout(GameObject host, float spacing,
                                                            RectOffset padding = null,
                                                            TextAnchor align = TextAnchor.UpperCenter)
        {
            var vlg = host.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.padding = padding ?? new RectOffset(24, 24, 24, 24);
            vlg.childAlignment = align;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            return vlg;
        }

        public static HorizontalLayoutGroup AddHorizontalLayout(GameObject host, float spacing,
                                                                RectOffset padding = null,
                                                                TextAnchor align = TextAnchor.MiddleCenter)
        {
            var hlg = host.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.padding = padding ?? new RectOffset(0, 0, 0, 0);
            hlg.childAlignment = align;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            return hlg;
        }
    }
}
