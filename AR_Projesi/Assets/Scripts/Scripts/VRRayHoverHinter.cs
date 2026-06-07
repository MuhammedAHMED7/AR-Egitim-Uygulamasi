using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// XR Ray Interactor'a takılır. Lazer bir objeye değdiğinde, eğer o obje
    /// VRHoverInfo bileşeni taşıyorsa onun metnini tooltip olarak gösterir.
    /// Kullanıcıya görsel geri bildirim sağlar.
    /// </summary>
    [RequireComponent(typeof(XRRayInteractor))]
    public class VRRayHoverHinter : MonoBehaviour
    {
        private XRRayInteractor _ray;
        private VRHoverInfo _currentTarget;

        private void Awake()
        {
            _ray = GetComponent<XRRayInteractor>();
        }

        private void Update()
        {
            if (_ray == null || VRUIManager.Instance == null) return;

            VRHoverInfo info = null;
            Vector3 hitPos = Vector3.zero;

            if (_ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                info = hit.collider != null ? hit.collider.GetComponentInParent<VRHoverInfo>() : null;
                hitPos = hit.point;
            }

            if (info != _currentTarget)
            {
                if (info != null)
                {
                    VRUIManager.Instance.ShowTooltip(info.tooltipText, hitPos);
                }
                else
                {
                    VRUIManager.Instance.HideTooltip();
                }
                _currentTarget = info;
            }
            else if (info != null)
            {
                // Konumu tazele
                VRUIManager.Instance.ShowTooltip(info.tooltipText, hitPos);
            }
        }
    }
}
