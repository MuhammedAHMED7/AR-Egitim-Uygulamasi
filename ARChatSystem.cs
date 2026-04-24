using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AREducation.Multiplayer
{
    /// <summary>
    /// AR Çoklu Kullanıcı Mesajlaşma Sistemi
    /// Öğrenciler arasında metin tabanlı iletişim sağlar.
    /// Sesli sohbet için Vivox veya Agora entegrasyonu önerilir.
    /// </summary>
    public class ARChatSystem : NetworkBehaviour
    {
        public static ARChatSystem Instance { get; private set; }

        [Header("Ayarlar")]
        [SerializeField] private int maxMessageHistory = 50;
        [SerializeField] private float messageDisplayDuration = 5f;
        [SerializeField] private ARChatUI chatUI;

        // Events
        public event Action<ChatMessage> OnMessageReceived;
        public event Action<ulong, bool> OnPlayerMicStatusChanged;

        // Mesaj geçmişi (lokal)
        private Queue<ChatMessage> _messageHistory = new();

        // Hızlı mesajlar (ders sırasında tek dokunuşla gönderim)
        private static readonly string[] QuickMessages = new[]
        {
            "Anladım! ✓",
            "Tekrar açıklar mısın?",
            "Harika fikir!",
            "Dur, bir saniye...",
            "Hazırım!",
            "Yardım lazım 🙋"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── Mesaj Gönderme ────────────────────────────────────────────────────

        /// <summary>
        /// Tüm oyunculara mesaj gönder
        /// </summary>
        public void SendMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            text = text.Substring(0, Mathf.Min(text.Length, 200)); // Max 200 karakter

            ulong senderId = NetworkManager.Singleton.LocalClientId;
            SendMessageServerRpc(text, senderId);
        }

        /// <summary>
        /// Hızlı mesaj gönder
        /// </summary>
        public void SendQuickMessage(int index)
        {
            if (index < 0 || index >= QuickMessages.Length) return;
            SendMessage(QuickMessages[index]);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendMessageServerRpc(string text, ulong senderId)
        {
            BroadcastMessageClientRpc(text, senderId, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        [ClientRpc]
        private void BroadcastMessageClientRpc(string text, ulong senderId, long timestamp)
        {
            ChatMessage message = new ChatMessage
            {
                Text = text,
                SenderId = senderId,
                SenderName = $"Öğrenci {senderId + 1}",
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime,
                IsOwnMessage = senderId == NetworkManager.Singleton.LocalClientId
            };

            AddToHistory(message);
            OnMessageReceived?.Invoke(message);
            chatUI?.DisplayMessage(message);
        }

        // ─── Öğretmen Duyurusu ─────────────────────────────────────────────────

        /// <summary>
        /// Sunucu/öğretmen tüm öğrencilere özel duyuru gönderir
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SendAnnouncementServerRpc(string announcement)
        {
            if (!IsServer) return;
            DisplayAnnouncementClientRpc(announcement);
        }

        [ClientRpc]
        private void DisplayAnnouncementClientRpc(string announcement)
        {
            ChatMessage message = new ChatMessage
            {
                Text = announcement,
                SenderId = ulong.MaxValue,
                SenderName = "Öğretmen",
                Timestamp = DateTime.Now,
                IsAnnouncement = true
            };

            AddToHistory(message);
            OnMessageReceived?.Invoke(message);
            chatUI?.DisplayAnnouncement(announcement);
        }

        // ─── Mikrofon Durum Bildirimi ──────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void SetMicStatusServerRpc(bool isActive, ulong clientId)
        {
            BroadcastMicStatusClientRpc(isActive, clientId);
        }

        [ClientRpc]
        private void BroadcastMicStatusClientRpc(bool isActive, ulong clientId)
        {
            OnPlayerMicStatusChanged?.Invoke(clientId, isActive);
            chatUI?.UpdateMicStatus(clientId, isActive);
        }

        // ─── Mesaj Geçmişi ─────────────────────────────────────────────────────

        private void AddToHistory(ChatMessage message)
        {
            _messageHistory.Enqueue(message);
            while (_messageHistory.Count > maxMessageHistory)
                _messageHistory.Dequeue();
        }

        public IEnumerable<ChatMessage> GetMessageHistory() => _messageHistory;
        public string[] GetQuickMessages() => QuickMessages;
    }

    // ─── Veri Modeli ──────────────────────────────────────────────────────────

    [Serializable]
    public class ChatMessage
    {
        public string Text;
        public ulong SenderId;
        public string SenderName;
        public DateTime Timestamp;
        public bool IsOwnMessage;
        public bool IsAnnouncement;

        public string FormattedTime => Timestamp.ToString("HH:mm");
    }

    // ─── Stub UI Arayüzleri (Inspector'dan atanacak) ─────────────────────────

    public abstract class ARChatUI : MonoBehaviour
    {
        public abstract void DisplayMessage(ChatMessage message);
        public abstract void DisplayAnnouncement(string text);
        public abstract void UpdateMicStatus(ulong clientId, bool active);
    }

    public abstract class ARTaskUI : MonoBehaviour
    {
        public abstract void ShowTask(ARCollaborativeTaskManager.ARTask task);
        public abstract void UpdateProgress(float progress, string message);
        public abstract void ShowCompletion(ARCollaborativeTaskManager.ARTask task, float time);
        public abstract void ShowTimeExpired();
    }

    public abstract class ARMultiplayerUI : MonoBehaviour
    {
        public abstract void SetStatus(string status);
        public abstract void ShowJoinCode(string code);
        public abstract void UpdatePlayerCount(int current, int max);
        public abstract void AddPlayerToList(string playerName);
        public abstract void RemovePlayerFromList(string playerName);
    }
}
