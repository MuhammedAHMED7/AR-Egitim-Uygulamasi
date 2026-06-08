using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// VR butonlarına eklenir. Pointer (XR ray) butonun üzerine geldiğinde
    /// rengini değiştirir ve kumandada hafif bir titreşim üretir.
    /// Tıklamada daha güçlü titreşim ve renk feedback'i verir.
    ///
    /// Hem standart UI EventSystem (PointerEnter/Exit/Down/Up) hem de
    /// XR Ray Interactor'un hover'ını destekler.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class VRButtonFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler
    {
        [Header("Renkler")]
        public Graphic targetGraphic;
        public Color normalColor   = new Color(0.15f, 0.18f, 0.24f, 1f);
        public Color hoverColor    = new Color(0.22f, 0.28f, 0.36f, 1f);
        public Color pressedColor  = new Color(0.12f, 0.15f, 0.20f, 1f);

        [Header("Geçiş hızı")]
        [Range(0f, 0.4f)] public float transitionDuration = 0.08f;

        bool _isHovered;
        bool _isPressed;
        Color _currentTarget;
        float _t;

        void Awake()
        {
            if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
            _currentTarget = normalColor;
            if (targetGraphic != null) targetGraphic.color = normalColor;
        }

        void Update()
        {
            if (targetGraphic == null) return;
            if (targetGraphic.color != _currentTarget)
            {
                _t += Time.unscaledDeltaTime;
                float k = transitionDuration <= 0f ? 1f : Mathf.Clamp01(_t / transitionDuration);
                targetGraphic.color = Color.Lerp(targetGraphic.color, _currentTarget, k);
            }
        }

        void SetTarget(Color c)
        {
            _currentTarget = c;
            _t = 0f;
        }

        public void OnPointerEnter(PointerEventData e)
        {
            _isHovered = true;
            SetTarget(_isPressed ? pressedColor : hoverColor);
            VRHapticFeedback.PulseFromEvent(e, VRUITheme.HapticHoverAmplitude, VRUITheme.HapticHoverDuration);
        }

        public void OnPointerExit(PointerEventData e)
        {
            _isHovered = false;
            SetTarget(normalColor);
        }

        public void OnPointerDown(PointerEventData e)
        {
            _isPressed = true;
            SetTarget(pressedColor);
        }

        public void OnPointerUp(PointerEventData e)
        {
            _isPressed = false;
            SetTarget(_isHovered ? hoverColor : normalColor);
        }

        public void OnPointerClick(PointerEventData e)
        {
            VRHapticFeedback.PulseFromEvent(e, VRUITheme.HapticClickAmplitude, VRUITheme.HapticClickDuration);
        }
    }
}
