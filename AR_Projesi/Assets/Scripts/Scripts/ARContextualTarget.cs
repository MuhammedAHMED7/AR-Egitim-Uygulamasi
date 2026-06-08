using UnityEngine;

namespace ARApp.Core.ContextualLabel
{
    /// <summary>
    /// 3D model üzerine yerleþtirilen, mesafe tabanlý tetiklemeyi ve etiket konumlandýrmasýný yöneten bileþendir.
    /// </summary>
    [RequireComponent(typeof(Collider))] // Raycast tespiti ve dokunma için Collider zorunludur.
    public class ARContextualTarget : MonoBehaviour
    {
        [Header("Veri ve UI Ayarlarý")]
        [SerializeField] private ContextualLabelData labelData; // Bu modele ait ScriptableObject verisi
        [SerializeField] private GameObject labelPrefab;      // Proje panelindeki ContextualLabelCanvas prefab'ý
        [SerializeField] private float verticalOffset = 0.3f;   // Modelin üstünde ne kadar havada duracak?

        [Header("Mesafe Algýlama Ayarlarý")]
        [SerializeField] private bool useProximityThreshold = true;
        [SerializeField] private float activationDistance = 1.5f; // Kaç metreden sonra etiket otomatik açýlsýn?

        private ContextualLabelController _instantiatedLabel;
        private Transform _cameraTransform;
        private float _sqrActivationDistance;

        private void Start()
        {
            // Performans için ana kamera transform referansý cache'lenir.
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            // Ahmet'in performans standartlarý için: sqrMagnitude optimizasyonunda mesafenin karesi önceden hesaplanýr.
            _sqrActivationDistance = activationDistance * activationDistance;

            InitializeLabel();
        }

        private void Update()
        {
            if (useProximityThreshold)
            {
                EvaluateProximity();
            }
        }

        /// <summary>
        /// Etiket prefab'ýný dinamik olarak instantiate eder ve modelin pivot noktasýna göre konumlandýrýr.
        /// </summary>
        private void InitializeLabel()
        {
            if (labelPrefab == null || labelData == null) return;

            // Modelin dünya üzerindeki merkez/pivot noktasýna dikey ofset eklenerek spawn konumu hesaplanýr.
            Vector3 spawnPosition = transform.position + (Vector3.up * verticalOffset);

            // Etiketi modelin bir alt nesnesi (Child) olarak doðuruyoruz. Böylece model hareket ederse etiket de onunla taþýnýr.
            GameObject spawnedObj = Instantiate(labelPrefab, spawnPosition, Quaternion.identity, transform);
            _instantiatedLabel = spawnedObj.GetComponent<ContextualLabelController>();

            if (_instantiatedLabel != null)
            {
                _instantiatedLabel.Initialize(labelData);
            }
        }

        /// <summary>
        /// Kamera ile model arasýndaki mesafeyi sqrMagnitude ile performanslý þekilde ölçer.
        /// </summary>
        private void EvaluateProximity()
        {
            if (_cameraTransform == null || _instantiatedLabel == null) return;

            // Vector3.Distance yerine sqrMagnitude kullanarak CPU üzerindeki aðýr karekök (Sqrt) yükünü eliyoruz.
            Vector3 offset = transform.position - _cameraTransform.position;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance <= _sqrActivationDistance)
            {
                if (!_instantiatedLabel.IsVisible) _instantiatedLabel.SetVisibility(true);
            }
            else
            {
                if (_instantiatedLabel.IsVisible) _instantiatedLabel.SetVisibility(false);
            }
        }

        /// <summary>
        /// Ekrana dokunulduðunda (TouchRaycastManager tarafýndan) dýþarýdan tetiklenecek fonksiyon.
        /// </summary>
        public void HandleTouchInteraction()
        {
            if (_instantiatedLabel != null)
            {
                // Mevcut görünürlük durumunun tam tersine çekerek Aç/Kapat (Toggle) yapar.
                _instantiatedLabel.SetVisibility(!_instantiatedLabel.IsVisible);
            }
        }
    }
}
