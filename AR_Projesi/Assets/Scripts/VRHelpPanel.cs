using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// VR yardım paneli — kumanda eşleştirmesi ve temel etkileşim ipuçlarını
    /// gösterir. Yeni kullanıcılar için sahnenin nasıl kullanılacağını anlatır.
    /// </summary>
    public class VRHelpPanel : VRUIPanel
    {
        protected override void Awake()
        {
            base.Awake();
            Build();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        void Build()
        {
            VRUIFactory.CreatePanelBackground(transform);

            // Başlık
            var title = VRUIFactory.CreateText(transform, "Yardım & Kumanda Rehberi",
                VRUITheme.FontTitle, VRUITheme.TextPrimary,
                TextAlignmentOptions.Top, "Title");
            title.fontStyle = FontStyles.Bold;
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -32);
            trt.sizeDelta = new Vector2(-48, 70);

            // İçerik - iki kolon (sol kumanda / sağ kumanda)
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(transform, false);
            var cRT = (RectTransform)content.transform;
            cRT.anchorMin = new Vector2(0, 0);
            cRT.anchorMax = new Vector2(1, 1);
            cRT.offsetMin = new Vector2(40, 110);
            cRT.offsetMax = new Vector2(-40, -110);

            var hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24;
            hlg.padding = new RectOffset(0, 0, 0, 0);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            BuildColumn(content.transform, "🖐  Sol Kumanda", VRUITheme.Warning, new (string, string)[] {
                ("Joystick",   "Hareket et (ileri/geri/yan)"),
                ("Trigger",    "UI'da seç / tıkla"),
                ("Grip",       "Nesneyi tut"),
                ("Menu (≡)",   "Ana menüyü aç/kapa"),
                ("Bileği çevir", "Bilek menüsü görünür"),
            });

            BuildColumn(content.transform, "✋  Sağ Kumanda", VRUITheme.Accent, new (string, string)[] {
                ("Joystick (yatay)", "Snap/yumuşak dön"),
                ("Joystick (yukarı)", "Işınlanmayı başlat"),
                ("Trigger",         "UI'da seç / tıkla"),
                ("Grip",            "Nesneyi tut"),
                ("A düğmesi",       "Radial menü (hızlı)"),
                ("B düğmesi",       "Işınlanmayı iptal et"),
            });

            // Alt: ipucu metni
            var hint = VRUIFactory.CreateText(transform,
                "İpucu: Konfor için ilk denemende Snap Turn (varsayılan) kullan. " +
                "Alıştıktan sonra Ayarlar'dan Yumuşak Dönüş'e geçebilirsin.",
                VRUITheme.FontSmall, VRUITheme.TextSecondary,
                TextAlignmentOptions.Center, "Hint");
            var hrt = hint.rectTransform;
            hrt.anchorMin = new Vector2(0, 0);
            hrt.anchorMax = new Vector2(1, 0);
            hrt.pivot = new Vector2(0.5f, 0);
            hrt.anchoredPosition = new Vector2(0, 100);
            hrt.sizeDelta = new Vector2(-80, 70);

            // Kapat butonu
            var closeBtn = VRUIFactory.CreateButton(transform, "Anladım",
                new Vector2(280, 80), VRUITheme.Accent, VRUITheme.FontButton, "BtnClose");
            var cbRT = (RectTransform)closeBtn.transform;
            cbRT.anchorMin = new Vector2(0.5f, 0);
            cbRT.anchorMax = new Vector2(0.5f, 0);
            cbRT.pivot = new Vector2(0.5f, 0);
            cbRT.anchoredPosition = new Vector2(0, 24);
            closeBtn.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.CloseHelp();
            });
        }

        void BuildColumn(Transform parent, string heading, Color accent,
                         (string key, string desc)[] rows)
        {
            var colGO = new GameObject(heading, typeof(RectTransform));
            colGO.transform.SetParent(parent, false);
            var colRT = (RectTransform)colGO.transform;

            // İç dikey düzen
            var vlg = colGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;

            var bg = colGO.AddComponent<Image>();
            bg.sprite = VRUIFactory.RoundedSprite;
            bg.type = Image.Type.Sliced;
            bg.color = VRUITheme.PanelBackgroundDim;

            // Başlık
            var headGO = new GameObject("Heading", typeof(RectTransform));
            headGO.transform.SetParent(colGO.transform, false);
            var hRT = (RectTransform)headGO.transform;
            hRT.sizeDelta = new Vector2(0, 60);
            var hTmp = headGO.AddComponent<TextMeshProUGUI>();
            hTmp.text = heading;
            hTmp.fontSize = VRUITheme.FontHeading;
            hTmp.fontStyle = FontStyles.Bold;
            hTmp.color = accent;
            hTmp.alignment = TextAlignmentOptions.MidlineLeft;
            hTmp.raycastTarget = false;

            // Satırlar
            foreach (var (key, desc) in rows)
            {
                var rowGO = new GameObject("Row_" + key, typeof(RectTransform));
                rowGO.transform.SetParent(colGO.transform, false);
                var rRT = (RectTransform)rowGO.transform;
                rRT.sizeDelta = new Vector2(0, 64);

                var rhlg = rowGO.AddComponent<HorizontalLayoutGroup>();
                rhlg.spacing = 12;
                rhlg.padding = new RectOffset(0, 0, 0, 0);
                rhlg.childAlignment = TextAnchor.MiddleLeft;
                rhlg.childControlWidth = false;
                rhlg.childControlHeight = true;

                // Tuş etiketi (rozet)
                var keyGO = new GameObject("Key", typeof(RectTransform));
                keyGO.transform.SetParent(rowGO.transform, false);
                var keyImg = keyGO.AddComponent<Image>();
                keyImg.sprite = VRUIFactory.RoundedSprite;
                keyImg.type = Image.Type.Sliced;
                keyImg.color = VRUITheme.ButtonNormal;
                var keyLE = keyGO.AddComponent<LayoutElement>();
                keyLE.preferredWidth = 180;
                keyLE.preferredHeight = 56;

                var keyLblGO = new GameObject("KeyLabel", typeof(RectTransform));
                keyLblGO.transform.SetParent(keyGO.transform, false);
                var kRT = (RectTransform)keyLblGO.transform;
                kRT.anchorMin = Vector2.zero;
                kRT.anchorMax = Vector2.one;
                kRT.offsetMin = new Vector2(8, 4);
                kRT.offsetMax = new Vector2(-8, -4);
                var keyTmp = keyLblGO.AddComponent<TextMeshProUGUI>();
                keyTmp.text = key;
                keyTmp.fontSize = VRUITheme.FontSmall;
                keyTmp.color = accent;
                keyTmp.fontStyle = FontStyles.Bold;
                keyTmp.alignment = TextAlignmentOptions.Center;
                keyTmp.raycastTarget = false;

                // Açıklama
                var descGO = new GameObject("Desc", typeof(RectTransform));
                descGO.transform.SetParent(rowGO.transform, false);
                var dRT = (RectTransform)descGO.transform;
                dRT.sizeDelta = new Vector2(360, 56);
                var dTmp = descGO.AddComponent<TextMeshProUGUI>();
                dTmp.text = desc;
                dTmp.fontSize = VRUITheme.FontBody;
                dTmp.color = VRUITheme.TextPrimary;
                dTmp.alignment = TextAlignmentOptions.MidlineLeft;
                dTmp.enableWordWrapping = true;
                dTmp.raycastTarget = false;
                var dle = descGO.AddComponent<LayoutElement>();
                dle.flexibleWidth = 1;
                dle.preferredHeight = 56;
            }
        }
    }
}
