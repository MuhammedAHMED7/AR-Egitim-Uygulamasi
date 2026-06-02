using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// XR Ray Interactor'ın çizgisini durum bazlı renklendirir:
    /// - Hover yok: hafif gri
    /// - Geçerli UI/nesne üzerine hover: mavi
    /// - Seçim sırasında: yeşil
    ///
    /// XRInteractorLineVisual'a doğrudan renk atayarak çalışır.
    /// </summary>
    [RequireComponent(typeof(XRRayInteractor))]
    public class VRPointerVisuals : MonoBehaviour
    {
        XRRayInteractor _ray;
        XRInteractorLineVisual _lineVisual;

        Gradient _defaultGradient;
        Gradient _hoverGradient;
        Gradient _selectGradient;

        void Awake()
        {
            _ray = GetComponent<XRRayInteractor>();
            _lineVisual = GetComponent<XRInteractorLineVisual>();
            if (_lineVisual == null) _lineVisual = gameObject.AddComponent<XRInteractorLineVisual>();

            _defaultGradient = BuildGradient(VRUITheme.RayDefault, VRUITheme.RayDefault * new Color(1, 1, 1, 0.2f));
            _hoverGradient   = BuildGradient(VRUITheme.RayHover,   VRUITheme.RayHover   * new Color(1, 1, 1, 0.3f));
            _selectGradient  = BuildGradient(VRUITheme.RaySelect,  VRUITheme.RaySelect  * new Color(1, 1, 1, 0.3f));

            ApplyDefault();
        }

        void OnEnable()
        {
            if (_ray == null) return;
            _ray.hoverEntered.AddListener(OnHoverEntered);
            _ray.hoverExited.AddListener(OnHoverExited);
            _ray.selectEntered.AddListener(OnSelectEntered);
            _ray.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            if (_ray == null) return;
            _ray.hoverEntered.RemoveListener(OnHoverEntered);
            _ray.hoverExited.RemoveListener(OnHoverExited);
            _ray.selectEntered.RemoveListener(OnSelectEntered);
            _ray.selectExited.RemoveListener(OnSelectExited);
        }

        void OnHoverEntered(HoverEnterEventArgs args)  => ApplyHover();
        void OnHoverExited(HoverExitEventArgs args)
        {
            // Hala başka bir hover varsa rengi koru
            if (_ray.hasHover) ApplyHover();
            else ApplyDefault();
        }
        void OnSelectEntered(SelectEnterEventArgs args) => ApplySelect();
        void OnSelectExited(SelectExitEventArgs args)
        {
            if (_ray.hasHover) ApplyHover();
            else ApplyDefault();
        }

        public void ApplyDefault()
        {
            if (_lineVisual == null) return;
            _lineVisual.validColorGradient = _defaultGradient;
            _lineVisual.invalidColorGradient = _defaultGradient;
            _lineVisual.lineWidth = 0.005f;
        }
        public void ApplyHover()
        {
            if (_lineVisual == null) return;
            _lineVisual.validColorGradient = _hoverGradient;
            _lineVisual.lineWidth = 0.0075f;
        }
        public void ApplySelect()
        {
            if (_lineVisual == null) return;
            _lineVisual.validColorGradient = _selectGradient;
            _lineVisual.lineWidth = 0.009f;
        }

        static Gradient BuildGradient(Color start, Color end)
        {
            var g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(start, 0f),
                    new GradientColorKey(end,   1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(start.a, 0f),
                    new GradientAlphaKey(end.a,   1f)
                });
            return g;
        }
    }
}
