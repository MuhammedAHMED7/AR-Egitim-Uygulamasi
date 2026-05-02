using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

namespace AREducation.Multiplayer
{
    /// <summary>
    /// AR Sahnede her oyuncuyu temsil eden avatar bileşeni.
    /// NetworkTransform ile konum senkronizasyonu sağlar.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    public class ARPlayerAvatar : NetworkBehaviour
    {
        [Header("Avatar Görünümü")]
        [SerializeField] private Renderer avatarRenderer;
        [SerializeField] private TextMeshPro playerNameLabel;
        [SerializeField] private GameObject pointerIndicator;
        [SerializeField] private LineRenderer gazeRay;

        [Header("Renkler (Oyuncu Sırası)")]
        [SerializeField] private Color[] playerColors = new Color[]
        {
            new Color(0.2f, 0.6f, 1f),    // Mavi
            new Color(0.2f, 0.9f, 0.4f),  // Yeşil
            new Color(1f, 0.4f, 0.2f),    // Turuncu
            new Color(0.9f, 0.2f, 0.8f),  // Mor
            new Color(1f, 0.85f, 0.1f),   // Sarı
            new Color(0.2f, 0.9f, 0.9f),  // Cyan
        };

        // Ağ üzerinde senkronize edilen değişkenler
        private NetworkVariable<Vector3> _networkPosition =
            new NetworkVariable<Vector3>(Vector3.zero,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        private NetworkVariable<Quaternion> _networkRotation =
            new NetworkVariable<Quaternion>(Quaternion.identity,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        private NetworkVariable<int> _colorIndex =
            new NetworkVariable<int>(0,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        private NetworkVariable<FixedString64Bytes> _playerName =
            new NetworkVariable<FixedString64Bytes>("",
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        // Lokal referanslar
        private Camera _arCamera;
        private float _positionSmoothSpeed = 12f;
        private float _rotationSmoothSpeed = 12f;

        // ─── Lifecycle ─────────────────────────────────────────────────────────

        public override void OnNetworkSpawn()
        {
            _arCamera = Camera.main;

            if (IsOwner)
            {
                // Kendi oyuncumuz: renk ve isim belirle
                int myColorIndex = (int)(OwnerClientId % (ulong)playerColors.Length);
                _colorIndex.Value = myColorIndex;
                _playerName.Value = $"Öğrenci {OwnerClientId + 1}";

                // Lokal avatarı biraz saydam yap
                SetAvatarAlpha(0.5f);
            }
            else
            {
                // Diğer oyuncular: değişkenleri dinle
                _colorIndex.OnValueChanged += OnColorChanged;
                _playerName.OnValueChanged += OnNameChanged;
            }

            // İlk değerleri uygula
            ApplyColor(_colorIndex.Value);
            ApplyName(_playerName.Value.ToString());
        }

        private void Update()
        {
            if (IsOwner)
            {
                // Kendi konumumuzu senkronize et
                SyncLocalTransform();
            }
            else
            {
                // Diğer oyuncuları interpolasyon ile yumuşat
                SmoothRemoteTransform();
            }

            // İsim etiketi her zaman kameraya baksın
            FaceNameLabelToCamera();
        }

        // ─── Konum Senkronizasyonu ─────────────────────────────────────────────

        private void SyncLocalTransform()
        {
            if (_arCamera == null) return;

            // AR kamera konumunu ağa yayınla
            Vector3 camPos = _arCamera.transform.position;
            Quaternion camRot = _arCamera.transform.rotation;

            // Sadece değer değiştiyse yayınla (bant genişliği tasarrufu)
            if (Vector3.Distance(_networkPosition.Value, camPos) > 0.001f)
                _networkPosition.Value = camPos;

            if (Quaternion.Angle(_networkRotation.Value, camRot) > 0.1f)
                _networkRotation.Value = camRot;
        }

        private void SmoothRemoteTransform()
        {
            // Uzak oyuncuların konumlarını interpolasyonla yumuşat
            transform.position = Vector3.Lerp(
                transform.position,
                _networkPosition.Value,
                Time.deltaTime * _positionSmoothSpeed);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                _networkRotation.Value,
                Time.deltaTime * _rotationSmoothSpeed);
        }

        // ─── Renk & İsim ──────────────────────────────────────────────────────

        private void OnColorChanged(int oldVal, int newVal) => ApplyColor(newVal);
        private void OnNameChanged(FixedString64Bytes oldVal, FixedString64Bytes newVal)
            => ApplyName(newVal.ToString());

        private void ApplyColor(int index)
        {
            if (avatarRenderer == null) return;
            Color c = playerColors[index % playerColors.Length];
            avatarRenderer.material.color = c;

            if (gazeRay != null)
            {
                gazeRay.startColor = c;
                gazeRay.endColor = new Color(c.r, c.g, c.b, 0f);
            }
        }

        private void ApplyName(string playerName)
        {
            if (playerNameLabel != null)
                playerNameLabel.text = playerName;
        }

        private void SetAvatarAlpha(float alpha)
        {
            if (avatarRenderer == null) return;
            Color c = avatarRenderer.material.color;
            c.a = alpha;
            avatarRenderer.material.color = c;
        }

        private void FaceNameLabelToCamera()
        {
            if (playerNameLabel == null || _arCamera == null) return;
            playerNameLabel.transform.LookAt(
                playerNameLabel.transform.position + _arCamera.transform.forward);
        }

        // ─── İşaret Göstergesi ─────────────────────────────────────────────────

        /// <summary>
        /// AR nesnesi üzerine işaret/pointer gönder (tüm oyunculara görünür)
        /// </summary>
        [ServerRpc]
        public void PointAtObjectServerRpc(Vector3 worldPosition)
        {
            PointAtObjectClientRpc(worldPosition);
        }

        [ClientRpc]
        private void PointAtObjectClientRpc(Vector3 worldPosition)
        {
            if (pointerIndicator != null)
            {
                pointerIndicator.SetActive(true);
                pointerIndicator.transform.position = worldPosition;
                StartCoroutine(HidePointerAfterDelay(2f));
            }
        }

        private System.Collections.IEnumerator HidePointerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (pointerIndicator != null)
                pointerIndicator.SetActive(false);
        }

        // ─── Oyuncu İsmini Güncelle ────────────────────────────────────────────

        public void SetPlayerName(string newName)
        {
            if (IsOwner)
                _playerName.Value = newName;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                _colorIndex.OnValueChanged -= OnColorChanged;
                _playerName.OnValueChanged -= OnNameChanged;
            }
        }
    }
}
