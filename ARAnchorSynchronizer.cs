using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AREducation.Multiplayer
{
    /// <summary>
    /// AR Dünya Çıpası Senkronizasyonu
    /// Host'un tarattığı AR düzlemini tüm istemcilerle paylaşır.
    /// Tüm öğrencilerin aynı sanal koordinat sisteminde görüntü görmesini sağlar.
    /// </summary>
    public class ARAnchorSynchronizer : NetworkBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private ARRaycastManager raycastManager;

        [Header("Senkronizasyon Ayarları")]
        [SerializeField] private float anchorSyncInterval = 0.5f;
        [SerializeField] private bool autoSyncOnPlaneDetected = true;

        // Ağ değişkenleri - dünya orijin dönüşümü
        private NetworkVariable<Vector3> _worldOriginPosition =
            new NetworkVariable<Vector3>(Vector3.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<Quaternion> _worldOriginRotation =
            new NetworkVariable<Quaternion>(Quaternion.identity,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> _anchorEstablished =
            new NetworkVariable<bool>(false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private ARAnchor _sharedAnchor;
        private bool _localAnchorApplied;

        // ─── Lifecycle ─────────────────────────────────────────────────────────

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                // İstemci: çıpa kurulunca uygula
                _anchorEstablished.OnValueChanged += OnAnchorEstablishedChanged;

                if (_anchorEstablished.Value)
                    ApplyAnchorToLocal();
            }
            else
            {
                // Host: düzlem tespitini dinle
                if (autoSyncOnPlaneDetected && planeManager != null)
                    planeManager.planesChanged += OnPlanesChanged;
            }
        }

        // ─── Host: Düzlem Tespit Edilince ──────────────────────────────────────

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            if (_anchorEstablished.Value) return;
            if (args.added.Count == 0) return;

            // İlk tespit edilen düzlemin merkezini çıpa olarak kullan
            ARPlane firstPlane = args.added[0];
            EstablishSharedAnchor(firstPlane.center, firstPlane.transform.rotation);
        }

        /// <summary>
        /// Paylaşılan çıpayı belirtilen konuma kur (Host çağırır)
        /// </summary>
        public void EstablishSharedAnchor(Vector3 worldPosition, Quaternion worldRotation)
        {
            if (!IsServer) return;

            _worldOriginPosition.Value = worldPosition;
            _worldOriginRotation.Value = worldRotation;
            _anchorEstablished.Value = true;

            Debug.Log($"[ARAnchorSync] Paylaşılan çıpa kuruldu: {worldPosition}");
            NotifyAnchorEstablishedClientRpc(worldPosition, worldRotation);
        }

        // ─── İstemci: Çıpa Kurulunca Uygula ───────────────────────────────────

        private void OnAnchorEstablishedChanged(bool oldVal, bool newVal)
        {
            if (newVal && !_localAnchorApplied)
                ApplyAnchorToLocal();
        }

        private void ApplyAnchorToLocal()
        {
            StartCoroutine(ApplyAnchorCoroutine());
        }

        private IEnumerator ApplyAnchorCoroutine()
        {
            // AR sistemi stabil olana kadar bekle
            yield return new WaitForSeconds(0.3f);

            Vector3 targetPos = _worldOriginPosition.Value;
            Quaternion targetRot = _worldOriginRotation.Value;

            // AR Session Origin'i hizala
            ARSessionOrigin sessionOrigin = FindObjectOfType<ARSessionOrigin>();
            if (sessionOrigin != null)
            {
                sessionOrigin.MakeContentAppearAt(
                    sessionOrigin.transform,
                    targetPos,
                    targetRot
                );
                _localAnchorApplied = true;
                Debug.Log("[ARAnchorSync] Lokal AR çıpası uygulandı.");
            }
        }

        // ─── RPC'ler ──────────────────────────────────────────────────────────

        [ClientRpc]
        private void NotifyAnchorEstablishedClientRpc(Vector3 position, Quaternion rotation)
        {
            if (IsServer) return; // Host zaten biliyor
            Debug.Log($"[ARAnchorSync] Sunucudan çıpa alındı: {position}");
        }

        /// <summary>
        /// İstemci, çıpayı doğrulamak için geri bildirim gönderir
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ConfirmAnchorServerRpc(ulong clientId, bool success)
        {
            Debug.Log($"[ARAnchorSync] Oyuncu {clientId} çıpa doğrulama: {success}");
        }

        // ─── Manuel Çıpa Kurma (Dokunmatik) ───────────────────────────────────

        private void Update()
        {
            if (!IsServer) return;
            if (_anchorEstablished.Value) return;

            // Host ekrana dokunarak çıpa kurar
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                var hits = new System.Collections.Generic.List<ARRaycastHit>();
                if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    EstablishSharedAnchor(hitPose.position, hitPose.rotation);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer)
                _anchorEstablished.OnValueChanged -= OnAnchorEstablishedChanged;
            else if (planeManager != null)
                planeManager.planesChanged -= OnPlanesChanged;
        }

        // ─── Hata Ayıklama ─────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            if (!_anchorEstablished.Value) return;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_worldOriginPosition.Value, 0.05f);
            Gizmos.DrawRay(_worldOriginPosition.Value, _worldOriginRotation.Value * Vector3.up * 0.3f);
        }
    }
}
