using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace AREgitim.UI
{
    /// <summary>
    /// Zemin reticle (hedefleyici). Eğer sahnede ARRaycastManager varsa
    /// gerçek AR raycast kullanır; yoksa kameranın önünde fallback bir konum gösterir.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ReticleController : MonoBehaviour
    {
        public Transform reticleVisual;
        public Camera arCamera;
        public ARRaycastManager raycastManager;

        static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        public bool HasValidPose { get; private set; }
        public Pose CurrentPose { get; private set; }

        void Awake()
        {
            if (arCamera == null) arCamera = Camera.main;
            if (raycastManager == null) raycastManager = FindObjectOfType<ARRaycastManager>();
        }

        void Update()
        {
            if (reticleVisual == null) return;

            // Önce gerçek AR raycast dene
            if (raycastManager != null && arCamera != null)
            {
                var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                if (raycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    var hit = s_Hits[0].pose;
                    CurrentPose = hit;
                    reticleVisual.position = hit.position;
                    reticleVisual.rotation = hit.rotation;
                    reticleVisual.gameObject.SetActive(true);
                    HasValidPose = true;
                    return;
                }
            }

            // Fallback: kameranın 1.5m önüne yerleştir (Editor / no-AR test)
            if (arCamera != null)
            {
                var fwd = arCamera.transform.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
                fwd.Normalize();
                Vector3 pos = arCamera.transform.position + fwd * 1.5f;
                pos.y = arCamera.transform.position.y - 1f;
                reticleVisual.position = pos;
                reticleVisual.rotation = Quaternion.LookRotation(fwd);
                reticleVisual.gameObject.SetActive(true);
                CurrentPose = new Pose(pos, reticleVisual.rotation);
                HasValidPose = true;
            }
            else
            {
                HasValidPose = false;
            }
        }
    }
}
