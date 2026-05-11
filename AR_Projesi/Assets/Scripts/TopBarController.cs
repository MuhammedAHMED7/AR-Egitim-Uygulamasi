using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.UI
{
    /// <summary>
    /// Ekranın tepesindeki HUD bar. Model adı + AR oturum durumu + ayarlar butonu.
    /// </summary>
    public class TopBarController : MonoBehaviour
    {
        public TMP_Text modelNameLabel;
        public Image sessionDot;
        public Button settingsButton;

        Color _activeColor = new Color(0.30f, 0.95f, 0.55f);
        Color _idleColor   = new Color(0.95f, 0.40f, 0.40f);
        float _pulseT;
        bool _sessionReady;

        void Start()
        {
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (ARUIManager.Instance != null)
            {
                ARUIManager.Instance.OnModelSelected += HandleModelSelected;
                if (ARUIManager.Instance.CurrentModel != null)
                    HandleModelSelected(ARUIManager.Instance.CurrentModel);
            }
        }

        void OnDestroy()
        {
            if (ARUIManager.Instance != null)
                ARUIManager.Instance.OnModelSelected -= HandleModelSelected;
        }

        void Update()
        {
            // AR oturum durumu yeşil yanıp sönen nokta
            if (sessionDot == null) return;
            if (_sessionReady)
            {
                _pulseT += Time.deltaTime * 2.5f;
                float a = 0.5f + 0.5f * Mathf.Sin(_pulseT);
                var c = _activeColor;
                c.a = Mathf.Lerp(0.4f, 1f, a);
                sessionDot.color = c;
            }
            else
            {
                sessionDot.color = _idleColor;
            }
        }

        public void SetSessionIndicator(bool ready)
        {
            _sessionReady = ready;
        }

        void HandleModelSelected(ARUIManager.ModelData m)
        {
            if (modelNameLabel != null)
                modelNameLabel.text = m != null ? m.displayName : "Model Yok";
        }

        bool _settingsOpen;
        void OnSettingsClicked()
        {
            _settingsOpen = !_settingsOpen;
            if (ARUIManager.Instance != null)
                ARUIManager.Instance.ToggleSettings(_settingsOpen);
        }
    }
}
