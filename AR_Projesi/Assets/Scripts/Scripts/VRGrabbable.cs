using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// VR ortamında tutulabilen ve kullanılabilen nesneler için temel davranış.
    /// XR Grab Interactable üzerine "kullanma" (activate) eylemini ekler:
    /// tetiği tutarken karşı butona basınca nesne kendi UseAction() metodunu çalıştırır.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VRGrabbable : XRGrabInteractable
    {
        [Header("Davranış")]
        [Tooltip("Bu nesnenin görüntü adı (UI / bildirim için).")]
        public string displayName = "Nesne";

        [Tooltip("Bu nesne 'kullanılabilir' mi? Kullanılırsa Use() çağrılır.")]
        public bool isUsable = true;

        [Tooltip("Nesne tutulduğunda bir kez gösterilecek ipucu metni.")]
        public string grabHint = "";

        [Header("Görsel geri bildirim")]
        [Tooltip("Tutulduğunda öne çıkacak vurgu rengi.")]
        public Color highlightColor = new Color(0.30f, 0.78f, 1f, 1f);

        Color[] _originalColors;
        Renderer[] _renderers;
        bool _isHighlighted;

        protected override void Awake()
        {
            base.Awake();
            _renderers = GetComponentsInChildren<Renderer>();
            CacheOriginalColors();
        }

        void CacheOriginalColors()
        {
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r != null && r.material != null && r.material.HasProperty("_Color"))
                    _originalColors[i] = r.material.color;
                else if (r != null && r.material != null && r.material.HasProperty("_BaseColor"))
                    _originalColors[i] = r.material.GetColor("_BaseColor");
                else
                    _originalColors[i] = Color.white;
            }
        }

        // ───────── Olay kancaları ─────────
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            SetHighlight(true);
            VRInteractionManager.NotifyGrabbed(this);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            SetHighlight(false);
            VRInteractionManager.NotifyReleased(this);
        }

        protected override void OnActivated(ActivateEventArgs args)
        {
            base.OnActivated(args);
            if (isUsable) UseAction();
        }

        // ───────── Vurgu ─────────
        void SetHighlight(bool on)
        {
            if (_renderers == null || _renderers.Length == 0) return;
            _isHighlighted = on;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null || r.material == null) continue;
                Color c = on ? highlightColor : _originalColors[i];
                if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", c);
                else if (r.material.HasProperty("_Color")) r.material.SetColor("_Color", c);
            }
        }

        // ───────── Türetilmiş sınıflar için ─────────
        /// <summary>
        /// Nesnenin "kullanılması" olayı. Türetilmiş sınıflar override edebilir.
        /// </summary>
        protected virtual void UseAction()
        {
            VRInteractionManager.NotifyUsed(this);
        }
    }
}
