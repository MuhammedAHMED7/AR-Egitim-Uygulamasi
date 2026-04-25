using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AREducation.Multiplayer
{
    /// <summary>
    /// İşbirliği Tabanlı Görev Sistemi
    /// Öğrencilerin birlikte tamamlaması gereken AR görevlerini yönetir.
    /// Örnek: Hücre organellerini doğru konuma yerleştirme, atom modeli kurma vs.
    /// </summary>
    public class ARCollaborativeTaskManager : NetworkBehaviour
    {
        public static ARCollaborativeTaskManager Instance { get; private set; }

        [Header("Görev Ayarları")]
        [SerializeField] private List<ARTask> taskDefinitions = new();
        [SerializeField] private ARTaskUI taskUI;

        // Ağ değişkenleri
        private NetworkVariable<int> _currentTaskIndex =
            new NetworkVariable<int>(0,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<int> _completedSteps =
            new NetworkVariable<int>(0,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<bool> _taskActive =
            new NetworkVariable<bool>(false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        private NetworkVariable<float> _taskTimer =
            new NetworkVariable<float>(0f,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        // Events
        public event Action<ARTask> OnTaskStarted;
        public event Action<ARTask, int> OnStepCompleted;
        public event Action<ARTask> OnTaskCompleted;
        public event Action<float> OnTimerUpdated;

        // Lokal takip
        private HashSet<ulong> _playersReadyForNextTask = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            _currentTaskIndex.OnValueChanged += OnTaskChanged;
            _completedSteps.OnValueChanged += OnStepsChanged;
            _taskActive.OnValueChanged += OnTaskActiveChanged;
            _taskTimer.OnValueChanged += (o, n) => OnTimerUpdated?.Invoke(n);
        }

        private void Update()
        {
            if (!IsServer || !_taskActive.Value) return;

            // Zamanlayıcıyı güncelle
            ARTask current = GetCurrentTask();
            if (current != null && current.timeLimit > 0)
            {
                _taskTimer.Value += Time.deltaTime;
                if (_taskTimer.Value >= current.timeLimit)
                {
                    OnTaskTimeExpiredClientRpc();
                }
            }
        }

        // ─── Görev Başlatma ────────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void StartTaskServerRpc(int taskIndex)
        {
            if (taskIndex < 0 || taskIndex >= taskDefinitions.Count) return;

            _currentTaskIndex.Value = taskIndex;
            _completedSteps.Value = 0;
            _taskTimer.Value = 0f;
            _taskActive.Value = true;
            _playersReadyForNextTask.Clear();

            NotifyTaskStartedClientRpc(taskIndex);
        }

        [ClientRpc]
        private void NotifyTaskStartedClientRpc(int taskIndex)
        {
            ARTask task = taskDefinitions[taskIndex];
            OnTaskStarted?.Invoke(task);
            taskUI?.ShowTask(task);
            Debug.Log($"[TaskManager] Görev başladı: {task.taskName}");
        }

        // ─── Adım Tamamlama ────────────────────────────────────────────────────

        /// <summary>
        /// Bir oyuncu görev adımını tamamladığında çağrılır
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CompleteStepServerRpc(int stepIndex, ulong completedByClientId)
        {
            ARTask current = GetCurrentTask();
            if (current == null || !_taskActive.Value) return;
            if (stepIndex != _completedSteps.Value) return; // Sıralı tamamlama

            _completedSteps.Value++;

            NotifyStepCompletedClientRpc(stepIndex, completedByClientId,
                (float)_completedSteps.Value / current.steps.Count);

            // Tüm adımlar tamam mı?
            if (_completedSteps.Value >= current.steps.Count)
                CompleteCurrentTask();
        }

        [ClientRpc]
        private void NotifyStepCompletedClientRpc(int stepIndex, ulong byClientId, float progress)
        {
            ARTask task = GetCurrentTask();
            if (task == null) return;

            ARTaskStep step = task.steps[stepIndex];
            OnStepCompleted?.Invoke(task, stepIndex);
            taskUI?.UpdateProgress(progress, $"Öğrenci {byClientId + 1} '{step.stepName}' tamamladı!");
        }

        // ─── Görev Tamamlama ───────────────────────────────────────────────────

        private void CompleteCurrentTask()
        {
            _taskActive.Value = false;
            ARTask task = GetCurrentTask();
            float completionTime = _taskTimer.Value;

            NotifyTaskCompletedClientRpc(_currentTaskIndex.Value, completionTime);
        }

        [ClientRpc]
        private void NotifyTaskCompletedClientRpc(int taskIndex, float completionTime)
        {
            ARTask task = taskDefinitions[taskIndex];
            OnTaskCompleted?.Invoke(task);
            taskUI?.ShowCompletion(task, completionTime);
            Debug.Log($"[TaskManager] Görev tamamlandı: {task.taskName} ({completionTime:F1}s)");
        }

        [ClientRpc]
        private void OnTaskTimeExpiredClientRpc()
        {
            taskUI?.ShowTimeExpired();
            Debug.Log("[TaskManager] Süre doldu!");
        }

        // ─── Sonraki Göreve Geçiş ──────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void VoteForNextTaskServerRpc(ulong clientId)
        {
            _playersReadyForNextTask.Add(clientId);

            int connectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;
            float readyRatio = (float)_playersReadyForNextTask.Count / connectedPlayers;

            // Oyuncuların yarısından fazlası hazırsa sonraki göreve geç
            if (readyRatio >= 0.5f && _currentTaskIndex.Value + 1 < taskDefinitions.Count)
            {
                StartTaskServerRpc(_currentTaskIndex.Value + 1);
            }
        }

        // ─── Yardımcı Metodlar ─────────────────────────────────────────────────

        private ARTask GetCurrentTask()
        {
            if (_currentTaskIndex.Value < 0 || _currentTaskIndex.Value >= taskDefinitions.Count)
                return null;
            return taskDefinitions[_currentTaskIndex.Value];
        }

        private void OnTaskChanged(int oldVal, int newVal) { }
        private void OnStepsChanged(int oldVal, int newVal) { }
        private void OnTaskActiveChanged(bool oldVal, bool newVal) { }

        public ARTask CurrentTask => GetCurrentTask();
        public int CompletedSteps => _completedSteps.Value;
        public bool IsTaskActive => _taskActive.Value;
        public float TaskTimer => _taskTimer.Value;

        public override void OnNetworkDespawn()
        {
            _currentTaskIndex.OnValueChanged -= OnTaskChanged;
            _completedSteps.OnValueChanged -= OnStepsChanged;
            _taskActive.OnValueChanged -= OnTaskActiveChanged;
        }
    }

    // ─── Veri Modelleri ────────────────────────────────────────────────────────

    [Serializable]
    public class ARTask
    {
        public string taskId;
        public string taskName;
        [TextArea(2, 4)] public string description;
        public float timeLimit; // 0 = süre sınırı yok
        public int minPlayersRequired = 1;
        public List<ARTaskStep> steps = new();
        public GameObject taskScenePrefab; // AR sahnesi prefab'ı
    }

    [Serializable]
    public class ARTaskStep
    {
        public string stepName;
        [TextArea(1, 3)] public string instruction;
        public Vector3 targetPosition;
        public float completionRadius = 0.1f;
        public bool requiresMultiplePlayers;
    }
}
