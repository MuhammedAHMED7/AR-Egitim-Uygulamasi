// VRSharedTaskManager.cs
// Tüm oyuncular arasında senkronize edilen görev sistemi.
// Host bir NetworkObject olarak spawn eder; herkes okur, herkes ilerleme bildirir.
// İlerleme kuralı: sadece sunucu görev durumunu değiştirebilir, istemciler ServerRpc ile talep eder.

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace AREgitim.VR
{
    public class VRSharedTaskManager : NetworkBehaviour
    {
        public static VRSharedTaskManager Instance { get; private set; }

        // Görev listesi — herkes okur, sadece sunucu yazar.
        public NetworkList<SharedTaskData> Tasks;

        // Yerel istemci tarafında dinlemek için olaylar
        public static event Action<SharedTaskData> OnTaskCompleted;
        public static event Action<SharedTaskData> OnTaskProgressChanged;
        public static event Action OnTasksChanged;

        void Awake()
        {
            Tasks = new NetworkList<SharedTaskData>();
        }

        public override void OnNetworkSpawn()
        {
            if (Instance == null) Instance = this;
            Tasks.OnListChanged += HandleListChanged;

            if (IsServer)
            {
                // Host: varsayılan demo görev setini ekle
                if (Tasks.Count == 0)
                {
                    SeedDefaultTasks();
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Tasks != null) Tasks.OnListChanged -= HandleListChanged;
            if (Instance == this) Instance = null;
        }

        void HandleListChanged(NetworkListEvent<SharedTaskData> e)
        {
            OnTasksChanged?.Invoke();
            if (e.Type == NetworkListEvent<SharedTaskData>.EventType.Value)
            {
                var prev = e.PreviousValue;
                var cur = e.Value;
                if (cur.state == SharedTaskState.Completed && prev.state != SharedTaskState.Completed)
                {
                    OnTaskCompleted?.Invoke(cur);
                    // Kullanıcıya bildirim
                    if (VRUIManager.Instance != null)
                        VRUIManager.Instance.ShowNotification($"✓ Görev tamamlandı: {cur.title}",
                            VRNotificationController.NotificationType.Success);
                }
                else if (cur.currentProgress != prev.currentProgress)
                {
                    OnTaskProgressChanged?.Invoke(cur);
                }
            }
        }

        // ---- Sunucu API'si (host'ta çağrılır) ----

        public void AddTask(SharedTaskData task)
        {
            if (!IsServer) return;
            Tasks.Add(task);
        }

        public void RemoveTaskById(int id)
        {
            if (!IsServer) return;
            for (int i = 0; i < Tasks.Count; i++)
            {
                if (Tasks[i].id == id) { Tasks.RemoveAt(i); return; }
            }
        }

        public void ClearTasks()
        {
            if (!IsServer) return;
            Tasks.Clear();
        }

        // ---- İstemci API'si (herkesten çağrılabilir) ----

        /// <summary>Bir görevin ilerlemesini artır. Sunucuda işlenir.</summary>
        public void ReportProgress(int taskId, int delta)
        {
            ReportProgressServerRpc(taskId, delta);
        }

        /// <summary>Bir görevi doğrudan tamamlanmış olarak işaretle.</summary>
        public void CompleteTask(int taskId)
        {
            CompleteTaskServerRpc(taskId);
        }

        /// <summary>Belirli sayıda oyuncunun aynı anda bir bölgede olduğunu bildir.</summary>
        public void ReportCollaborativeOccupancy(int taskId, int occupantCount)
        {
            ReportOccupancyServerRpc(taskId, occupantCount);
        }

        [ServerRpc(RequireOwnership = false)]
        void ReportProgressServerRpc(int taskId, int delta)
        {
            if (!IsServer) return;
            for (int i = 0; i < Tasks.Count; i++)
            {
                var t = Tasks[i];
                if (t.id != taskId) continue;
                if (t.state == SharedTaskState.Completed) return;

                int newProgress = Mathf.Clamp(t.currentProgress + delta, 0, t.targetProgress);
                t.currentProgress = (byte)newProgress;
                if (t.state == SharedTaskState.Pending) t.state = SharedTaskState.Active;
                if (t.currentProgress >= t.targetProgress) t.state = SharedTaskState.Completed;
                Tasks[i] = t;
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void CompleteTaskServerRpc(int taskId)
        {
            if (!IsServer) return;
            for (int i = 0; i < Tasks.Count; i++)
            {
                var t = Tasks[i];
                if (t.id != taskId) continue;
                t.currentProgress = t.targetProgress;
                t.state = SharedTaskState.Completed;
                Tasks[i] = t;
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void ReportOccupancyServerRpc(int taskId, int occupantCount)
        {
            if (!IsServer) return;
            for (int i = 0; i < Tasks.Count; i++)
            {
                var t = Tasks[i];
                if (t.id != taskId) continue;
                if (t.state == SharedTaskState.Completed) return;
                // İstenen oyuncu sayısı sağlanırsa tamamlanır
                if (occupantCount >= t.requiredPlayers)
                {
                    t.state = SharedTaskState.Completed;
                    t.currentProgress = t.targetProgress;
                }
                else
                {
                    t.state = SharedTaskState.Active;
                    // İlerleme = oyuncu sayısı oranı * hedef
                    float ratio = (float)occupantCount / Mathf.Max(1, t.requiredPlayers);
                    t.currentProgress = (byte)Mathf.Clamp(Mathf.RoundToInt(ratio * t.targetProgress), 0, t.targetProgress);
                }
                Tasks[i] = t;
                return;
            }
        }

        // ---- Varsayılan görev seti ----

        void SeedDefaultTasks()
        {
            AddTask(new SharedTaskData
            {
                id = 1,
                kind = SharedTaskKind.CollaborativeTrigger,
                state = SharedTaskState.Pending,
                requiredPlayers = 2,
                currentProgress = 0,
                targetProgress = 100,
                title = new FixedString64Bytes("İki kişi bölgede"),
                description = new FixedString128Bytes("Yeşil halkanın üzerinde aynı anda 2 oyuncu durun.")
            });
            AddTask(new SharedTaskData
            {
                id = 2,
                kind = SharedTaskKind.SimultaneousPress,
                state = SharedTaskState.Pending,
                requiredPlayers = 2,
                currentProgress = 0,
                targetProgress = 2,
                title = new FixedString64Bytes("Eş zamanlı düğme"),
                description = new FixedString128Bytes("İki oyuncu farklı düğmelere aynı anda dokunmalı.")
            });
            AddTask(new SharedTaskData
            {
                id = 3,
                kind = SharedTaskKind.DeliverObject,
                state = SharedTaskState.Pending,
                requiredPlayers = 1,
                currentProgress = 0,
                targetProgress = 4,
                title = new FixedString64Bytes("Küpleri yerleştir"),
                description = new FixedString128Bytes("4 küpü hedef platforma taşı (birlikte yapılabilir).")
            });
        }

        // ---- Yardımcı sorgular ----

        public bool TryGetTask(int id, out SharedTaskData task)
        {
            for (int i = 0; i < Tasks.Count; i++)
            {
                if (Tasks[i].id == id) { task = Tasks[i]; return true; }
            }
            task = default;
            return false;
        }

        public List<SharedTaskData> Snapshot()
        {
            var list = new List<SharedTaskData>(Tasks.Count);
            for (int i = 0; i < Tasks.Count; i++) list.Add(Tasks[i]);
            return list;
        }
    }
}
