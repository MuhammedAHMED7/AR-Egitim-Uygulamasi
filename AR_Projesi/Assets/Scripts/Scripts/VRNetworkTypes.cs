// VRNetworkTypes.cs
// Çoklu kullanıcı katmanı tarafından paylaşılan veri yapıları.
// Tüm INetworkSerializable yapıları burada toplanır.

using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>Bir VR uzuvunun (kafa, sol el, sağ el) hızlı senkronize edilebilir pozu.</summary>
    [Serializable]
    public struct NetworkPose : INetworkSerializable, IEquatable<NetworkPose>
    {
        public Vector3 position;
        public Quaternion rotation;

        public NetworkPose(Vector3 p, Quaternion r) { position = p; rotation = r; }
        public static NetworkPose FromTransform(Transform t) => new NetworkPose(t.position, t.rotation);
        public static NetworkPose Identity => new NetworkPose(Vector3.zero, Quaternion.identity);

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref position);
            s.SerializeValue(ref rotation);
        }

        public bool Equals(NetworkPose other) => position == other.position && rotation == other.rotation;
    }

    /// <summary>El durumları: parmak animasyonu, tutma efektleri için.</summary>
    [Serializable]
    public struct HandState : INetworkSerializable, IEquatable<HandState>
    {
        public float grip;     // 0..1
        public float trigger;  // 0..1
        public bool isGrabbing;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref grip);
            s.SerializeValue(ref trigger);
            s.SerializeValue(ref isGrabbing);
        }

        public bool Equals(HandState o) => grip == o.grip && trigger == o.trigger && isGrabbing == o.isGrabbing;
    }

    /// <summary>Görev türleri — paylaşılan görev sistemi için.</summary>
    public enum SharedTaskKind : byte
    {
        Generic = 0,
        CollaborativeTrigger = 1, // N oyuncu aynı anda bölgede olmalı
        DeliverObject = 2,        // Belirli bir nesneyi belirli bir bölgeye getir
        SequentialAction = 3,     // Sıralı eylemler tamamlanmalı
        SimultaneousPress = 4     // İki oyuncu farklı düğmelere aynı anda basmalı
    }

    /// <summary>Görev durumları.</summary>
    public enum SharedTaskState : byte
    {
        Pending = 0,
        Active = 1,
        Completed = 2,
        Failed = 3
    }

    /// <summary>Ağ üzerinden senkronize edilen görev kaydı.</summary>
    [Serializable]
    public struct SharedTaskData : INetworkSerializable, IEquatable<SharedTaskData>
    {
        public int id;
        public SharedTaskKind kind;
        public SharedTaskState state;
        public byte requiredPlayers;     // Gerekli oyuncu sayısı (kolaboratif görevler)
        public byte currentProgress;     // 0..100 yüzde, ya da tamamlanmış adım sayısı
        public byte targetProgress;      // Hedef ilerleme (genellikle 100)
        public FixedString64Bytes title;
        public FixedString128Bytes description;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref id);
            s.SerializeValue(ref kind);
            s.SerializeValue(ref state);
            s.SerializeValue(ref requiredPlayers);
            s.SerializeValue(ref currentProgress);
            s.SerializeValue(ref targetProgress);
            s.SerializeValue(ref title);
            s.SerializeValue(ref description);
        }

        public bool Equals(SharedTaskData o) =>
            id == o.id && kind == o.kind && state == o.state &&
            requiredPlayers == o.requiredPlayers &&
            currentProgress == o.currentProgress && targetProgress == o.targetProgress &&
            title.Equals(o.title) && description.Equals(o.description);

        public float NormalizedProgress => targetProgress == 0 ? 0f : Mathf.Clamp01((float)currentProgress / targetProgress);
    }

    /// <summary>Oyuncu sunum bilgisi (ad, renk, durum).</summary>
    [Serializable]
    public struct PlayerPresence : INetworkSerializable, IEquatable<PlayerPresence>
    {
        public ulong clientId;
        public FixedString64Bytes displayName;
        public Color avatarColor;
        public bool isHost;
        public bool isSpeaking; // Ses göstergesi için ileride kullanılabilir.

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref clientId);
            s.SerializeValue(ref displayName);
            s.SerializeValue(ref avatarColor);
            s.SerializeValue(ref isHost);
            s.SerializeValue(ref isSpeaking);
        }

        public bool Equals(PlayerPresence o) =>
            clientId == o.clientId && displayName.Equals(o.displayName) &&
            avatarColor == o.avatarColor && isHost == o.isHost && isSpeaking == o.isSpeaking;
    }

    /// <summary>Hızlı sohbet için önceden tanımlanmış mesajlar.</summary>
    public enum QuickChatMessage : byte
    {
        Hello = 0,
        Yes = 1,
        No = 2,
        ComeHere = 3,
        LookAtThis = 4,
        Help = 5,
        ThankYou = 6,
        Wait = 7,
        Ready = 8,
        Done = 9
    }

    /// <summary>QuickChat mesajlarının Türkçe karşılıkları.</summary>
    public static class QuickChatStrings
    {
        public static string Get(QuickChatMessage m)
        {
            switch (m)
            {
                case QuickChatMessage.Hello: return "Merhaba!";
                case QuickChatMessage.Yes: return "Evet";
                case QuickChatMessage.No: return "Hayır";
                case QuickChatMessage.ComeHere: return "Buraya gel";
                case QuickChatMessage.LookAtThis: return "Şuna bak";
                case QuickChatMessage.Help: return "Yardım edebilir misin?";
                case QuickChatMessage.ThankYou: return "Teşekkürler";
                case QuickChatMessage.Wait: return "Bekle";
                case QuickChatMessage.Ready: return "Hazırım";
                case QuickChatMessage.Done: return "Bitirdim";
                default: return "";
            }
        }
    }

    /// <summary>Ağ oturum türleri.</summary>
    public enum NetworkSessionMode : byte
    {
        None = 0,
        LocalLAN = 1,   // Doğrudan IP üzerinden bağlantı (aynı ağ)
        Relay = 2,      // Unity Relay üzerinden internet bağlantısı
        Offline = 3     // Tek oyuncu (varsayılan)
    }
}
