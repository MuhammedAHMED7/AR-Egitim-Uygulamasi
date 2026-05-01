using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AREducation.Multiplayer
{
    /// <summary>
    /// AR Eğitim Uygulaması - Çoklu Kullanıcı Ağ Yöneticisi
    /// Unity Relay + Netcode for GameObjects kullanır
    /// ARFoundation ile tam entegrasyon sağlar
    /// </summary>
    public class ARMultiplayerManager : MonoBehaviour
    {
        public static ARMultiplayerManager Instance { get; private set; }

        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARSessionOrigin arSessionOrigin;

        [Header("Network Settings")]
        [SerializeField] private int maxPlayers = 6;
        [SerializeField] private GameObject playerAvatarPrefab;
        [SerializeField] private GameObject arAnchorSyncPrefab;

        [Header("UI References")]
        [SerializeField] private ARMultiplayerUI multiplayerUI;

        // Events
        public event Action<string> OnRoomCreated;
        public event Action<string> OnRoomJoined;
        public event Action<string> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;
        public event Action<string> OnError;
        public event Action OnSessionReady;

        // State
        private string _joinCode;
        private bool _isInitialized;
        private NetworkManager _networkManager;
        private UnityTransport _transport;

        // Bağlı oyuncuların takibi
        private Dictionary<ulong, ARPlayerData> _connectedPlayers = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _networkManager = NetworkManager.Singleton;
            _transport = _networkManager.GetComponent<UnityTransport>();
        }

        private async void Start()
        {
            await InitializeUnityServices();
        }

        // ─── Unity Services Başlatma ───────────────────────────────────────────

        private async Task InitializeUnityServices()
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                _isInitialized = true;
                Debug.Log($"[ARMultiplayer] Unity Services başlatıldı. PlayerID: {AuthenticationService.Instance.PlayerId}");
                multiplayerUI?.SetStatus("Hazır");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ARMultiplayer] Servis başlatma hatası: {e.Message}");
                OnError?.Invoke($"Bağlantı servisi başlatılamadı: {e.Message}");
            }
        }

        // ─── Oda Oluşturma (Host) ──────────────────────────────────────────────

        public async Task<string> CreateRoom()
        {
            if (!_isInitialized)
            {
                OnError?.Invoke("Servisler henüz başlatılmadı.");
                return null;
            }

            try
            {
                multiplayerUI?.SetStatus("Oda oluşturuluyor...");

                // Relay sunucusunda yer ayır
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // Transport'u yapılandır
                _transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                // NetworkManager event'lerini kaydet
                RegisterNetworkEvents();

                // Host olarak başlat
                _networkManager.StartHost();

                Debug.Log($"[ARMultiplayer] Oda oluşturuldu. Katılım kodu: {_joinCode}");
                multiplayerUI?.ShowJoinCode(_joinCode);
                multiplayerUI?.SetStatus($"Oda aktif | Kod: {_joinCode}");
                OnRoomCreated?.Invoke(_joinCode);

                return _joinCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ARMultiplayer] Oda oluşturma hatası: {e.Message}");
                OnError?.Invoke($"Oda oluşturulamadı: {e.Message}");
                return null;
            }
        }

        // ─── Odaya Katılma (Client) ────────────────────────────────────────────

        public async Task JoinRoom(string joinCode)
        {
            if (!_isInitialized)
            {
                OnError?.Invoke("Servisler henüz başlatılmadı.");
                return;
            }

            if (string.IsNullOrWhiteSpace(joinCode) || joinCode.Length != 6)
            {
                OnError?.Invoke("Geçersiz katılım kodu.");
                return;
            }

            try
            {
                multiplayerUI?.SetStatus("Odaya katılınıyor...");

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode.ToUpper());

                _transport.SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                RegisterNetworkEvents();
                _networkManager.StartClient();

                Debug.Log($"[ARMultiplayer] Odaya katılım isteği gönderildi: {joinCode}");
                multiplayerUI?.SetStatus("Bağlanıyor...");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ARMultiplayer] Odaya katılma hatası: {e.Message}");
                OnError?.Invoke($"Odaya katılınamadı: {e.Message}");
            }
        }

        // ─── Bağlantıyı Kes ───────────────────────────────────────────────────

        public void Disconnect()
        {
            if (_networkManager.IsHost)
                _networkManager.Shutdown();
            else if (_networkManager.IsClient)
                _networkManager.Shutdown();

            _connectedPlayers.Clear();
            multiplayerUI?.SetStatus("Bağlantı kesildi");
            Debug.Log("[ARMultiplayer] Bağlantı kesildi.");
        }

        // ─── Network Event Kayıtları ───────────────────────────────────────────

        private void RegisterNetworkEvents()
        {
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            _networkManager.OnServerStarted += OnServerStarted;
        }

        private void UnregisterNetworkEvents()
        {
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            _networkManager.OnServerStarted -= OnServerStarted;
        }

        private void OnServerStarted()
        {
            Debug.Log("[ARMultiplayer] Sunucu başladı.");
            OnSessionReady?.Invoke();
        }

        private void OnClientConnected(ulong clientId)
        {
            var playerName = $"Öğrenci_{clientId}";
            _connectedPlayers[clientId] = new ARPlayerData
            {
                ClientId = clientId,
                PlayerName = playerName,
                JoinTime = DateTime.Now
            };

            int count = _connectedPlayers.Count;
            multiplayerUI?.UpdatePlayerCount(count, maxPlayers);
            multiplayerUI?.AddPlayerToList(playerName);

            OnPlayerJoined?.Invoke(playerName);
            Debug.Log($"[ARMultiplayer] Oyuncu bağlandı: {clientId} | Toplam: {count}");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (_connectedPlayers.TryGetValue(clientId, out var playerData))
            {
                multiplayerUI?.RemovePlayerFromList(playerData.PlayerName);
                OnPlayerLeft?.Invoke(playerData.PlayerName);
                _connectedPlayers.Remove(clientId);
            }

            multiplayerUI?.UpdatePlayerCount(_connectedPlayers.Count, maxPlayers);
            Debug.Log($"[ARMultiplayer] Oyuncu ayrıldı: {clientId}");
        }

        // ─── Yardımcı Metodlar ─────────────────────────────────────────────────

        public int GetConnectedPlayerCount() => _connectedPlayers.Count;
        public string GetJoinCode() => _joinCode;
        public bool IsHost() => _networkManager != null && _networkManager.IsHost;
        public bool IsConnected() => _networkManager != null && _networkManager.IsConnectedClient;

        private void OnDestroy()
        {
            UnregisterNetworkEvents();
        }
    }

    // ─── Yardımcı Veri Sınıfı ─────────────────────────────────────────────────

    [Serializable]
    public class ARPlayerData
    {
        public ulong ClientId;
        public string PlayerName;
        public DateTime JoinTime;
        public Vector3 LastKnownPosition;
    }
}
