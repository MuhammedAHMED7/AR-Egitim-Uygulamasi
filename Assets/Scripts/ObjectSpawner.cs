using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AREducationApp
{
    /// <summary>
    /// Kullanıcının ekrana dokunarak 3D eğitim materyallerini AR sahnesine yerleştirmesini sağlar.
    /// </summary>
    public class ObjectSpawner : MonoBehaviour
    {
        public static ObjectSpawner Instance { get; private set; }

        [Header("Eğitim Materyalleri")]
        [Tooltip("Yerleştirilecek olan 3D modelin Prefab'ı")]
        public GameObject CurrentModelPrefab;
        
        // Sahneye yerleştirilmiş mevcut obje referansı
        private GameObject _spawnedObject;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            // Eğer UI üzerine dokunuluyorsa, obje yerleştirmeyi iptal et (EventSystem ile entegre edilebilir)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // Yalnızca ekrana ilk dokunulduğunda çalış
                if (touch.phase == TouchPhase.Began)
                {
                    // AR Manager üzerinden zemin algılama kontrolü yap
                    if (ARManager.Instance.RaycastToPlane(touch.position, out Pose hitPose))
                    {
                        SpawnOrMoveObject(hitPose);
                    }
                }
            }
        }

        /// <summary>
        /// Belirtilen noktaya obje yerleştirir veya var olan objeyi o noktaya taşır.
        /// </summary>
        private void SpawnOrMoveObject(Pose hitPose)
        {
            if (CurrentModelPrefab == null)
            {
                Debug.LogWarning("Yerleştirilecek bir Model Prefab'ı seçilmedi!");
                return;
            }

            if (_spawnedObject == null)
            {
                // Obje sahnede yoksa, yeni bir tane oluştur (Instantiate)
                _spawnedObject = Instantiate(CurrentModelPrefab, hitPose.position, hitPose.rotation);
                
                // Objeye etkileşim scripti ekleyelim
                if (_spawnedObject.GetComponent<InteractableObject>() == null)
                {
                    _spawnedObject.AddComponent<InteractableObject>();
                }
            }
            else
            {
                // Obje sahnede varsa, sadece pozisyonunu ve rotasyonunu güncelle
                _spawnedObject.transform.position = hitPose.position;
                _spawnedObject.transform.rotation = hitPose.rotation;
            }
        }

        /// <summary>
        /// Seçili 3D model prefeb'ını değiştirir (Örn: Hücre modelinden, Güneş Sistemi modeline geçiş)
        /// </summary>
        public void SetModelPrefab(GameObject newModel)
        {
            CurrentModelPrefab = newModel;

            // Eğer daha önceden yerleştirilmiş bir obje varsa, onu yok et
            if (_spawnedObject != null)
            {
                Destroy(_spawnedObject);
                _spawnedObject = null;
            }
        }
    }
}
