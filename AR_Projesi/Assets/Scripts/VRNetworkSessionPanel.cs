// VRNetworkSessionPanel.cs
// Çoklu kullanıcı oturum yönetimi paneli: LAN ve Relay üzerinden host/katıl.
// VRUIPanel'i miras alır; VRUIFactory pattern'ini takip eder.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.VR
{
    public class VRNetworkSessionPanel : VRUIPanel
    {
        TMP_InputField _ipField;
        TMP_InputField _joinCodeField;
        TMP_InputField _displayNameField;
        TextMeshProUGUI _statusText;
        TextMeshProUGUI _relayCodeText;
        Button _hostLanBtn, _joinLanBtn, _hostRelayBtn, _joinRelayBtn, _disconnectBtn, _closeBtn;

        protected override void Awake()
        {
            base.Awake();
            Build();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            VRNetworkManager.OnSessionStarted += OnSessionStarted;
            VRNetworkManager.OnSessionStopped += OnSessionStopped;
            VRNetworkManager.OnConnectionError += OnError;
            VRNetworkManager.OnRelayCodeReady += OnRelayCode;
            VRNetworkManager.OnPlayerJoined += OnPlayerCountChanged;
            VRNetworkManager.OnPlayerLeft += OnPlayerCountChanged;
            RefreshStatus();
        }

        void OnDisable()
        {
            VRNetworkManager.OnSessionStarted -= OnSessionStarted;
            VRNetworkManager.OnSessionStopped -= OnSessionStopped;
            VRNetworkManager.OnConnectionError -= OnError;
            VRNetworkManager.OnRelayCodeReady -= OnRelayCode;
            VRNetworkManager.OnPlayerJoined -= OnPlayerCountChanged;
            VRNetworkManager.OnPlayerLeft -= OnPlayerCountChanged;
        }

        void Build()
        {
            VRUIFactory.CreatePanelBackground(transform);

            // Başlık
            var titleArea = CreateSubRect("TitleArea", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-48, 110));
            var title = VRUIFactory.CreateText(titleArea, "Çoklu Kullanıcı",
                VRUITheme.FontTitle, VRUITheme.TextPrimary,
                TextAlignmentOptions.Top, "Title");
            title.fontStyle = FontStyles.Bold;

            // Alt başlık
            var subRect = CreateSubRect("Subtitle", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(-48, 36));
            var sub = VRUIFactory.CreateText(subRect, "Aynı sanal ortamda birlikte çalışın.",
                VRUITheme.FontBody, VRUITheme.TextSecondary,
                TextAlignmentOptions.Center, "SubtitleText");

            // Ad alanı
            var nameLabelRect = CreateSubRect("NameLabel", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(80, -150), new Vector2(220, 36));
            VRUIFactory.CreateText(nameLabelRect, "Görünen ad:",
                VRUITheme.FontBody, VRUITheme.TextSecondary, TextAlignmentOptions.MidlineLeft, "Lbl");

            _displayNameField = CreateInputField(new Vector2(330, -150), new Vector2(420, 56),
                VRNetworkManager.Instance != null ? VRNetworkManager.Instance.LocalDisplayName : "Oyuncu",
                "Adın", "NameField");
            _displayNameField.onEndEdit.AddListener(n =>
            {
                if (VRNetworkManager.Instance != null) VRNetworkManager.Instance.LocalDisplayName = n;
            });

            // LAN bölümü başlık + ayraç
            CreateSectionHeader("LANSection", "Yerel Ağ (LAN)", new Vector2(0, -210));

            var ipLabelRect = CreateSubRect("IpLabel", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(80, -270), new Vector2(180, 36));
            VRUIFactory.CreateText(ipLabelRect, "IP adresi:",
                VRUITheme.FontBody, VRUITheme.TextSecondary, TextAlignmentOptions.MidlineLeft, "Lbl");

            _ipField = CreateInputField(new Vector2(290, -270), new Vector2(460, 56),
                "127.0.0.1", "IP", "IpField");

            _hostLanBtn = CreatePositionedButton("Oda Aç (Host)", new Vector2(-180, -340),
                new Vector2(320, 70), VRUITheme.Accent, OnHostLanClicked, "HostLan");

            _joinLanBtn = CreatePositionedButton("Odaya Katıl", new Vector2(180, -340),
                new Vector2(320, 70), VRUITheme.ButtonNormal, OnJoinLanClicked, "JoinLan");

            // Relay bölümü
            CreateSectionHeader("RelaySection", "İnternet (Relay)", new Vector2(0, -420));

            _relayCodeText = CreateSubRectText("RelayCode", new Vector2(0, -470), new Vector2(700, 50),
                "", VRUITheme.FontHeading, VRUITheme.Accent, FontStyles.Bold);

            var jcLabelRect = CreateSubRect("JcLabel", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(80, -520), new Vector2(200, 36));
            VRUIFactory.CreateText(jcLabelRect, "Katılım kodu:",
                VRUITheme.FontBody, VRUITheme.TextSecondary, TextAlignmentOptions.MidlineLeft, "Lbl");

            _joinCodeField = CreateInputField(new Vector2(310, -520), new Vector2(440, 56),
                "", "Kod", "JoinCodeField");
            _joinCodeField.characterLimit = 8;

            _hostRelayBtn = CreatePositionedButton("İnternet Oda Aç", new Vector2(-180, -590),
                new Vector2(320, 70), VRUITheme.Accent, OnHostRelayClicked, "HostRelay");

            _joinRelayBtn = CreatePositionedButton("Kodla Katıl", new Vector2(180, -590),
                new Vector2(320, 70), VRUITheme.ButtonNormal, OnJoinRelayClicked, "JoinRelay");

            // Durum
            _statusText = CreateSubRectText("Status", new Vector2(0, -660), new Vector2(820, 40),
                "Bağlı değil", VRUITheme.FontBody, VRUITheme.TextSecondary, FontStyles.Italic);

            // Alt eylem butonları
            _disconnectBtn = CreatePositionedButton("Bağlantıyı Kes", new Vector2(-170, -720),
                new Vector2(300, 60), VRUITheme.Danger, OnDisconnectClicked, "Disc");

            _closeBtn = CreatePositionedButton("Kapat", new Vector2(170, -720),
                new Vector2(300, 60), VRUITheme.ButtonNormal, () => Hide(), "Close");
        }

        // ---- UI yardımcıları ----

        RectTransform CreateSubRect(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        void CreateSectionHeader(string name, string label, Vector2 anchoredPos)
        {
            var headerRect = CreateSubRect(name, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), anchoredPos, new Vector2(-48, 40));
            var t = VRUIFactory.CreateText(headerRect, label,
                VRUITheme.FontHeading, VRUITheme.Accent, TextAlignmentOptions.Center, "Header");
            t.fontStyle = FontStyles.Bold;

            // Ayraç
            var sepRect = CreateSubRect(name + "Sep", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), anchoredPos + new Vector2(0, -32), new Vector2(-120, 2));
            var sepImg = sepRect.gameObject.AddComponent<Image>();
            sepImg.color = VRUITheme.Accent;
            sepImg.raycastTarget = false;
        }

        TextMeshProUGUI CreateSubRectText(string name, Vector2 anchoredPos, Vector2 size,
            string content, int fontSize, Color color, FontStyles style)
        {
            var r = CreateSubRect(name, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), anchoredPos, size);
            var t = VRUIFactory.CreateText(r, content, fontSize, color,
                TextAlignmentOptions.Center, name + "Txt");
            t.fontStyle = style;
            return t;
        }

        Button CreatePositionedButton(string label, Vector2 anchoredPos, Vector2 size,
            Color tint, UnityEngine.Events.UnityAction action, string name)
        {
            var holder = CreateSubRect(name + "Holder", new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0.5f, 1), anchoredPos, size);
            var btn = VRUIFactory.CreateButton(holder, label, size, tint, VRUITheme.FontButton, name);
            // Button is sized internally; anchor to fill the holder
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            brt.sizeDelta = Vector2.zero;
            btn.onClick.AddListener(action);
            return btn;
        }

        TMP_InputField CreateInputField(Vector2 anchoredPos, Vector2 size,
            string initial, string placeholderText, string name)
        {
            // Konteynır
            var holder = CreateSubRect(name + "Holder", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), anchoredPos, size);
            var img = holder.gameObject.AddComponent<Image>();
            img.color = new Color(0.10f, 0.12f, 0.15f, 0.92f);
            img.sprite = VRUIFactory.RoundedSprite;
            img.type = Image.Type.Sliced;

            // Text görüntüleme alanı
            var textArea = new GameObject("TextArea", typeof(RectTransform));
            textArea.transform.SetParent(holder, false);
            var tar = (RectTransform)textArea.transform;
            tar.anchorMin = Vector2.zero; tar.anchorMax = Vector2.one;
            tar.offsetMin = new Vector2(18, 6); tar.offsetMax = new Vector2(-18, -6);
            var rectMask = textArea.AddComponent<RectMask2D>();

            var placeholder = new GameObject("Placeholder", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            placeholder.transform.SetParent(textArea.transform, false);
            placeholder.text = placeholderText;
            placeholder.fontSize = VRUITheme.FontBody;
            placeholder.color = new Color(1, 1, 1, 0.35f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.raycastTarget = false;
            var prt = placeholder.rectTransform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            var textComp = new GameObject("Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            textComp.transform.SetParent(textArea.transform, false);
            textComp.text = initial;
            textComp.fontSize = VRUITheme.FontBody;
            textComp.color = VRUITheme.TextPrimary;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;
            textComp.raycastTarget = false;
            var trt = textComp.rectTransform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            var input = holder.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = tar;
            input.textComponent = textComp;
            input.placeholder = placeholder;
            input.text = initial;
            input.targetGraphic = img;
            input.fontAsset = textComp.font;
            return input;
        }

        // ---- Eylemler ----

        void OnHostLanClicked()
        {
            if (VRNetworkManager.Instance == null) return;
            _statusText.text = "Yerel oda açılıyor…";
            VRNetworkManager.Instance.StartHostLAN(_ipField.text, 7777);
        }

        void OnJoinLanClicked()
        {
            if (VRNetworkManager.Instance == null) return;
            _statusText.text = "Bağlanılıyor…";
            VRNetworkManager.Instance.StartClientLAN(_ipField.text, 7777);
        }

        async void OnHostRelayClicked()
        {
            if (VRNetworkManager.Instance == null) return;
            SetButtonsInteractable(false);
            _statusText.text = "Relay servisine bağlanılıyor…";
            await VRNetworkManager.Instance.StartHostRelay();
            SetButtonsInteractable(true);
        }

        async void OnJoinRelayClicked()
        {
            if (VRNetworkManager.Instance == null) return;
            string code = _joinCodeField.text;
            if (string.IsNullOrWhiteSpace(code))
            {
                _statusText.text = "Lütfen katılım kodunu gir.";
                return;
            }
            SetButtonsInteractable(false);
            _statusText.text = $"Koda bağlanılıyor: {code}";
            await VRNetworkManager.Instance.JoinRelay(code);
            SetButtonsInteractable(true);
        }

        void OnDisconnectClicked()
        {
            if (VRNetworkManager.Instance == null) return;
            VRNetworkManager.Instance.Disconnect();
        }

        void SetButtonsInteractable(bool v)
        {
            if (_hostLanBtn) _hostLanBtn.interactable = v;
            if (_joinLanBtn) _joinLanBtn.interactable = v;
            if (_hostRelayBtn) _hostRelayBtn.interactable = v;
            if (_joinRelayBtn) _joinRelayBtn.interactable = v;
        }

        // ---- Olay dinleyicileri ----

        void OnSessionStarted(NetworkSessionMode mode) { RefreshStatus(); }
        void OnSessionStopped() { if (_relayCodeText) _relayCodeText.text = ""; RefreshStatus(); }
        void OnError(string msg) { if (_statusText) _statusText.text = "Hata: " + msg; }
        void OnRelayCode(string code)
        {
            if (_relayCodeText) _relayCodeText.text = $"Katılım Kodu: {code}";
            if (_statusText) _statusText.text = "Diğer oyuncular bu kodla katılabilir.";
        }
        void OnPlayerCountChanged(ulong _) { RefreshStatus(); }

        void RefreshStatus()
        {
            if (_statusText == null) return;
            if (VRNetworkManager.Instance == null) { _statusText.text = "Ağ yöneticisi yok"; return; }
            var nm = VRNetworkManager.Instance;
            if (!nm.IsActive) { _statusText.text = "Bağlı değil"; return; }
            string mode = nm.CurrentMode == NetworkSessionMode.Relay ? "İnternet (Relay)" : "Yerel (LAN)";
            string role = nm.IsHost ? "Sunucu" : "İstemci";
            _statusText.text = $"{role} • {mode} • Oyuncu: {nm.ConnectedCount}";
        }
    }
}
