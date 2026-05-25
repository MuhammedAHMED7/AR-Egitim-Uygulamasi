using UnityEngine;
using UnityEngine.UI;
using System;

namespace AREgitim.VR
{
    /// <summary>
    /// Ana menü paneli. Modül seçimi, sahne değiştirme, çıkış gibi
    /// üst düzey eylemleri içerir. Bootstrapper tarafından oluşturulur.
    /// </summary>
    public class VRMainMenuPanel : VRFloatingPanel
    {
        [Header("Ana Menü Butonları")]
        public Button startLessonButton;
        public Button switchToARButton;
        public Button settingsButton;
        public Button helpButton;
        public Button quitButton;

        public event Action OnStartLessonClicked;
        public event Action OnSwitchToARClicked;
        public event Action OnQuitClicked;

        protected override void Awake()
        {
            base.Awake();
            BindButtons();
        }

        private void BindButtons()
        {
            if (startLessonButton != null)
                startLessonButton.onClick.AddListener(() =>
                {
                    OnStartLessonClicked?.Invoke();
                    VRUIManager.Instance?.ShowToast("Ders başlatılıyor...", 2f, ToastType.Info);
                    VRUIManager.Instance?.ClosePanel(this);
                });

            if (switchToARButton != null)
                switchToARButton.onClick.AddListener(() =>
                {
                    OnSwitchToARClicked?.Invoke();
                    VRUIManager.Instance?.ShowToast("AR moduna geçiliyor...", 2f, ToastType.Info);
                });

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() =>
                {
                    var ui = VRUIManager.Instance;
                    if (ui == null) return;
                    ui.ClosePanel(this);
                    ui.OpenPanel(ui.settingsPanel);
                });

            if (helpButton != null)
                helpButton.onClick.AddListener(() =>
                {
                    var ui = VRUIManager.Instance;
                    if (ui == null) return;
                    ui.ClosePanel(this);
                    ui.OpenPanel(ui.infoPanel);
                });

            if (quitButton != null)
                quitButton.onClick.AddListener(() =>
                {
                    OnQuitClicked?.Invoke();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
        }
    }
}
