using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// Bir nesneyi geçerli teleport zemini olarak işaretler.
    /// TeleportationArea componentinin Awake'inde otomatik yapılandırılır.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VRTeleportTarget : MonoBehaviour
    {
        [Tooltip("Bu zeminde dönüş açısı sabitleniyor mu?")]
        public bool matchOrientation = false;

        TeleportationArea _area;

        void Reset()
        {
            EnsureArea();
        }

        void Awake()
        {
            EnsureArea();
        }

        void EnsureArea()
        {
            _area = GetComponent<TeleportationArea>();
            if (_area == null) _area = gameObject.AddComponent<TeleportationArea>();
            _area.matchOrientation = matchOrientation
                ? MatchOrientation.TargetUpAndForward
                : MatchOrientation.None;
        }
    }
}
