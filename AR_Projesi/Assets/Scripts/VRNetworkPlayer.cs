// VRNetworkPlayer.cs
// Her bağlanan oyuncu için bir örnek spawn edilir.
// SAHİP istemci: yerel XR rig'inden kafa ve iki el pozlarını her karede
// NetworkVariable'lara yazar. DİĞER istemciler bu değerleri okuyup
// yumuşatma uygulayarak görsel avatarı hareket ettirir.

using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace AREgitim.VR
{
    [DefaultExecutionOrder(-200)]
    public class VRNetworkPlayer : NetworkBehaviour
    {
        // ---- Ağ üzerinden senkronize edilen değişkenler ----
        // Sahip yazma izniyle: sadece sahibi olan istemci yazabilir, sunucu broadcast eder.
        // Bu, VR transformlarının lokalden gelmesi için idealdir.

        public readonly NetworkVariable<NetworkPose> headPose =
            new(NetworkPose.Identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public readonly NetworkVariable<NetworkPose> leftHandPose =
            new(NetworkPose.Identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public readonly NetworkVariable<NetworkPose> rightHandPose =
            new(NetworkPose.Identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public readonly NetworkVariable<HandState> leftHandState =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public readonly NetworkVariable<HandState> rightHandState =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public readonly NetworkVariable<FixedString64Bytes> displayName =
            new(new FixedString64Bytes("Oyuncu"), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public readonly NetworkVariable<Color> avatarColor =
            new(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // İşaret etme (uzaktan göstergeç) — opsiyonel
        public readonly NetworkVariable<bool> isPointing =
            new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // ---- Yerel referanslar (sadece sahip için) ----
        Transform _localHead;
        Transform _localLeftHand;
        Transform _localRightHand;
        UnityEngine.XR.Interaction.Toolkit.ActionBasedController _localLeftCtrl;
        UnityEngine.XR.Interaction.Toolkit.ActionBasedController _localRightCtrl;

        // ---- Avatar görseli ----
        public VRNetworkAvatar Avatar { get; private set; }

        // Rastgele bir renk üretmek için statik palet
        static readonly Color[] _palette = new Color[]
        {
            new Color(0.30f, 0.78f, 1.00f), // mavi
            new Color(1.00f, 0.55f, 0.30f), // turuncu
            new Color(0.55f, 0.85f, 0.45f), // yeşil
            new Color(0.95f, 0.40f, 0.55f), // pembe
            new Color(0.85f, 0.75f, 0.30f), // sarı
            new Color(0.65f, 0.45f, 0.95f), // mor
            new Color(0.40f, 0.85f, 0.85f), // turkuaz
            new Color(1.00f, 0.85f, 0.60f)  // krem
        };

        public override void OnNetworkSpawn()
        {
            // Avatar'ı oluştur (her örnekte). Sahip kendi avatarını gizler.
            Avatar = gameObject.GetComponent<VRNetworkAvatar>();
            if (Avatar == null) Avatar = gameObject.AddComponent<VRNetworkAvatar>();
            Avatar.Initialize(this);

            if (IsOwner)
            {
                // Yerel XR rig referanslarını çöz
                ResolveLocalRig();
                // İsim ve rengi yaz
                string name = VRNetworkManager.Instance != null ? VRNetworkManager.Instance.LocalDisplayName : "Oyuncu";
                displayName.Value = new FixedString64Bytes(name);
                Color c = _palette[(int)(OwnerClientId % (ulong)_palette.Length)];
                avatarColor.Value = c;
                // Kendi avatarımızı gizle (kafamızın içinde olmamalı)
                Avatar.SetVisibleForOwner(false);
            }
            else
            {
                Avatar.SetVisibleForOwner(true);
            }

            // Tüm istemcilerde isim ve renk değişikliklerine abone ol
            displayName.OnValueChanged += (_, v) => Avatar?.SetDisplayName(v.ToString());
            avatarColor.OnValueChanged += (_, v) => Avatar?.SetColor(v);
            // İlk değerleri de uygula
            Avatar?.SetDisplayName(displayName.Value.ToString());
            Avatar?.SetColor(avatarColor.Value);
        }

        public override void OnNetworkDespawn()
        {
            // Avatar otomatik yok edilir (component aynı GO'da)
        }

        void ResolveLocalRig()
        {
            // VRSceneBootstrapper'ın oluşturduğu yapıyı kullan
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                if (xrOrigin.Camera != null) _localHead = xrOrigin.Camera.transform;
            }
            // Kumandaları ada göre bul
            var controllers = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
            foreach (var c in controllers)
            {
                string n = c.gameObject.name.ToLower();
                if (n.Contains("left")) { _localLeftCtrl = c; _localLeftHand = c.transform; }
                else if (n.Contains("right")) { _localRightCtrl = c; _localRightHand = c.transform; }
            }
        }

        void Update()
        {
            if (!IsOwner || !IsSpawned) return;

            // Eksik referansları yakalama (XR rig geç gelirse)
            if (_localHead == null || _localLeftHand == null || _localRightHand == null)
            {
                ResolveLocalRig();
            }

            if (_localHead != null)
            {
                headPose.Value = NetworkPose.FromTransform(_localHead);
            }
            if (_localLeftHand != null)
            {
                leftHandPose.Value = NetworkPose.FromTransform(_localLeftHand);
                if (_localLeftCtrl != null)
                {
                    leftHandState.Value = new HandState
                    {
                        grip = SafeRead(_localLeftCtrl.selectActionValue),
                        trigger = SafeRead(_localLeftCtrl.activateActionValue),
                        isGrabbing = SafeRead(_localLeftCtrl.selectActionValue) > 0.5f
                    };
                }
            }
            if (_localRightHand != null)
            {
                rightHandPose.Value = NetworkPose.FromTransform(_localRightHand);
                if (_localRightCtrl != null)
                {
                    rightHandState.Value = new HandState
                    {
                        grip = SafeRead(_localRightCtrl.selectActionValue),
                        trigger = SafeRead(_localRightCtrl.activateActionValue),
                        isGrabbing = SafeRead(_localRightCtrl.selectActionValue) > 0.5f
                    };
                }
            }
        }

        static float SafeRead(UnityEngine.InputSystem.InputActionProperty p)
        {
            try
            {
                if (p.action == null) return 0f;
                return p.action.ReadValue<float>();
            }
            catch { return 0f; }
        }

        /// <summary>Diğer oyunculara bir QuickChat mesajı gönder (RPC).</summary>
        [ServerRpc(RequireOwnership = true)]
        public void SendQuickChatServerRpc(QuickChatMessage msg)
        {
            BroadcastQuickChatClientRpc(msg, OwnerClientId);
        }

        [ClientRpc]
        void BroadcastQuickChatClientRpc(QuickChatMessage msg, ulong fromClient)
        {
            if (VRUIManager.Instance != null)
            {
                string sender = displayName.Value.ToString();
                string body = QuickChatStrings.Get(msg);
                VRUIManager.Instance.ShowNotification(
                    $"{sender}: {body}",
                    VRNotificationController.NotificationType.Info);
            }
        }
    }
}
