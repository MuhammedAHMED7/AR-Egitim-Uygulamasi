using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace AREgitim.VR
{
    /// <summary>
    /// Bilgi / yardım paneli. Sekmeli (tab) yapıda yönergeleri ve dersleri gösterir.
    /// İçerik runtime'da set edilebilir; varsayılan içerikleri Bootstrapper doldurur.
    /// </summary>
    public class VRInfoPanel : VRFloatingPanel
    {
        [System.Serializable]
        public class InfoTab
        {
            public string title;
            [TextArea(3, 10)]
            public string content;
            [HideInInspector] public Button tabButton;
        }

        [Header("Sekmeler")]
        public List<InfoTab> tabs = new List<InfoTab>();

        [Header("UI Bağlantıları")]
        public RectTransform tabContainer;       // Sekme butonlarının parent'ı
        public Button tabButtonPrefab;           // Klonlanacak prefab (bootstrapper oluşturur)
        public Text contentText;                 // İçerik metni
        public Text contentTitleText;            // Sekme başlığı (büyük)
        public Button nextButton;
        public Button prevButton;
        public Button closeButton;
        public Text  pageIndicatorText;          // "1 / 3" gibi

        [Header("Stil")]
        public Color activeTabColor   = new Color(0.2f, 0.6f, 1f, 1f);
        public Color inactiveTabColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private int _currentIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            if (closeButton != null) closeButton.onClick.AddListener(() => VRUIManager.Instance?.ClosePanel(this));
            if (nextButton != null)  nextButton.onClick.AddListener(Next);
            if (prevButton != null)  prevButton.onClick.AddListener(Prev);
        }

        public override void OnPanelOpened()
        {
            base.OnPanelOpened();
            BuildTabs();
            ShowTab(_currentIndex);
        }

        public void SetTabs(List<InfoTab> newTabs)
        {
            tabs = newTabs ?? new List<InfoTab>();
            _currentIndex = 0;
            if (gameObject.activeSelf)
            {
                BuildTabs();
                ShowTab(0);
            }
        }

        private void BuildTabs()
        {
            if (tabContainer == null || tabButtonPrefab == null) return;

            // Eskileri temizle
            for (int i = tabContainer.childCount - 1; i >= 0; i--)
            {
                var c = tabContainer.GetChild(i);
                if (c != null) Destroy(c.gameObject);
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                var btn = Instantiate(tabButtonPrefab, tabContainer);
                btn.gameObject.SetActive(true);
                var label = btn.GetComponentInChildren<Text>();
                if (label != null) label.text = tabs[i].title;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowTab(index));
                tabs[i].tabButton = btn;
            }
        }

        public void ShowTab(int index)
        {
            if (tabs.Count == 0) return;
            _currentIndex = Mathf.Clamp(index, 0, tabs.Count - 1);
            var tab = tabs[_currentIndex];
            if (contentText != null)      contentText.text      = tab.content;
            if (contentTitleText != null) contentTitleText.text = tab.title;
            if (pageIndicatorText != null) pageIndicatorText.text = $"{_currentIndex + 1} / {tabs.Count}";

            // Tab stillemesi
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabButton == null) continue;
                var img = tabs[i].tabButton.GetComponent<Image>();
                if (img != null) img.color = (i == _currentIndex) ? activeTabColor : inactiveTabColor;
            }

            if (prevButton != null) prevButton.interactable = _currentIndex > 0;
            if (nextButton != null) nextButton.interactable = _currentIndex < tabs.Count - 1;
        }

        public void Next() => ShowTab(_currentIndex + 1);
        public void Prev() => ShowTab(_currentIndex - 1);
    }
}
