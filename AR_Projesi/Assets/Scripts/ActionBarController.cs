using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.UI
{
    /// <summary>
    /// Carousel'in altındaki üç buton: Ses Komutu, Yerleştir, Sıfırla.
    /// </summary>
    public class ActionBarController : MonoBehaviour
    {
        public Button voiceButton;
        public Button placeButton;
        public Button resetButton;

        void Start()
        {
            if (voiceButton != null) voiceButton.onClick.AddListener(OnVoice);
            if (placeButton != null) placeButton.onClick.AddListener(OnPlace);
            if (resetButton != null) resetButton.onClick.AddListener(OnReset);
        }

        void OnVoice()
        {
            if (ARUIManager.Instance != null) ARUIManager.Instance.RequestVoiceCommand();
        }

        void OnPlace()
        {
            if (ARUIManager.Instance != null) ARUIManager.Instance.RequestPlace();
        }

        void OnReset()
        {
            if (ARUIManager.Instance != null) ARUIManager.Instance.RequestReset();
        }
    }
}
