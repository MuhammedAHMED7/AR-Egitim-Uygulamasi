using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// Standart UI Button'ı VR'ye yakışır şekilde geliştirir:
    /// - Hover'da haptik titreşim (her iki kumandada da çalışır)
    /// - Hover'da ölçek büyütme (kullanıcı geri bildirimi)
    /// - Tıklamada ses (opsiyonel)
    /// XR Interaction Toolkit'in XRBaseInteractor olaylarıyla çalışır.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class VRUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Hover")]
        [Tooltip("Hover anında uygulanacak ölçek çarpanı")]
        public float hoverScale = 1.08f;

        [Range(1f, 30f)] public float hoverSpeed = 14f;

        [Header("Haptik")]
        [Tooltip("Hover'a girince titreşim göndersin mi?")]
        public bool hapticOnHover = true;
        [Range(0f, 1f)] public float hoverHapticAmplitude = 0.3f;
        public float hoverHapticDuration = 0.05f;

        public bool hapticOnClick = true;
        [Range(0f, 1f)] public float clickHapticAmplitude = 0.6f;
        public float clickHapticDuration = 0.08f;

        [Header("Ses")]
        public AudioClip hoverClip;
        public AudioClip clickClip;
        public AudioSource audioSource;

        private Vector3 _baseScale;
        private Vector3 _targetScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _targetScale = _baseScale;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale,
                Time.deltaTime * hoverSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetScale = _baseScale * hoverScale;
            TrySendHaptic(eventData, hapticOnHover ? hoverHapticAmplitude : 0f, hoverHapticDuration);
            PlayClip(hoverClip);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetScale = _baseScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TrySendHaptic(eventData, hapticOnClick ? clickHapticAmplitude : 0f, clickHapticDuration);
            PlayClip(clickClip);
        }

        private void TrySendHaptic(PointerEventData data, float amp, float dur)
        {
            if (amp <= 0f) return;

#if XR_INTERACTION_AVAILABLE || true
            // XR Interaction Toolkit'in XRRayInteractor olaylarına pointer event'inden ulaşmak için
            // event'in sender'ı bir XRRayInteractor olabilir. ITV 2.6.3'te XRUIInputModule pointer'a
            // 'interactor' alanı koyar (XRBaseInteractor türünde).
            // Type-safe erişim için reflection yerine TryGetComponent ile kontrol ediyoruz.
            var go = data.pointerEnter != null ? data.pointerEnter : gameObject;
            // Pointer'ın hangi kontrolörden geldiğini çıkarmak için pointerId'yi de kullanabiliriz,
            // ancak en güvenilir yol XRUIInputModule.GetTrackedDevice gibi API'lar olmaktadır.
            // Burada her iki XRBaseController'ı bulup ikisine de hafif bir titreşim gönderiyoruz
            // (kullanıcı için doğru kumandayı tahmin etmektense güvenli düşmek).
            var controllers = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.XRBaseController>();
            foreach (var c in controllers)
            {
                if (c == null) continue;
                c.SendHapticImpulse(amp, dur);
            }
#endif
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;
            if (audioSource != null) audioSource.PlayOneShot(clip);
            else AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
