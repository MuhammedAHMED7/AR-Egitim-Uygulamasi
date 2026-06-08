// VRMultiplayerBootstrapper.cs
// Çoklu kullanıcı altyapısının ana sahne kurucusu.
// VRUIBootstrapper'dan SONRA çalışır (-850 < -900 değil; -850 > -900 yani sonra).
//
// Görevleri:
//   1) NetworkManager GameObject'ini oluştur (NetworkManager + UnityTransport)
//   2) Player prefab'ını runtime'da oluştur (NetworkObject + VRNetworkPlayer + VRNetworkAvatar)
//   3) Paylaşılan görev yöneticisini ve açıklama yöneticisini spawn etmek için
//      scene-spawn prefab'larını hazırla
//   4) VRNetworkManager singleton'unu kur
//   5) UI: Oturum paneli + Roster panelini ekleyip VRUIManager'a bağla
//   6) VRMainMenuPanel'e "Çok Oyunculu" butonu ekle (varsa atla)
//   7) EventSystem'e XRUIInputModule ekle (yoksa)
//
// Ağ oturumu KENDİLİĞİNDEN başlamaz — kullanıcı oturum panelinden seçim yapar.
//
// NGO 1.15.1 NOTU: Runtime'da oluşturulan NetworkObject prefab'ları için
// GlobalObjectIdHash alanı reflection ile manuel olarak set edilir
// (NGO editör derleme aşamasında otomatik atar, runtime'da değil).

