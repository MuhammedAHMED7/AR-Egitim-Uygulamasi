using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// VR ana menü paneli. Menu düğmesine basıldığında kullanıcının önünde
    /// dünya uzayında belirir. Devam Et, Ayarlar, Yardım, Yeniden Ortala,
    /// Çıkış seçeneklerini sunar.
    /// </summary>
    public class VRMainMenuPanel : VRUIPanel
    {
        TextMeshProUGUI _titleText;
        TextMeshProUGUI _subtitleText;

        protected override void Awake()
        {
            base.Awake();
            Build();
            // Başlangıçta gizli
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        void Build()
        {
            // Bu GO Canvas (CreateWorldCanvas tarafından) zaten ayarlanmış olarak gelir.
            // Sadece içerik kurulur.
            VRUIFactory.CreatePanelBackground(transform);

            // Başlık alanı
            var titleArea = new GameObject("TitleArea", typeof(RectTransform));
            titleArea.transform.SetParent(transform, false);
            var taRT = (RectTransform)titleArea.transform;
            taRT.anchorMin = new Vector2(0, 1);
            taRT.anchorMax = new Vector2(1, 1);
            taRT.pivot = new Vector2(0.5f, 1);
            taRT.anchoredPosition = new Vector2(0, -32);
            taRT.sizeDelta = new Vector2(-48, 140);

            _titleText = VRUIFactory.CreateText(titleArea.transform, "AR Eğitim — VR",
                                                VRUITheme.FontTitle, VRUITheme.TextPrimary,
                                                TextAlignmentOptions.Top, "Title");
            _titleText.fontStyle = FontStyles.Bold;

            // Alt başlık daha küçük, başlığın altına
            var subGO = new GameObject("Subtitle", typeof(RectTransform));
            subGO.transform.SetParent(titleArea.transform, false);
            var subRT = (RectTransform)subGO.transform;
            subRT.anchorMin = new Vector2(0, 0);
            subRT.anchorMax = new Vector2(1, 0);
            subRT.pivot = new Vector2(0.5f, 0);
            subRT.sizeDelta = new Vector2(0, 50);
            subRT.anchoredPosition = new Vector2(0, 6);
            _subtitleText = subGO.AddComponent<TextMeshProUGUI>();
            _subtitleText.text = "Ne yapmak istersin?";
            _subtitleText.fontSize = VRUITheme.FontBody;
            _subtitleText.color = VRUITheme.TextSecondary;
            _subtitleText.alignment = TextAlignmentOptions.Center;
            _subtitleText.raycastTarget = false;

            // Ayraç
            var sep = VRUIFactory.CreateSeparator(transform);
            var sepRT = sep.rectTransform;
            sepRT.anchorMin = new Vector2(0, 1);
            sepRT.anchorMax = new Vector2(1, 1);
            sepRT.pivot = new Vector2(0.5f, 1);
            sepRT.anchoredPosition = new Vector2(0, -180);

            // Butonlar — dikey liste
            var buttonsGO = new GameObject("Buttons", typeof(RectTransform));
            buttonsGO.transform.SetParent(transform, false);
            var bRT = (RectTransform)buttonsGO.transform;
            bRT.anchorMin = new Vector2(0, 0);
            bRT.anchorMax = new Vector2(1, 1);
            bRT.offsetMin = new Vector2(60, 100);
            bRT.offsetMax = new Vector2(-60, -210);

            VRUIFactory.AddVerticalLayout(buttonsGO, spacing: 18,
                padding: new RectOffset(0, 0, 0, 0),
                align: TextAnchor.UpperCenter);

            var sizeBig = new Vector2(0, 90);
            var sizeMed = new Vector2(0, 76);

            var btnContinue = VRUIFactory.CreateButton(buttonsGO.transform, "Devam Et",
                sizeBig, VRUITheme.Accent, VRUITheme.FontHeading, "BtnContinue");
            btnContinue.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.CloseMainMenu();
            });

            var btnSettings = VRUIFactory.CreateButton(buttonsGO.transform, "Ayarlar",
                sizeMed, VRUITheme.ButtonNormal, VRUITheme.FontButton, "BtnSettings");
            btnSettings.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.OpenSettings();
            });

            var btnHelp = VRUIFactory.CreateButton(buttonsGO.transform, "Yardım & İpuçları",
                sizeMed, VRUITheme.ButtonNormal, VRUITheme.FontButton, "BtnHelp");
            btnHelp.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.OpenHelp();
            });

            var btnRecenter = VRUIFactory.CreateButton(buttonsGO.transform, "Görüşü Ortala",
                sizeMed, VRUITheme.ButtonNormal, VRUITheme.FontButton, "BtnRecenter");
            btnRecenter.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.RequestRecenter();
            });

            var btnExit = VRUIFactory.CreateButton(buttonsGO.transform, "Çıkış",
                sizeMed, VRUITheme.Danger, VRUITheme.FontButton, "BtnExit");
            // Çıkışı renk olarak vurgula ama acil eylem
            var exitFb = btnExit.GetComponent<VRButtonFeedback>();
            if (exitFb != null)
            {
                exitFb.normalColor = VRUITheme.Danger;
                exitFb.hoverColor = new Color(1f, 0.55f, 0.55f, 1f);
                exitFb.pressedColor = new Color(0.75f, 0.30f, 0.32f, 1f);
            }
            btnExit.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.RequestExit();
            });

            // Alt bilgi
            var footer = VRUIFactory.CreateText(transform, "Menüyü kapatmak için tekrar Menu'ye bas",
                VRUITheme.FontSmall, VRUITheme.TextSecondary,
                TextAlignmentOptions.Bottom, "Footer");
            var fRT = footer.rectTransform;
            fRT.anchorMin = new Vector2(0, 0);
            fRT.anchorMax = new Vector2(1, 0);
            fRT.pivot = new Vector2(0.5f, 0);
            fRT.anchoredPosition = new Vector2(0, 18);
            fRT.sizeDelta = new Vector2(0, 50);
        }
    }
}
