// VRNetworkGrabbable.cs
// VRGrabbable'ı genişletir: tutma anında NetworkObject sahipliği aktarılır.
// Böylece istemci yetkili NetworkTransform sayesinde nesne yumuşak hareket eder.
// Çoklu oyuncu olmadığında (network kapalıyken) klasik VRGrabbable gibi davranır.

using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    [RequireComponent(typeof(NetworkObject))]
    public class VRNetworkGrabbable : VRGrabbable
    {
        NetworkObject _netObj;

        protected override void Awake()
        {
            base.Awake();
            _netObj = GetComponent<NetworkObject>();
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            // Sadece ağ etkinse sahiplik kontrol et
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;
            if (_netObj == null || !_netObj.IsSpawned) return;

            // Sahip değilsek talep et
            if (_netObj.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            // İstersen burada sahipliği serbest bırakabilirsin. Şimdilik tutuyoruz
            // (sonraki kullanıcı kendisi talep eder). Bu daha az ağ gürültüsü yaratır.
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestOwnershipServerRpc(ulong newOwnerId)
        {
            if (_netObj == null) return;
            if (!_netObj.IsSpawned) return;
            // Sunucu sahiplik aktarımını yapar
            try
            {
                _netObj.ChangeOwnership(newOwnerId);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VRNetworkGrabbable] Sahiplik aktarım hatası: {ex.Message}");
            }
        }
    }
}