using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AREgitim.VR
{
    [DefaultExecutionOrder(-850)]
    public class VRMultiplayerBootstrapper : MonoBehaviour
    {
        [Header("Ağ Yapılandırması")]
        public string defaultIP = "127.0.0.1";
        public ushort defaultPort = 7777;
        public int maxConcurrentPlayers = 8;

        [Header("Demo İçerik")]
        [Tooltip("Sahnede 2 oyuncu için bir işbirliği bölgesi oluştur.")]
        public bool spawnDemoCollaborationZone = true;
        [Tooltip("İşbirliği bölgesinin konumu.")]
        public Vector3 demoZonePosition = new Vector3(3f, 0.05f, 2f);

        // Çalışma anında oluşturulan referanslar
        NetworkManager _networkManager;
        UnityTransport _transport;
        GameObject _playerPrefab;
        GameObject _taskManagerPrefab;
        GameObject _annotationPrefab;
        GameObject _collaborationZonePrefab;

        // UI referansları
        VRNetworkSessionPanel _sessionPanel;
        VRNetworkRosterPanel _rosterPanel;

        void Awake()
        {
            BuildNetworkManager();
            BuildPlayerPrefab();
            BuildTaskManagerPrefab();
            BuildAnnotationPrefab();
            BuildCollaborationZonePrefab();
            BuildVRNetworkManagerSingleton();
            BuildNetworkUI();
            ExtendMainMenuWithMultiplayer();
            EnsureXRUIInputModule();

            // Server başladığında sahne-spawn objelerini oluştur
            if (_networkManager != null)
            {
                _networkManager.OnServerStarted += OnServerStarted;
            }
        }

        void OnDestroy()
        {
            if (_networkManager != null) _networkManager.OnServerStarted -= OnServerStarted;
        }

        // ---- 1) NetworkManager + UnityTransport ----

        void BuildNetworkManager()
        {
            // Mevcut NetworkManager varsa onu kullan
            if (NetworkManager.Singleton != null)
            {
                _networkManager = NetworkManager.Singleton;
                _transport = _networkManager.GetComponent<UnityTransport>();
                if (_transport == null) _transport = _networkManager.gameObject.AddComponent<UnityTransport>();
                return;
            }
            var go = new GameObject("VR_NetworkManager");
            DontDestroyOnLoad(go);
            _networkManager = go.AddComponent<NetworkManager>();
            _transport = go.AddComponent<UnityTransport>();
            _transport.SetConnectionData(defaultIP, defaultPort);

            // NetworkConfig ayarları
            _networkManager.NetworkConfig.NetworkTransport = _transport;
            _networkManager.NetworkConfig.ConnectionApproval = false;
            _networkManager.NetworkConfig.EnableSceneManagement = false; // Tek sahne, ek sahne yönetimi yok
        }

        // ---- 2) Player prefab (NetworkObject + VRNetworkPlayer + Avatar) ----

        void BuildPlayerPrefab()
        {
            _playerPrefab = new GameObject("VRPlayerPrefab");
            var netObj = _playerPrefab.AddComponent<NetworkObject>();
            ForceGlobalObjectIdHash(netObj, 0xA110C001u); // benzersiz, sabit
            _playerPrefab.AddComponent<VRNetworkPlayer>();
            _playerPrefab.AddComponent<VRNetworkAvatar>();

            // Prefab'ı sahnede aktif tutma — NGO doğru kurulum gerektirir
            _playerPrefab.SetActive(false);
            DontDestroyOnLoad(_playerPrefab);

            // NetworkManager'a kaydet
            _networkManager.NetworkConfig.PlayerPrefab = _playerPrefab;
            _networkManager.AddNetworkPrefab(_playerPrefab);
        }

        // ---- 3) Paylaşılan görev yöneticisi prefab'ı ----

        void BuildTaskManagerPrefab()
        {
            _taskManagerPrefab = new GameObject("VRSharedTaskManagerPrefab");
            var no = _taskManagerPrefab.AddComponent<NetworkObject>();
            ForceGlobalObjectIdHash(no, 0xA110C002u);
            _taskManagerPrefab.AddComponent<VRSharedTaskManager>();
            _taskManagerPrefab.SetActive(false);
            DontDestroyOnLoad(_taskManagerPrefab);
            _networkManager.AddNetworkPrefab(_taskManagerPrefab);
        }

        void BuildAnnotationPrefab()
        {
            _annotationPrefab = new GameObject("VRSharedAnnotationPrefab");
            var no = _annotationPrefab.AddComponent<NetworkObject>();
            ForceGlobalObjectIdHash(no, 0xA110C003u);
            _annotationPrefab.AddComponent<VRSharedAnnotation>();
            _annotationPrefab.SetActive(false);
            DontDestroyOnLoad(_annotationPrefab);
            _networkManager.AddNetworkPrefab(_annotationPrefab);
        }

        void BuildCollaborationZonePrefab()
        {
            _collaborationZonePrefab = new GameObject("VRCollaborationZonePrefab");
            var nobj = _collaborationZonePrefab.AddComponent<NetworkObject>();
            ForceGlobalObjectIdHash(nobj, 0xA110C004u);
            var col = _collaborationZonePrefab.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.0f;
            _collaborationZonePrefab.AddComponent<VRTaskCollaborationZone>();
            _collaborationZonePrefab.SetActive(false);
            DontDestroyOnLoad(_collaborationZonePrefab);
            _networkManager.AddNetworkPrefab(_collaborationZonePrefab);
        }

        /// <summary>
        /// NetworkObject.GlobalObjectIdHash 'internal' bir alan olduğu için
        /// runtime'da reflection ile set edilir. NGO 1.15.1'de bu alan editör
        /// derleme aşamasında otomatik atanır; runtime'da oluşturulan prefab'lar
        /// için bu çözüm gereklidir.
        /// </summary>
        static void ForceGlobalObjectIdHash(NetworkObject netObj, uint hash)
        {
            var t = typeof(NetworkObject);
            var field = t.GetField("GlobalObjectIdHash",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(netObj, hash);
                return;
            }
            // Alternatif: property olarak deneyelim
            var prop = t.GetProperty("GlobalObjectIdHash",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(netObj, hash);
                return;
            }
            Debug.LogWarning("[VRMultiplayerBootstrapper] GlobalObjectIdHash set edilemedi; ağ prefab'ı düzgün çalışmayabilir.");
        }

        // ---- 4) Yüksek seviye yönetici ----

        void BuildVRNetworkManagerSingleton()
        {
            if (VRNetworkManager.Instance != null) return;
            var go = new GameObject("VR_NetworkManagerWrapper");
            var mgr = go.AddComponent<VRNetworkManager>();
            mgr.defaultIP = defaultIP;
            mgr.defaultPort = defaultPort;
            mgr.maxConcurrentPlayers = maxConcurrentPlayers;
            DontDestroyOnLoad(go);
        }

        // ---- 5) Oturum / Roster panelleri ----

        void BuildNetworkUI()
        {
            if (VRUIManager.Instance == null)
            {
                Debug.LogWarning("[VRMultiplayerBootstrapper] VRUIManager bulunamadı, panel kurulumu atlandı.");
                return;
            }

            // Oturum paneli
            var sessionCanvas = VRUIFactory.CreateWorldCanvas("VR_NetworkSessionPanel",
                null, 900f, 920f);
            _sessionPanel = sessionCanvas.gameObject.AddComponent<VRNetworkSessionPanel>();
            DontDestroyOnLoad(sessionCanvas.gameObject);

            // Roster paneli
            var rosterCanvas = VRUIFactory.CreateWorldCanvas("VR_NetworkRosterPanel",
                null, 900f, 720f);
            _rosterPanel = rosterCanvas.gameObject.AddComponent<VRNetworkRosterPanel>();
            DontDestroyOnLoad(rosterCanvas.gameObject);
        }

        // ---- 6) Ana menüye "Çok Oyunculu" butonu ----

        void ExtendMainMenuWithMultiplayer()
        {
            if (VRUIManager.Instance == null) return;
            var mainMenu = VRUIManager.Instance.mainMenu;
            if (mainMenu == null) return;

            // Buton zaten var mı?
            var existing = mainMenu.transform.Find("Buttons/BtnMultiplayer");
            if (existing != null) return;

            var buttons = mainMenu.transform.Find("Buttons");
            if (buttons == null)
            {
                Debug.LogWarning("[VRMultiplayerBootstrapper] Ana menü buton konteyneri bulunamadı.");
                return;
            }

            var btn = VRUIFactory.CreateButton(buttons, "Çok Oyunculu",
                new Vector2(0, 76),
                VRUITheme.Accent,
                VRUITheme.FontButton,
                "BtnMultiplayer");

            // Yeni buton listenin sonuna eklenir; siblings'in arasında 4. konuma getir (Devam, Ayarlar, ..., Çok Oyunculu, ..., Çıkış)
            // Vertical layout otomatik düzenler. Sadece pozisyon olarak ortaya alalım:
            btn.transform.SetSiblingIndex(Mathf.Max(0, buttons.childCount - 2));

            btn.onClick.AddListener(() =>
            {
                if (VRUIManager.Instance != null) VRUIManager.Instance.CloseMainMenu();
                OpenSessionPanel();
            });
        }

        public void OpenSessionPanel()
        {
            if (_sessionPanel == null) return;
            _sessionPanel.gameObject.SetActive(true);
            _sessionPanel.PositionInFrontOfUser();
            _sessionPanel.Show();
        }

        public void OpenRosterPanel()
        {
            if (_rosterPanel == null) return;
            _rosterPanel.gameObject.SetActive(true);
            _rosterPanel.PositionInFrontOfUser();
            _rosterPanel.Show();
        }

        // ---- 7) XRUIInputModule garantisi ----

        void EnsureXRUIInputModule()
        {
            var es = FindObjectOfType<EventSystem>();
            if (es == null) return;
            var xrModule = es.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            if (xrModule == null)
            {
                // Standart InputSystemUIInputModule var olabilir; ek olarak XRUIInputModule ekle
                es.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            }
        }

        // ---- Sunucu başladığında sahne-spawn ----

        void OnServerStarted()
        {
            if (!_networkManager.IsServer) return;
            // Görev yöneticisini spawn et
            if (_taskManagerPrefab != null)
            {
                var inst = Instantiate(_taskManagerPrefab);
                inst.SetActive(true);
                var no = inst.GetComponent<NetworkObject>();
                no.Spawn(true);
            }
            // Açıklama yöneticisini spawn et
            if (_annotationPrefab != null)
            {
                var inst = Instantiate(_annotationPrefab);
                inst.SetActive(true);
                var no = inst.GetComponent<NetworkObject>();
                no.Spawn(true);
            }
            // Demo işbirliği bölgesi
            if (spawnDemoCollaborationZone && _collaborationZonePrefab != null)
            {
                var inst = Instantiate(_collaborationZonePrefab, demoZonePosition, Quaternion.identity);
                inst.SetActive(true);
                var zone = inst.GetComponent<VRTaskCollaborationZone>();
                zone.taskId = 1; // VRSharedTaskManager'daki ilk görev
                var no = inst.GetComponent<NetworkObject>();
                no.Spawn(true);
            }

            Debug.Log("[VRMultiplayerBootstrapper] Sunucu hazır — paylaşılan nesneler spawn edildi.");
        }
    }
}
