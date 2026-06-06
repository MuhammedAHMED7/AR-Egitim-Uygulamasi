// VRNetworkManager.cs
// Yüksek seviye ağ API'si. NGO NetworkManager.Singleton üzerine bir sarmalayıcıdır.
// Host/Client/Relay başlatma fonksiyonlarını ve olay yayınlarını sunar.
// Çağrı yapılmadığı sürece ağ pasif kalır — tek oyunculu deneyim etkilenmez.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>
    /// VR çoklu kullanıcı için yüksek seviye yönetici.
    /// Hem LAN (doğrudan IP) hem Relay (internet) bağlantısını destekler.
    /// </summary>
    public class VRNetworkManager : MonoBehaviour
    {
        public static VRNetworkManager Instance { get; private set; }

        [Header("Bağlantı Varsayılanları")]
        public string defaultIP = "127.0.0.1";
        public ushort defaultPort = 7777;
        public int maxConcurrentPlayers = 8;
        public string relayRegion = ""; // Boş = otomatik seçim

        // Geçerli oturum durumu
        public NetworkSessionMode CurrentMode { get; private set; } = NetworkSessionMode.Offline;
        public string LastJoinCode { get; private set; } = "";
        public bool IsConnecting { get; private set; }
        public string LastError { get; private set; } = "";

        // Olaylar
        public static event Action<NetworkSessionMode> OnSessionStarted;
        public static event Action OnSessionStopped;
        public static event Action<ulong> OnPlayerJoined;
        public static event Action<ulong> OnPlayerLeft;
        public static event Action<string> OnConnectionError;
        public static event Action<string> OnRelayCodeReady;

        // Yerel oyuncu adı (PlayerPrefs ile kalıcı)
        private const string PrefDisplayName = "vr_display_name";
        public string LocalDisplayName
        {
            get => PlayerPrefs.GetString(PrefDisplayName, "Oyuncu");
            set { PlayerPrefs.SetString(PrefDisplayName, value); PlayerPrefs.Save(); }
        }

        // Servisler bir kez başlatılır
        private static bool _servicesInitialized;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            // NetworkManager.Singleton hazır olduğunda olayları bağla
            StartCoroutine(BindWhenReady());
        }

        System.Collections.IEnumerator BindWhenReady()
        {
            while (NetworkManager.Singleton == null) yield return null;
            var nm = NetworkManager.Singleton;
            nm.OnClientConnectedCallback += HandleClientConnected;
            nm.OnClientDisconnectCallback += HandleClientDisconnected;
            nm.OnServerStarted += HandleServerStarted;
        }

        void OnDisable()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null)
            {
                nm.OnClientConnectedCallback -= HandleClientConnected;
                nm.OnClientDisconnectCallback -= HandleClientDisconnected;
                nm.OnServerStarted -= HandleServerStarted;
            }
        }

        // ----- LAN (Doğrudan IP) -----

        public bool StartHostLAN(string ip = null, ushort port = 0)
        {
            if (NetworkManager.Singleton == null) { Fail("NetworkManager bulunamadı."); return false; }
            if (NetworkManager.Singleton.IsListening) { Fail("Zaten bir oturumdasın."); return false; }
            var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (t == null) { Fail("UnityTransport eksik."); return false; }
            t.SetConnectionData(string.IsNullOrEmpty(ip) ? defaultIP : ip, port == 0 ? defaultPort : port);
            IsConnecting = true;
            if (!NetworkManager.Singleton.StartHost())
            {
                IsConnecting = false;
                Fail("Host başlatılamadı.");
                return false;
            }
            CurrentMode = NetworkSessionMode.LocalLAN;
            OnSessionStarted?.Invoke(CurrentMode);
            return true;
        }

        public bool StartClientLAN(string ip, ushort port = 0)
        {
            if (NetworkManager.Singleton == null) { Fail("NetworkManager bulunamadı."); return false; }
            if (NetworkManager.Singleton.IsListening) { Fail("Zaten bir oturumdasın."); return false; }
            var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (t == null) { Fail("UnityTransport eksik."); return false; }
            t.SetConnectionData(string.IsNullOrEmpty(ip) ? defaultIP : ip, port == 0 ? defaultPort : port);
            IsConnecting = true;
            if (!NetworkManager.Singleton.StartClient())
            {
                IsConnecting = false;
                Fail("İstemci başlatılamadı.");
                return false;
            }
            CurrentMode = NetworkSessionMode.LocalLAN;
            return true;
        }

        // ----- Relay (İnternet üzerinden) -----

        public async Task<bool> StartHostRelay()
        {
            if (NetworkManager.Singleton == null) { Fail("NetworkManager bulunamadı."); return false; }
            if (NetworkManager.Singleton.IsListening) { Fail("Zaten bir oturumdasın."); return false; }

            try
            {
                IsConnecting = true;
                await EnsureServicesAsync();

                int maxConnections = Mathf.Max(1, maxConcurrentPlayers - 1);
                Allocation alloc = await RelayService.Instance.CreateAllocationAsync(
                    maxConnections, string.IsNullOrEmpty(relayRegion) ? null : relayRegion);
                string code = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
                LastJoinCode = code;

                var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (t == null) { Fail("UnityTransport eksik."); IsConnecting = false; return false; }
                var relayData = new RelayServerData(alloc, "dtls");
                t.SetRelayServerData(relayData);

                if (!NetworkManager.Singleton.StartHost())
                {
                    IsConnecting = false;
                    Fail("Relay host başlatılamadı.");
                    return false;
                }

                CurrentMode = NetworkSessionMode.Relay;
                OnRelayCodeReady?.Invoke(code);
                OnSessionStarted?.Invoke(CurrentMode);
                Debug.Log($"[VRNetworkManager] Relay host hazır. Kod: {code}");
                return true;
            }
            catch (Exception ex)
            {
                IsConnecting = false;
                Fail("Relay hatası: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> JoinRelay(string joinCode)
        {
            if (NetworkManager.Singleton == null) { Fail("NetworkManager bulunamadı."); return false; }
            if (NetworkManager.Singleton.IsListening) { Fail("Zaten bir oturumdasın."); return false; }
            if (string.IsNullOrWhiteSpace(joinCode)) { Fail("Katılım kodu boş olamaz."); return false; }

            try
            {
                IsConnecting = true;
                await EnsureServicesAsync();

                JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(joinCode.Trim().ToUpper());
                var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (t == null) { Fail("UnityTransport eksik."); IsConnecting = false; return false; }
                var relayData = new RelayServerData(join, "dtls");
                t.SetRelayServerData(relayData);

                if (!NetworkManager.Singleton.StartClient())
                {
                    IsConnecting = false;
                    Fail("Relay istemcisi başlatılamadı.");
                    return false;
                }
                CurrentMode = NetworkSessionMode.Relay;
                LastJoinCode = joinCode;
                Debug.Log("[VRNetworkManager] Relay istemcisi bağlanıyor...");
                return true;
            }
            catch (Exception ex)
            {
                IsConnecting = false;
                Fail("Relay katılım hatası: " + ex.Message);
                return false;
            }
        }

        // ----- Ortak -----

        public void Disconnect()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            CurrentMode = NetworkSessionMode.Offline;
            LastJoinCode = "";
            IsConnecting = false;
            OnSessionStopped?.Invoke();
        }

        public bool IsActive => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        public bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        public int ConnectedCount => NetworkManager.Singleton == null ? 0 : NetworkManager.Singleton.ConnectedClientsList.Count;

        public IReadOnlyList<ulong> GetConnectedClientIds()
        {
            if (NetworkManager.Singleton == null) return Array.Empty<ulong>();
            var list = NetworkManager.Singleton.ConnectedClientsIds;
            var copy = new List<ulong>(list);
            return copy;
        }

        // ----- Servis başlatma -----

        async Task EnsureServicesAsync()
        {
            if (_servicesInitialized && AuthenticationService.Instance.IsSignedIn) return;
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            _servicesInitialized = true;
        }

        // ----- Callback'ler -----

        void HandleClientConnected(ulong clientId)
        {
            IsConnecting = false;
            OnPlayerJoined?.Invoke(clientId);
            Debug.Log($"[VRNetworkManager] Oyuncu bağlandı: {clientId}");
        }

        void HandleClientDisconnected(ulong clientId)
        {
            OnPlayerLeft?.Invoke(clientId);
            // Yerel istemci bağlantısı kesildiyse oturumu sıfırla
            if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId
                && !NetworkManager.Singleton.IsServer)
            {
                CurrentMode = NetworkSessionMode.Offline;
                LastJoinCode = "";
                OnSessionStopped?.Invoke();
            }
            Debug.Log($"[VRNetworkManager] Oyuncu ayrıldı: {clientId}");
        }

        void HandleServerStarted()
        {
            Debug.Log("[VRNetworkManager] Sunucu başladı.");
        }

        void Fail(string msg)
        {
            LastError = msg;
            Debug.LogWarning("[VRNetworkManager] " + msg);
            OnConnectionError?.Invoke(msg);
        }
    }
}
