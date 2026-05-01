using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;

namespace AREducation.Multiplayer
{
    /// <summary>
    /// Çok Kullanıcılı AR Ortamında Paylaşılan Etkileşimli Nesne
    /// Eğitim içeriği (3D model, bilgi paneli vs.) için temel sınıf.
    /// Tüm oyuncular nesneleri taşıyabilir, döndürebilir ve bilgi paylaşabilir.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class SharedARObject : NetworkBehaviour
    {
        [Header("Nesne Ayarları")]
        [SerializeField] private string objectId;
        [SerializeField] private string objectName = "AR Nesne";
        [SerializeField] private bool allowAllPlayersToMove = true;

        [Header("Etkileşim Geri Bildirimi")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Renderer objectRenderer;
        [SerializeField] private AudioClip interactSound;

        // Ağ değişkenleri
        private NetworkVariable<Vector3> _syncPosition =
            new NetworkVariable<Vector3>(Vector3.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<Quaternion> _syncRotation =
            new NetworkVariable<Quaternion>(Quaternion.identity,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<Vector3> _syncScale =
            new NetworkVariable<Vector3>(Vector3.one,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> _isHighlighted =
            new NetworkVariable<bool>(false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<ulong> _currentHolderClientId =
            new NetworkVariable<ulong>(ulong.MaxValue,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        // Events
        public event Action<ulong> OnObjectGrabbed;
        public event Action<ulong> OnObjectReleased;
        public event Action<string> OnAnnotationAdded;

        // Lokal durum
        private bool _isGrabbed;
        private AudioSource _audioSource;
        private float _smoothSpeed = 15f;

        // ─── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (string.IsNullOrEmpty(objectId))
                objectId = Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        public override void OnNetworkSpawn()
        {
            _syncPosition.OnValueChanged += OnPositionChanged;
            _syncRotation.OnValueChanged += OnRotationChanged;
            _syncScale.OnValueChanged += OnScaleChanged;
            _isHighlighted.OnValueChanged += OnHighlightChanged;
            _currentHolderClientId.OnValueChanged += OnHolderChanged;

            // İlk değerleri uygula
            transform.position = _syncPosition.Value;
            transform.rotation = _syncRotation.Value;
            transform.localScale = _syncScale.Value;
        }

        private void Update()
        {
            if (!IsOwner && !_isGrabbed)
            {
                // Uzak değişiklikleri yumuşak interpolasyonla uygula
                transform.position = Vector3.Lerp(
                    transform.position, _syncPosition.Value, Time.deltaTime * _smoothSpeed);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, _syncRotation.Value, Time.deltaTime * _smoothSpeed);
                transform.localScale = Vector3.Lerp(
                    transform.localScale, _syncScale.Value, Time.deltaTime * _smoothSpeed);
            }
        }

        // ─── Nesne Taşıma ──────────────────────────────────────────────────────

        /// <summary>
        /// Oyuncu nesneyi yakalar (tek seferde sadece bir oyuncu taşıyabilir)
        /// </summary>
        public void TryGrab()
        {
            if (!allowAllPlayersToMove && !IsServer) return;
            if (_currentHolderClientId.Value != ulong.MaxValue) return; // Başkası tutuyor

            GrabObjectServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void GrabObjectServerRpc(ulong requestingClientId)
        {
            if (_currentHolderClientId.Value != ulong.MaxValue) return; // Zaten tutulmuş
            _currentHolderClientId.Value = requestingClientId;
            _isHighlighted.Value = true;
        }

        /// <summary>
        /// Oyuncu nesneyi bırakır
        /// </summary>
        public void Release()
        {
            if (_currentHolderClientId.Value != NetworkManager.Singleton.LocalClientId) return;
            ReleaseObjectServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReleaseObjectServerRpc(ulong clientId)
        {
            if (_currentHolderClientId.Value != clientId) return;
            _currentHolderClientId.Value = ulong.MaxValue;
            _isHighlighted.Value = false;
        }

        // ─── Konum / Rotasyon / Ölçek Güncelleme ──────────────────────────────

        /// <summary>
        /// Nesneyi taşı (tutanın frame update'inde çağırır)
        /// </summary>
        public void MoveObject(Vector3 newPosition, Quaternion newRotation)
        {
            if (_currentHolderClientId.Value != NetworkManager.Singleton.LocalClientId) return;
            UpdateTransformServerRpc(newPosition, newRotation, transform.localScale);
        }

        /// <summary>
        /// Nesneyi ölçeklendir (pinch gesture ile)
        /// </summary>
        public void ScaleObject(float scaleFactor)
        {
            Vector3 newScale = transform.localScale * scaleFactor;
            newScale = Vector3.ClampMagnitude(newScale, 3f); // Maksimum boyut
            newScale = Vector3.Max(newScale, Vector3.one * 0.1f); // Minimum boyut
            UpdateTransformServerRpc(transform.position, transform.rotation, newScale);
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateTransformServerRpc(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            _syncPosition.Value = pos;
            _syncRotation.Value = rot;
            _syncScale.Value = scale;
        }

        // ─── Açıklama / Notlar ─────────────────────────────────────────────────

        /// <summary>
        /// Nesneye not/açıklama ekle (tüm oyunculara bildirim gider)
        /// </summary>
        public void AddAnnotation(string text)
        {
            AddAnnotationServerRpc(text, NetworkManager.Singleton.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddAnnotationServerRpc(string text, ulong fromClientId)
        {
            BroadcastAnnotationClientRpc(text, fromClientId);
        }

        [ClientRpc]
        private void BroadcastAnnotationClientRpc(string text, ulong fromClientId)
        {
            OnAnnotationAdded?.Invoke($"Öğrenci {fromClientId + 1}: {text}");
            Debug.Log($"[SharedARObject] Açıklama ({fromClientId}): {text}");
        }

        // ─── Highlight / Parlama ───────────────────────────────────────────────

        private void OnHighlightChanged(bool oldVal, bool newVal)
        {
            if (objectRenderer == null) return;
            objectRenderer.material = newVal ? highlightMaterial : defaultMaterial;

            if (newVal && _audioSource != null && interactSound != null)
                _audioSource.PlayOneShot(interactSound);
        }

        // ─── Değişken Callback'leri ────────────────────────────────────────────

        private void OnPositionChanged(Vector3 oldVal, Vector3 newVal) { /* Update() içinde interpolasyon */ }
        private void OnRotationChanged(Quaternion oldVal, Quaternion newVal) { }
        private void OnScaleChanged(Vector3 oldVal, Vector3 newVal) { }

        private void OnHolderChanged(ulong oldVal, ulong newVal)
        {
            if (newVal == ulong.MaxValue)
                OnObjectReleased?.Invoke(oldVal);
            else
                OnObjectGrabbed?.Invoke(newVal);
        }

        // ─── Getter'lar ────────────────────────────────────────────────────────

        public string ObjectId => objectId;
        public string ObjectName => objectName;
        public bool IsBeingHeld => _currentHolderClientId.Value != ulong.MaxValue;
        public bool IsHeldByLocalPlayer =>
            _currentHolderClientId.Value == NetworkManager.Singleton.LocalClientId;

        public override void OnNetworkDespawn()
        {
            _syncPosition.OnValueChanged -= OnPositionChanged;
            _syncRotation.OnValueChanged -= OnRotationChanged;
            _syncScale.OnValueChanged -= OnScaleChanged;
            _isHighlighted.OnValueChanged -= OnHighlightChanged;
            _currentHolderClientId.OnValueChanged -= OnHolderChanged;
        }
    }
}
