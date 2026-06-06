// VRTaskCollaborationZone.cs
// Bir görev için işbirliği bölgesi: kaç oyuncunun aynı anda bölgede olduğunu sayar
// ve VRSharedTaskManager'a bildirir. Aynı anda N oyuncu girince görev tamamlanır.
//
// Kullanım: Sahnedeki bir GameObject'e (NetworkObject + Collider isTrigger=true) bu betiği ekle.
// taskId ile VRSharedTaskManager'daki görev kaydını eşleştir.

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AREgitim.VR
{
    [RequireComponent(typeof(NetworkObject))]
    public class VRTaskCollaborationZone : NetworkBehaviour
    {
        [Header("Görev")]
        [Tooltip("VRSharedTaskManager'daki görev id'si")]
        public int taskId = 1;

        [Header("Bölge")]
        public Color zoneColor = new Color(0.30f, 0.85f, 0.45f, 0.40f);
        public Color completedColor = new Color(0.30f, 0.78f, 1f, 0.40f);
        public float visualHeight = 0.05f;
        public bool autoVisuals = true;

        // İçerideki oyuncuların NetworkObjectId set'i (yalnızca sunucuda doğru)
        readonly HashSet<ulong> _occupantsInside = new HashSet<ulong>();
        Renderer _zoneRenderer;
        Material _zoneMat;
        bool _completed;

        void Awake()
        {
            var col = GetComponent<Collider>();
            if (col == null)
            {
                var c = gameObject.AddComponent<SphereCollider>();
                c.isTrigger = true;
                c.radius = 0.8f;
            }
            else col.isTrigger = true;

            if (autoVisuals) BuildVisuals();
        }

        void BuildVisuals()
        {
            // Görsel zemin halkası
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "ZoneVisual";
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0, visualHeight * 0.5f, 0);
            visual.transform.localScale = new Vector3(1.8f, visualHeight, 1.8f);
            var col = visual.GetComponent<Collider>();
            if (col != null) Destroy(col);
            _zoneRenderer = visual.GetComponent<Renderer>();
            _zoneMat = new Material(Shader.Find("Standard"));
            // Şeffaf yapmak için Standard shader render mode = Fade
            _zoneMat.SetFloat("_Mode", 2);
            _zoneMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _zoneMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _zoneMat.SetInt("_ZWrite", 0);
            _zoneMat.DisableKeyword("_ALPHATEST_ON");
            _zoneMat.EnableKeyword("_ALPHABLEND_ON");
            _zoneMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _zoneMat.renderQueue = 3000;
            _zoneMat.color = zoneColor;
            _zoneRenderer.material = _zoneMat;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return; // sayım sunucuda
            ulong playerNetId = ResolvePlayerNetId(other);
            if (playerNetId == ulong.MaxValue) return;
            _occupantsInside.Add(playerNetId);
            PushOccupancyToTaskManager();
        }

        void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;
            ulong playerNetId = ResolvePlayerNetId(other);
            if (playerNetId == ulong.MaxValue) return;
            _occupantsInside.Remove(playerNetId);
            PushOccupancyToTaskManager();
        }

        ulong ResolvePlayerNetId(Collider other)
        {
            // Yerel oyuncunun XR rig'i NetworkObject taşımaz; bu yüzden ContextResolver
            // her istemcide kendi player NetworkObject'in trigger'a girdiğini sunucuya bildirir.
            // Sunucu tarafında: NetworkObject taşıyan collider'ları doğrudan algılayabiliriz.
            var netObj = other.GetComponentInParent<NetworkObject>();
            if (netObj != null) return netObj.NetworkObjectId;
            return ulong.MaxValue;
        }

        void PushOccupancyToTaskManager()
        {
            if (VRSharedTaskManager.Instance == null) return;
            VRSharedTaskManager.Instance.ReportCollaborativeOccupancy(taskId, _occupantsInside.Count);
        }

        // Yerel oyuncu trigger bildirimi (oyuncunun XR rig'i ayrı olduğu için yerel raycaster da çağırabilir)
        public void ClientLocalEntered()
        {
            ReportEnterServerRpc();
        }

        public void ClientLocalExited()
        {
            ReportExitServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void ReportEnterServerRpc(ServerRpcParams rpc = default)
        {
            _occupantsInside.Add(rpc.Receive.SenderClientId);
            PushOccupancyToTaskManager();
        }

        [ServerRpc(RequireOwnership = false)]
        void ReportExitServerRpc(ServerRpcParams rpc = default)
        {
            _occupantsInside.Remove(rpc.Receive.SenderClientId);
            PushOccupancyToTaskManager();
        }

        void Update()
        {
            // Görsel: görev tamamlanmışsa rengi mavi yap
            if (_zoneMat == null) return;
            if (VRSharedTaskManager.Instance == null) return;
            if (VRSharedTaskManager.Instance.TryGetTask(taskId, out var task))
            {
                bool completed = task.state == SharedTaskState.Completed;
                if (completed != _completed)
                {
                    _completed = completed;
                    _zoneMat.color = completed ? completedColor : zoneColor;
                }
                // Pulsasyon efekti
                if (!completed && task.requiredPlayers > 0)
                {
                    float ratio = (float)task.currentProgress / task.targetProgress;
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.5f);
                    var c = zoneColor;
                    c.a = Mathf.Lerp(0.25f, 0.55f, Mathf.Max(ratio, pulse * 0.5f));
                    _zoneMat.color = c;
                }
            }
        }
    }
}
