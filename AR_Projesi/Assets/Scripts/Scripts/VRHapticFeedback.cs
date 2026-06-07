using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// VR kumanda titreşimi (haptic) için statik yardımcı.
    /// UI EventSystem'in pointer eventlerinden kumandayı çıkarır
    /// (XR Ray Interactor → XRController) ve haptic impuls gönderir.
    ///
    /// VRSceneBootstrapper kumandalar için ActionBasedController kullandığından
    /// SendHapticImpulse() doğrudan o bileşen üzerinden çalışır.
    /// </summary>
    public static class VRHapticFeedback
    {
        /// <summary>
        /// UI pointer eventinden hangi kumandanın bunu tetiklediğini bulup
        /// titreşim üretir. Mouse vb. ile çağrıldığında sessizce iptal eder.
        /// </summary>
        public static void PulseFromEvent(PointerEventData e, float amplitude, float duration)
        {
            if (e == null) return;
            var module = e.currentInputModule as UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule;
            if (module == null) return;

            // PointerId'den interactor'ı bul
            var interactor = module.GetInteractor(e.pointerId);
            if (interactor == null) return;

            // Interactor'ın XRBaseController'ını bul
            var component = interactor as MonoBehaviour;
            if (component == null) return;

            var ctrl = component.GetComponentInParent<XRBaseController>();
            if (ctrl == null) return;

            ctrl.SendHapticImpulse(Mathf.Clamp01(amplitude), Mathf.Max(0.01f, duration));
        }

        /// <summary>
        /// Belirli bir kumandaya doğrudan titreşim gönderir.
        /// </summary>
        public static void Pulse(XRBaseController controller, float amplitude, float duration)
        {
            if (controller == null) return;
            controller.SendHapticImpulse(Mathf.Clamp01(amplitude), Mathf.Max(0.01f, duration));
        }

        /// <summary>
        /// Her iki kumandaya da titreşim gönderir (örn. başarı/uyarı geri bildirimi).
        /// </summary>
        public static void PulseBoth(float amplitude, float duration)
        {
            var controllers = Object.FindObjectsOfType<XRBaseController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].SendHapticImpulse(Mathf.Clamp01(amplitude), Mathf.Max(0.01f, duration));
            }
        }
    }
}
