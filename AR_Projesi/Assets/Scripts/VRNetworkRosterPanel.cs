// VRNetworkRosterPanel.cs
// Bağlı oyuncuların listesi + paylaşılan görev ilerlemesi paneli.
// Oturum sırasında açık tutulabilir (örn. bilek menüsünde kısayolu olabilir).

using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.VR
{
    public class VRNetworkRosterPanel : VRUIPanel
    {
        RectTransform _playerListContainer;
        RectTransform _taskListContainer;
        TextMeshProUGUI _emptyTasksLabel;
        TextMeshProUGUI _emptyPlayersLabel;
        Button _closeBtn;
        Button _quickHelloBtn, _quickReadyBtn, _quickHelpBtn;

        readonly List<GameObject> _playerRows = new List<GameObject>();
        readonly List<GameObject> _taskRows = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            Build();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            VRSharedTaskManager.OnTasksChanged += RefreshTasks;
            VRSharedTaskManager.OnTaskProgressChanged += _ => RefreshTasks();
            VRSharedTaskManager.OnTaskCompleted += _ => RefreshTasks();
            VRNetworkManager.OnPlayerJoined += _ => RefreshPlayers();
            VRNetworkManager.OnPlayerLeft += _ => RefreshPlayers();
            RefreshPlayers();
            RefreshTasks();
        }

        void OnDisable()
        {
            VRSharedTaskManager.OnTasksChanged -= RefreshTasks;
        }

        void Build()
        {
            VRUIFactory.CreatePanelBackground(transform);

            // Başlık
            var titleArea = CreateSubRect("Title", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-48, 70));
            var title = VRUIFactory.CreateText(titleArea, "Oturum Durumu",
                VRUITheme.FontTitle, VRUITheme.TextPrimary, TextAlignmentOptions.Center, "Title");
            title.fontStyle = FontStyles.Bold;

            // İki sütun: solda oyuncular, sağda görevler
            float topY = -110;
            float colWidth = 380;
            float colHeight = 480;

            // Oyuncu sütunu
            var playersCol = CreateSubRect("PlayersCol", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(48, topY), new Vector2(colWidth, colHeight));
            var pHeader = VRUIFactory.CreateText(
                CreateChildRect(playersCol, "Header", new Vector2(0,1), new Vector2(1,1),
                                new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(0, 40)),
                "Oyuncular", VRUITheme.FontHeading, VRUITheme.Accent, TextAlignmentOptions.Center, "Header");
            pHeader.fontStyle = FontStyles.Bold;

            _playerListContainer = CreateChildRect(playersCol, "List",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -54), new Vector2(0, colHeight - 60));
            VRUIFactory.AddVerticalLayout(_playerListContainer.gameObject, 8,
                new RectOffset(8, 8, 8, 8), TextAnchor.UpperCenter);

            _emptyPlayersLabel = VRUIFactory.CreateText(
                CreateChildRect(_playerListContainer, "Empty",
                    new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1),
                    Vector2.zero, new Vector2(0, 60)),
                "Oturum yok", VRUITheme.FontSmall, VRUITheme.TextSecondary,
                TextAlignmentOptions.Center, "Empty");

            // Görev sütunu
            var tasksCol = CreateSubRect("TasksCol", new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(-48, topY), new Vector2(colWidth, colHeight));
            var tHeader = VRUIFactory.CreateText(
                CreateChildRect(tasksCol, "Header", new Vector2(0,1), new Vector2(1,1),
                                new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(0, 40)),
                "Ortak Görevler", VRUITheme.FontHeading, VRUITheme.Accent, TextAlignmentOptions.Center, "Header");
            tHeader.fontStyle = FontStyles.Bold;

            _taskListContainer = CreateChildRect(tasksCol, "List",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -54), new Vector2(0, colHeight - 60));
            VRUIFactory.AddVerticalLayout(_taskListContainer.gameObject, 10,
                new RectOffset(8, 8, 8, 8), TextAnchor.UpperCenter);

            _emptyTasksLabel = VRUIFactory.CreateText(
                CreateChildRect(_taskListContainer, "Empty",
                    new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1),
                    Vector2.zero, new Vector2(0, 60)),
                "Görev yok", VRUITheme.FontSmall, VRUITheme.TextSecondary,
                TextAlignmentOptions.Center, "Empty");

            // Hızlı sohbet butonları
            var chatRow = CreateSubRect("ChatRow", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(-48, 70));
            VRUIFactory.AddHorizontalLayout(chatRow.gameObject, 12,
                new RectOffset(0,0,0,0), TextAnchor.MiddleCenter);

            _quickHelloBtn = MakeChatButton(chatRow, "Merhaba", QuickChatMessage.Hello);
            _quickReadyBtn = MakeChatButton(chatRow, "Hazırım", QuickChatMessage.Ready);
            _quickHelpBtn = MakeChatButton(chatRow, "Yardım", QuickChatMessage.Help);

            // Kapat
            _closeBtn = CreatePositionedButton("Kapat", new Vector2(0, 30), new Vector2(260, 60),
                VRUITheme.ButtonNormal, () => Hide(), "Close");
        }

        Button MakeChatButton(Transform parent, string label, QuickChatMessage msg)
        {
            var btn = VRUIFactory.CreateButton(parent, label, new Vector2(180, 60),
                VRUITheme.ButtonNormal, VRUITheme.FontButton, "Chat_" + msg);
            btn.onClick.AddListener(() =>
            {
                // Yerel oyuncu objesini bul ve mesaj yolla
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    var localObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
                    if (localObj != null)
                    {
                        var np = localObj.GetComponent<VRNetworkPlayer>();
                        if (np != null) np.SendQuickChatServerRpc(msg);
                    }
                }
            });
            return btn;
        }

        // ---- Yardımcılar ----

        RectTransform CreateSubRect(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = pivot; rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;
            return rt;
        }

        RectTransform CreateChildRect(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = pivot; rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }

        Button CreatePositionedButton(string label, Vector2 anchoredPos, Vector2 size,
            Color tint, UnityEngine.Events.UnityAction action, string name)
        {
            var holder = CreateSubRect(name + "Holder", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0.5f, 0), anchoredPos, size);
            var btn = VRUIFactory.CreateButton(holder, label, size, tint, VRUITheme.FontButton, name);
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            btn.onClick.AddListener(action);
            return btn;
        }

        // ---- Yenileme ----

        void RefreshPlayers()
        {
            ClearList(_playerRows);
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                _emptyPlayersLabel.gameObject.SetActive(true);
                _emptyPlayersLabel.text = "Oturum yok";
                return;
            }
            _emptyPlayersLabel.gameObject.SetActive(false);

            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                string label = $"Oyuncu {clientId}";
                Color rowColor = Color.white;

                // Player objesinden gerçek bilgileri al
                if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                {
                    var player = client.PlayerObject != null ?
                        client.PlayerObject.GetComponent<VRNetworkPlayer>() : null;
                    if (player != null)
                    {
                        label = player.displayName.Value.ToString();
                        rowColor = player.avatarColor.Value;
                    }
                }
                bool isHost = clientId == NetworkManager.ServerClientId;
                if (isHost) label += " (Sunucu)";
                if (clientId == NetworkManager.Singleton.LocalClientId) label += " — sen";

                AddPlayerRow(label, rowColor);
            }
        }

        void AddPlayerRow(string label, Color color)
        {
            var row = new GameObject("PlayerRow", typeof(RectTransform));
            row.transform.SetParent(_playerListContainer, false);
            var rt = (RectTransform)row.transform;
            rt.sizeDelta = new Vector2(0, 50);

            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 50;
            le.flexibleWidth = 1;

            var bg = row.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.04f);
            bg.sprite = VRUIFactory.RoundedSprite;
            bg.type = Image.Type.Sliced;

            // Renk göstergesi (sol kenar)
            var dotGO = new GameObject("Color", typeof(RectTransform));
            dotGO.transform.SetParent(row.transform, false);
            var drt = (RectTransform)dotGO.transform;
            drt.anchorMin = new Vector2(0, 0.5f); drt.anchorMax = new Vector2(0, 0.5f);
            drt.pivot = new Vector2(0, 0.5f);
            drt.anchoredPosition = new Vector2(12, 0);
            drt.sizeDelta = new Vector2(28, 28);
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.sprite = VRUIFactory.RoundedSprite;
            dotImg.type = Image.Type.Sliced;
            dotImg.color = color;

            // Etiket
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(row.transform, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(54, 4); lrt.offsetMax = new Vector2(-12, -4);
            var t = labelGO.AddComponent<TextMeshProUGUI>();
            t.text = label;
            t.fontSize = VRUITheme.FontBody - 2;
            t.color = VRUITheme.TextPrimary;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;

            _playerRows.Add(row);
        }

        void RefreshTasks()
        {
            ClearList(_taskRows);
            if (VRSharedTaskManager.Instance == null || VRSharedTaskManager.Instance.Tasks == null
                || VRSharedTaskManager.Instance.Tasks.Count == 0)
            {
                _emptyTasksLabel.gameObject.SetActive(true);
                _emptyTasksLabel.text = "Görev yok";
                return;
            }
            _emptyTasksLabel.gameObject.SetActive(false);

            var snapshot = VRSharedTaskManager.Instance.Snapshot();
            foreach (var t in snapshot)
            {
                AddTaskRow(t);
            }
        }

        void AddTaskRow(SharedTaskData t)
        {
            var row = new GameObject("TaskRow_" + t.id, typeof(RectTransform));
            row.transform.SetParent(_taskListContainer, false);
            var rt = (RectTransform)row.transform;
            rt.sizeDelta = new Vector2(0, 80);

            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 80;
            le.flexibleWidth = 1;

            var bg = row.AddComponent<Image>();
            bg.color = t.state == SharedTaskState.Completed ?
                new Color(0.40f, 0.85f, 0.45f, 0.18f) : new Color(1, 1, 1, 0.04f);
            bg.sprite = VRUIFactory.RoundedSprite;
            bg.type = Image.Type.Sliced;

            // Başlık
            var titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(row.transform, false);
            var trt = (RectTransform)titleGO.transform;
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -6);
            trt.sizeDelta = new Vector2(-20, 28);
            var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
            string prefix = t.state == SharedTaskState.Completed ? "✓ " : "○ ";
            titleTmp.text = prefix + t.title.ToString();
            titleTmp.fontSize = VRUITheme.FontBody;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = t.state == SharedTaskState.Completed ? VRUITheme.Success : VRUITheme.TextPrimary;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            titleTmp.raycastTarget = false;

            // Açıklama
            var descGO = new GameObject("Desc", typeof(RectTransform));
            descGO.transform.SetParent(row.transform, false);
            var drt = (RectTransform)descGO.transform;
            drt.anchorMin = new Vector2(0, 0); drt.anchorMax = new Vector2(1, 1);
            drt.offsetMin = new Vector2(10, 10); drt.offsetMax = new Vector2(-10, -38);
            var descTmp = descGO.AddComponent<TextMeshProUGUI>();
            descTmp.text = t.description.ToString();
            descTmp.fontSize = VRUITheme.FontSmall - 2;
            descTmp.color = VRUITheme.TextSecondary;
            descTmp.alignment = TextAlignmentOptions.TopLeft;
            descTmp.raycastTarget = false;
            descTmp.enableWordWrapping = true;

            // İlerleme çubuğu
            var progressGO = new GameObject("Progress", typeof(RectTransform));
            progressGO.transform.SetParent(row.transform, false);
            var prt = (RectTransform)progressGO.transform;
            prt.anchorMin = new Vector2(0, 0); prt.anchorMax = new Vector2(1, 0);
            prt.pivot = new Vector2(0.5f, 0);
            prt.anchoredPosition = new Vector2(0, 4);
            prt.sizeDelta = new Vector2(-16, 6);
            var progressBg = progressGO.AddComponent<Image>();
            progressBg.color = new Color(1, 1, 1, 0.08f);

            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(progressGO.transform, false);
            var frt = (RectTransform)fillGO.transform;
            frt.anchorMin = new Vector2(0, 0); frt.anchorMax = new Vector2(t.NormalizedProgress, 1);
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = t.state == SharedTaskState.Completed ? VRUITheme.Success : VRUITheme.Accent;

            _taskRows.Add(row);
        }

        void ClearList(List<GameObject> list)
        {
            foreach (var go in list) if (go != null) Destroy(go);
            list.Clear();
        }
    }
}
