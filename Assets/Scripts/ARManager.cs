using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AREducationApp
{
    /// <summary>
    /// Bu sınıf Artırılmış Gerçeklik oturumunu yönetir ve düzlem (Plane) algılamasını kontrol eder.
    /// </summary>
    [RequireComponent(typeof(ARSession))]
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARManager : MonoBehaviour
    {
        public static ARManager Instance { get; private set; }

        private ARRaycastManager _raycastManager;
        private ARPlaneManager _planeManager;
        private ARSession _arSession;

        [Header("Ayarlar")]
        public bool ShowPlanes = true;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            _raycastManager = GetComponent<ARRaycastManager>();
            _planeManager = GetComponent<ARPlaneManager>();
            _arSession = GetComponent<ARSession>();
        }

        void Start()
        {
            SetPlaneVisibility(ShowPlanes);
        }

        /// <summary>
        /// Ekrana dokunulan noktadan AR düzlemlerine bir ışın gönderir ve temas noktasını döndürür.
        /// </summary>
        public bool RaycastToPlane(Vector2 screenPosition, out Pose hitPose)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (_raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                hitPose = hits[0].pose;
                return true;
            }

            hitPose = Pose.identity;
            return false;
        }

        /// <summary>
        /// Algılanan zeminlerin (Plane) görünürlüğünü açar veya kapatır.
        /// </summary>
        public void SetPlaneVisibility(bool isVisible)
        {
            ShowPlanes = isVisible;
            foreach (var plane in _planeManager.trackables)
            {
                plane.gameObject.SetActive(isVisible);
            }
            _planeManager.enabled = isVisible;
        }

        /// <summary>
        /// AR Oturumunu sıfırlar. Zemin algılamaları baştan başlar.
        /// </summary>
        public void ResetARSession()
        {
            _arSession.Reset();
        }
    }
}
