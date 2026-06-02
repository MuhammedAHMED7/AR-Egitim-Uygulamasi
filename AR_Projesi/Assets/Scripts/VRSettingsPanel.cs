using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// VR ayarlar paneli. VRInteractionManager'daki değerleri okuyup yazar:
    /// hareket modu (Teleport / Sürekli / Karışık), hareket hızı,
    /// snap turn açısı, smooth turn açma/kapatma ve smooth turn hızı.
    /// Her değişiklik anında uygulanır + PlayerPrefs'e kaydedilir.
    /// </summary>
    public class VRSettingsPanel : VRUIPanel
    {
        TMP_Dropdown _movementDropdown;
        Slider _moveSpeedSlider;
        TextMeshProUGUI _moveSpeedValue;
        Slider _snapAngleSlider;
        TextMeshProUGUI _snapAngleValue;
        Toggle _smoothTurnToggle;
        Slider _smoothSpeedSlider;
        TextMeshProUGUI _smoothSpeedValue;

        // İçeri girerken VRInteractionManager.Load çağrılı; dışarı çıkarken
        // doğrudan Save edip controller'ı yeniden uygula
        VRMovementController _movement;

        bool _suppressEvents;

        protected override void Awake()
        {
            base.Awake();
            Build();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        void Start()
        {
            _movement = FindObjectOfType<VRMovementController>();
        }

        void Build()
        {
            VRUIFactory.CreatePanelBackground(transform);

            // Başlık
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(transform, false);
            var tRT = (RectTransform)titleGO.transform;
            tRT.anchorMin = new Vector2(0, 1);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.anchoredPosition = new Vector2(0, -32);
            tRT.sizeDelta = new Vector2(-48, 70);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text = "Ayarlar";
            title.fontSize = VRUITheme.FontTitle;
            title.fontStyle = FontStyles.Bold;
            title.color = VRUITheme.TextPrimary;
            title.alignment = TextAlignmentOptions.Center;
            title.raycastTarget = false;

            // İçerik konteyneri (dikey liste)
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(transform, false);
            var cRT = (RectTransform)content.transform;
            cRT.anchorMin = new Vector2(0, 0);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.offsetMin = new Vector2(48, 110);
            cRT.offsetMax = new Vector2(-48, -120);
            VRUIFactory.AddVerticalLayout(content, 22,
                new RectOffset(0, 0, 0, 0), TextAnchor.UpperCenter);

            // ─── 1) Hareket Modu Dropdown ───
            BuildLabeledRow(content.transform, "Hareket Modu", out var row1);
            _movementDropdown = CreateDropdown(row1.transform);
            _movementDropdown.AddOptions(new System.Collections.Generic.List<string> {
                "Yalnızca Işınlanma",
                "Yalnızca Sürekli",
                "Karışık (her ikisi)"
            });
            _movementDropdown.onValueChanged.AddListener(OnMovementModeChanged);

            // ─── 2) Hareket Hızı ───
            BuildLabeledRow(content.transform, "Hareket Hızı", out var row2, valueDisplayName: "MoveSpeedValue");
            _moveSpeedValue = row2.GetComponentInChildren<TextMeshProUGUI>(true);
            // CreateLabeledRow başlığı ekledi; biz değer için ayrı bir text döndürdük
            _moveSpeedSlider = VRUIFactory.CreateSlider(row2.transform, 0.5f, 5f, 1.8f,
                                                       new Vector2(0, 50), "MoveSpeedSlider");
            // İçeri sırayla yerleştir
            var moveSpeedValueGO = new GameObject("MoveSpeedDisplay", typeof(RectTransform));
            moveSpeedValueGO.transform.SetParent(row2.transform, false);
            var msvRT = (RectTransform)moveSpeedValueGO.transform;
            msvRT.sizeDelta = new Vector2(140, 50);
            _moveSpeedValue = moveSpeedValueGO.AddComponent<TextMeshProUGUI>();
            _moveSpeedValue.fontSize = VRUITheme.FontBody;
            _moveSpeedValue.color = VRUITheme.Accent;
            _moveSpeedValue.alignment = TextAlignmentOptions.MidlineRight;
            _moveSpeedValue.raycastTarget = false;
            _moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);

            // ─── 3) Snap Turn Açısı ───
            BuildLabeledRow(content.transform, "Snap Turn Açısı", out var row3);
            _snapAngleSlider = VRUIFactory.CreateSlider(row3.transform, 15f, 90f, 30f,
                                                       new Vector2(0, 50), "SnapAngleSlider");
            _snapAngleSlider.wholeNumbers = true;
            var snapDisplayGO = new GameObject("SnapDisplay", typeof(RectTransform));
            snapDisplayGO.transform.SetParent(row3.transform, false);
            var sdRT = (RectTransform)snapDisplayGO.transform;
            sdRT.sizeDelta = new Vector2(140, 50);
            _snapAngleValue = snapDisplayGO.AddComponent<TextMeshProUGUI>();
            _snapAngleValue.fontSize = VRUITheme.FontBody;
            _snapAngleValue.color = VRUITheme.Accent;
            _snapAngleValue.alignment = TextAlignmentOptions.MidlineRight;
            _snapAngleValue.raycastTarget = false;
            _snapAngleSlider.onValueChanged.AddListener(OnSnapAngleChanged);

            // ─── 4) Smooth Turn Toggle ───
            var toggleRow = new GameObject("SmoothTurnRow", typeof(RectTransform));
            toggleRow.transform.SetParent(content.transform, false);
            var trRT = (RectTransform)toggleRow.transform;
            trRT.sizeDelta = new Vector2(0, 70);
            _smoothTurnToggle = VRUIFactory.CreateToggle(toggleRow.transform,
                "Yumuşak Dönüş (Snap yerine)", false,
                new Vector2(0, 60), "SmoothTurnToggle");
            // Toggle stretch
            var stRT = (RectTransform)_smoothTurnToggle.transform;
            stRT.anchorMin = new Vector2(0, 0);
            stRT.anchorMax = new Vector2(1, 1);
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            _smoothTurnToggle.onValueChanged.AddListener(OnSmoothTurnToggleChanged);

            // ─── 5) Smooth Turn Hızı ───
            BuildLabeledRow(content.transform, "Yumuşak Dönüş Hızı", out var row5);
            _smoothSpeedSlider = VRUIFactory.CreateSlider(row5.transform, 30f, 180f, 80f,
                                                         new Vector2(0, 50), "SmoothSpeedSlider");
            _smoothSpeedSlider.wholeNumbers = true;
            var smoothDispGO = new GameObject("SmoothDisplay", typeof(RectTransform));
            smoothDispGO.transform.SetParent(row5.transform, false);
            var smdRT = (RectTransform)smoothDispGO.transform;
            smdRT.sizeDelta = new Vector2(140, 50);
            _smoothSpeedValue = smoothDispGO.AddComponent<TextMeshProUGUI>();
            _smoothSpeedValue.fontSize = VRUITheme.FontBody;
            _smoothSpeedValue.color = VRUITheme.Accent;
            _smoothSpeedValue.alignment = TextAlignmentOptions.MidlineRight;
            _smoothSpeedValue.raycastTarget = false;
            _smoothSpeedSlider.onValueChanged.AddListener(OnSmoothSpeedChanged);

            // ─── Alt: Kapat butonu ───
            var closeBtn = VRUIFactory.CreateButton(transform, "Tamam",
                new Vector2(280, 80), VRUITheme.Accent, VRUITheme.FontButton, "BtnClose");
            var cbRT = (RectTransform)closeBtn.transform;
            cbRT.anchorMin = new Vector2(0.5f, 0);
            cbRT.anchorMax = new Vector2(0.5f, 0);
            cbRT.pivot = new Vector2(0.5f, 0);
            cbRT.anchoredPosition = new Vector2(0, 28);
            closeBtn.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.CloseSettings();
            });
        }

        // Yardımcı: etiket+kontrol satırı
        void BuildLabeledRow(Transform parent, string labelText, out GameObject rowGO, string valueDisplayName = null)
        {
            rowGO = new GameObject(labelText.Replace(" ", "") + "Row", typeof(RectTransform));
            rowGO.transform.SetParent(parent, false);
            var rRT = (RectTransform)rowGO.transform;
            rRT.sizeDelta = new Vector2(0, 88);

            VRUIFactory.AddHorizontalLayout(rowGO, spacing: 12,
                padding: new RectOffset(4, 4, 4, 4),
                align: TextAnchor.MiddleLeft);

            // Etiket
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(rowGO.transform, false);
            var lRT = (RectTransform)labelGO.transform;
            lRT.sizeDelta = new Vector2(280, 60);
            var lTmp = labelGO.AddComponent<TextMeshProUGUI>();
            lTmp.text = labelText;
            lTmp.fontSize = VRUITheme.FontBody;
            lTmp.color = VRUITheme.TextPrimary;
            lTmp.alignment = TextAlignmentOptions.MidlineLeft;
            lTmp.raycastTarget = false;

            var le = labelGO.AddComponent<LayoutElement>();
            le.preferredWidth = 280;
            le.minWidth = 240;
        }

        // Yardımcı dropdown
        TMP_Dropdown CreateDropdown(Transform parent)
        {
            var go = new GameObject("Dropdown", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(380, 60);

            var img = go.AddComponent<Image>();
            img.sprite = VRUIFactory.RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = VRUITheme.ButtonNormal;

            var dd = go.AddComponent<TMP_Dropdown>();
            dd.targetGraphic = img;

            // Caption (seçili metin)
            var captionGO = new GameObject("Label", typeof(RectTransform));
            captionGO.transform.SetParent(go.transform, false);
            var capRT = (RectTransform)captionGO.transform;
            capRT.anchorMin = new Vector2(0, 0);
            capRT.anchorMax = new Vector2(1, 1);
            capRT.offsetMin = new Vector2(16, 6);
            capRT.offsetMax = new Vector2(-40, -6);
            var capTmp = captionGO.AddComponent<TextMeshProUGUI>();
            capTmp.fontSize = VRUITheme.FontBody;
            capTmp.color = VRUITheme.TextPrimary;
            capTmp.alignment = TextAlignmentOptions.MidlineLeft;
            capTmp.raycastTarget = false;
            dd.captionText = capTmp;

            // Ok işareti
            var arrowGO = new GameObject("Arrow", typeof(RectTransform));
            arrowGO.transform.SetParent(go.transform, false);
            var arRT = (RectTransform)arrowGO.transform;
            arRT.anchorMin = new Vector2(1, 0.5f);
            arRT.anchorMax = new Vector2(1, 0.5f);
            arRT.pivot = new Vector2(1, 0.5f);
            arRT.sizeDelta = new Vector2(20, 20);
            arRT.anchoredPosition = new Vector2(-12, 0);
            var arTmp = arrowGO.AddComponent<TextMeshProUGUI>();
            arTmp.text = "▾";
            arTmp.fontSize = VRUITheme.FontBody;
            arTmp.color = VRUITheme.TextSecondary;
            arTmp.alignment = TextAlignmentOptions.Center;
            arTmp.raycastTarget = false;

            // Template (açılır liste)
            var templateGO = new GameObject("Template", typeof(RectTransform));
            templateGO.transform.SetParent(go.transform, false);
            templateGO.SetActive(false);
            var tRT = (RectTransform)templateGO.transform;
            tRT.anchorMin = new Vector2(0, 0);
            tRT.anchorMax = new Vector2(1, 0);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.anchoredPosition = new Vector2(0, 2);
            tRT.sizeDelta = new Vector2(0, 180);
            var tImg = templateGO.AddComponent<Image>();
            tImg.sprite = VRUIFactory.RoundedSprite;
            tImg.type = Image.Type.Sliced;
            tImg.color = VRUITheme.PanelBackgroundDim;

            // ScrollRect — basit
            var scroll = templateGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(templateGO.transform, false);
            var vRT = (RectTransform)viewport.transform;
            vRT.anchorMin = Vector2.zero;
            vRT.anchorMax = Vector2.one;
            vRT.offsetMin = new Vector2(4, 4);
            vRT.offsetMax = new Vector2(-4, -4);
            var mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            var vImg = viewport.AddComponent<Image>();
            vImg.color = new Color(0, 0, 0, 0.01f);
            scroll.viewport = (RectTransform)viewport.transform;

            // Content
            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(viewport.transform, false);
            var cRT = (RectTransform)contentGO.transform;
            cRT.anchorMin = new Vector2(0, 1);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.pivot = new Vector2(0.5f, 1);
            cRT.sizeDelta = new Vector2(0, 60);
            scroll.content = cRT;

            // Item template
            var itemGO = new GameObject("Item", typeof(RectTransform));
            itemGO.transform.SetParent(contentGO.transform, false);
            var iRT = (RectTransform)itemGO.transform;
            iRT.anchorMin = new Vector2(0, 0.5f);
            iRT.anchorMax = new Vector2(1, 0.5f);
            iRT.sizeDelta = new Vector2(0, 60);
            var itemTog = itemGO.AddComponent<Toggle>();
            var itemBG = itemGO.AddComponent<Image>();
            itemBG.color = new Color(1, 1, 1, 0.0f);
            itemTog.targetGraphic = itemBG;

            // Item — Selected (checkmark area)
            var itemCheckedGO = new GameObject("Item Checkmark", typeof(RectTransform));
            itemCheckedGO.transform.SetParent(itemGO.transform, false);
            var icRT = (RectTransform)itemCheckedGO.transform;
            icRT.anchorMin = new Vector2(0, 0);
            icRT.anchorMax = new Vector2(0, 1);
            icRT.pivot = new Vector2(0, 0.5f);
            icRT.sizeDelta = new Vector2(40, 0);
            icRT.anchoredPosition = new Vector2(10, 0);
            var icImg = itemCheckedGO.AddComponent<Image>();
            icImg.color = VRUITheme.Accent;
            icImg.sprite = VRUIFactory.RoundedSprite;
            icImg.type = Image.Type.Sliced;
            itemTog.graphic = icImg;

            // Item — Label
            var itemLabelGO = new GameObject("Item Label", typeof(RectTransform));
            itemLabelGO.transform.SetParent(itemGO.transform, false);
            var ilRT = (RectTransform)itemLabelGO.transform;
            ilRT.anchorMin = new Vector2(0, 0);
            ilRT.anchorMax = new Vector2(1, 1);
            ilRT.offsetMin = new Vector2(60, 0);
            ilRT.offsetMax = new Vector2(-10, 0);
            var ilTmp = itemLabelGO.AddComponent<TextMeshProUGUI>();
            ilTmp.fontSize = VRUITheme.FontBody;
            ilTmp.color = VRUITheme.TextPrimary;
            ilTmp.alignment = TextAlignmentOptions.MidlineLeft;
            ilTmp.raycastTarget = false;

            dd.template = (RectTransform)templateGO.transform;
            dd.itemText = ilTmp;

            return dd;
        }

        // ───────── Olay handlers ─────────
        void OnMovementModeChanged(int index)
        {
            if (_suppressEvents) return;
            var m = VRInteractionManager.Instance;
            if (m == null) return;
            m.SetMovementMode((VRInteractionManager.MovementMode)index);
            // VRMovementController.OnMovementModeChanged ile zaten reaplly oluyor
        }

        void OnMoveSpeedChanged(float value)
        {
            if (_suppressEvents) return;
            var m = VRInteractionManager.Instance;
            if (m == null) return;
            m.moveSpeed = value;
            m.Save();
            if (_movement != null) _movement.ApplyCurrentSettings();
            UpdateValueLabels();
        }

        void OnSnapAngleChanged(float value)
        {
            if (_suppressEvents) return;
            var m = VRInteractionManager.Instance;
            if (m == null) return;
            m.snapTurnAngle = value;
            m.Save();
            if (_movement != null) _movement.ApplyCurrentSettings();
            UpdateValueLabels();
        }

        void OnSmoothTurnToggleChanged(bool isOn)
        {
            if (_suppressEvents) return;
            var m = VRInteractionManager.Instance;
            if (m == null) return;
            m.useSmoothTurn = isOn;
            m.Save();
            if (_movement != null) _movement.ApplyCurrentSettings();
            RefreshDependentInteractables();
        }

        void OnSmoothSpeedChanged(float value)
        {
            if (_suppressEvents) return;
            var m = VRInteractionManager.Instance;
            if (m == null) return;
            m.smoothTurnSpeed = value;
            m.Save();
            if (_movement != null) _movement.ApplyCurrentSettings();
            UpdateValueLabels();
        }

        public void RefreshFromManager()
        {
            var m = VRInteractionManager.Instance;
            if (m == null) return;

            _suppressEvents = true;
            try
            {
                if (_movementDropdown != null) _movementDropdown.value = (int)m.movementMode;
                if (_moveSpeedSlider != null) _moveSpeedSlider.value = m.moveSpeed;
                if (_snapAngleSlider != null) _snapAngleSlider.value = m.snapTurnAngle;
                if (_smoothTurnToggle != null) _smoothTurnToggle.isOn = m.useSmoothTurn;
                if (_smoothSpeedSlider != null) _smoothSpeedSlider.value = m.smoothTurnSpeed;
            }
            finally { _suppressEvents = false; }

            UpdateValueLabels();
            RefreshDependentInteractables();
        }

        void UpdateValueLabels()
        {
            if (_moveSpeedValue != null && _moveSpeedSlider != null)
                _moveSpeedValue.text = $"{_moveSpeedSlider.value:0.0} m/s";
            if (_snapAngleValue != null && _snapAngleSlider != null)
                _snapAngleValue.text = $"{_snapAngleSlider.value:0}°";
            if (_smoothSpeedValue != null && _smoothSpeedSlider != null)
                _smoothSpeedValue.text = $"{_smoothSpeedSlider.value:0}°/s";
        }

        void RefreshDependentInteractables()
        {
            // Snap açısı sadece snap turn aktifken anlamlı; smooth turn aktifse
            // snap slider'ı yarı saydam yapıp etkileşimi kapatabiliriz
            var m = VRInteractionManager.Instance;
            if (m == null) return;
            bool snapActive = !m.useSmoothTurn;
            if (_snapAngleSlider != null)
            {
                _snapAngleSlider.interactable = snapActive;
                var cg = _snapAngleSlider.GetComponent<CanvasGroup>();
                if (cg == null) cg = _snapAngleSlider.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = snapActive ? 1f : 0.45f;
            }
            if (_smoothSpeedSlider != null)
            {
                _smoothSpeedSlider.interactable = !snapActive;
                var cg = _smoothSpeedSlider.GetComponent<CanvasGroup>();
                if (cg == null) cg = _smoothSpeedSlider.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = !snapActive ? 1f : 0.45f;
            }
        }
    }
}
