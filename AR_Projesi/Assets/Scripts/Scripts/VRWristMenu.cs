using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// VR'da çok yaygın "wrist menu" kalıbı:
    /// kullanıcı sol kolunu çevirip avcunu yüzüne döndürdüğünde,
    /// bileğin üzerinde küçük bir menü beliriyor (hızlı erişim).
    ///
    /// Bu bileşen sol kumanda transform'una çocuk olarak takılır.
    /// Bilek açısına göre kendini fade in/out yapar.
    /// </summary>
    public class VRWristMenu : MonoBehaviour
    {
        [Header("Görünürlük")]
        [Tooltip("Bilek menüsünün görünmesi için avucun kullanıcıya bakması gerekir.")]
        [Range(0f, 1f)] public float visibilityThreshold = 0.55f;

        [Tooltip("Görünmeye başlamak için açı eşiği yumuşaklığı.")]
        [Range(0f, 1f)] public float fadeSoftness = 0.2f;

        Canvas _canvas;
        CanvasGroup _canvasGroup;
        Transform _head;

        protected void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            Build();
        }

        void Start()
        {
            if (VRUIManager.Instance != null)
                _head = VRUIManager.Instance.headTransform;
            if (_head == null && Camera.main != null)
                _head = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (_head == null) return;

            // Avuç içi yönü: bu menü sol kumandanın transform'una yapışık.
            // Kumandanın "yukarı" yönü (transform.up) avucun normalini temsil ediyor.
            // Avucun kullanıcıya doğru bakması için bu vektörle baş-kola vektörü arasındaki
            // skaler çarpım pozitif (ve eşikten büyük) olmalı.
            Vector3 wristToHead = (_head.position - transform.position).normalized;
            float palmDot = Vector3.Dot(transform.up, wristToHead);

            // 0..1 aralığında saydamlık
            float target = Mathf.InverseLerp(visibilityThreshold - fadeSoftness,
                                             visibilityThreshold + fadeSoftness,
                                             palmDot);

            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, target, Time.unscaledDeltaTime * 8f);
            bool visible = _canvasGroup.alpha > 0.5f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        void Build()
        {
            // Arka plan
            VRUIFactory.CreatePanelBackground(transform, VRUITheme.PanelBackgroundDim);

            // Başlık
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(transform, false);
            var tRT = (RectTransform)titleGO.transform;
            tRT.anchorMin = new Vector2(0, 1);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.pivot = new Vector2(0.5f, 1);
            tRT.anchoredPosition = new Vector2(0, -8);
            tRT.sizeDelta = new Vector2(-16, 36);
            var tTmp = titleGO.AddComponent<TextMeshProUGUI>();
            tTmp.text = "Hızlı Erişim";
            tTmp.fontSize = VRUITheme.FontSmall;
            tTmp.color = VRUITheme.TextSecondary;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.fontStyle = FontStyles.UpperCase;
            tTmp.raycastTarget = false;

            // Saat (basit canlı bilgi öğesi — kullanıcı zamanı görebilsin)
            var clockGO = new GameObject("Clock", typeof(RectTransform));
            clockGO.transform.SetParent(transform, false);
            var clRT = (RectTransform)clockGO.transform;
            clRT.anchorMin = new Vector2(0, 1);
            clRT.anchorMax = new Vector2(1, 1);
            clRT.pivot = new Vector2(0.5f, 1);
            clRT.anchoredPosition = new Vector2(0, -50);
            clRT.sizeDelta = new Vector2(-16, 48);
            var clTmp = clockGO.AddComponent<TextMeshProUGUI>();
            clTmp.fontSize = VRUITheme.FontHeading;
            clTmp.fontStyle = FontStyles.Bold;
            clTmp.color = VRUITheme.Accent;
            clTmp.alignment = TextAlignmentOptions.Center;
            clTmp.raycastTarget = false;
            clockGO.AddComponent<VRClockUpdater>().target = clTmp;

            // Ayraç
            var sep = VRUIFactory.CreateSeparator(transform);
            var sRT = sep.rectTransform;
            sRT.anchorMin = new Vector2(0, 1);
            sRT.anchorMax = new Vector2(1, 1);
            sRT.pivot = new Vector2(0.5f, 1);
            sRT.anchoredPosition = new Vector2(0, -108);

            // Butonlar - dikey
            var btnArea = new GameObject("Buttons", typeof(RectTransform));
            btnArea.transform.SetParent(transform, false);
            var baRT = (RectTransform)btnArea.transform;
            baRT.anchorMin = new Vector2(0, 0);
            baRT.anchorMax = new Vector2(1, 1);
            baRT.offsetMin = new Vector2(12, 12);
            baRT.offsetMax = new Vector2(-12, -120);
            VRUIFactory.AddVerticalLayout(btnArea, spacing: 8,
                padding: new RectOffset(0, 0, 0, 0),
                align: TextAnchor.UpperCenter);

            Vector2 btnSize = new Vector2(0, 56);

            var menuBtn = VRUIFactory.CreateButton(btnArea.transform, "≡ Menü",
                btnSize, VRUITheme.Accent, VRUITheme.FontBody, "WMMenu");
            menuBtn.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.ToggleMainMenu();
            });

            var settingsBtn = VRUIFactory.CreateButton(btnArea.transform, "⚙ Ayarlar",
                btnSize, VRUITheme.ButtonNormal, VRUITheme.FontBody, "WMSettings");
            settingsBtn.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.OpenSettings();
            });

            var recenterBtn = VRUIFactory.CreateButton(btnArea.transform, "⟳ Ortala",
                btnSize, VRUITheme.ButtonNormal, VRUITheme.FontBody, "WMRecenter");
            recenterBtn.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.RequestRecenter();
            });

            var helpBtn = VRUIFactory.CreateButton(btnArea.transform, "? Yardım",
                btnSize, VRUITheme.ButtonNormal, VRUITheme.FontBody, "WMHelp");
            helpBtn.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.OpenHelp();
            });
        }
    }

    /// <summary>
    /// Verilen TMP_Text içine saat:dakika yazar; her saniye günceller.
    /// </summary>
    public class VRClockUpdater : MonoBehaviour
    {
        public TextMeshProUGUI target;
        float _t;

        void Update()
        {
            if (target == null) return;
            _t += Time.unscaledDeltaTime;
            if (_t >= 1f)
            {
                _t = 0f;
                target.text = System.DateTime.Now.ToString("HH:mm");
            }
        }

        void OnEnable()
        {
            if (target != null) target.text = System.DateTime.Now.ToString("HH:mm");
        }
    }
}
