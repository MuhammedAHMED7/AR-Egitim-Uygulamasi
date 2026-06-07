using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>
    /// VRSceneBootstrapper sahneyi kurduktan sonra UI bootstrap'i tetikler.
    /// Bu script'i sahnedeki "VRBootstrap" GameObject'una eklemeniz yeterli;
    /// VRUIBootstrapper otomatik olarak oluşturulur ve çalışır.
    /// </summary>
    [DefaultExecutionOrder(-30)]
    public class VRUIIntegration : MonoBehaviour
    {
        [Tooltip("Otomatik olarak VRUIBootstrapper oluştur ve UI'ı kur")]
        public bool autoBuildOnStart = true;

        [Tooltip("Sahne yüklendikten kaç saniye sonra UI inşa edilsin?")]
        [Range(0f, 2f)] public float delaySeconds = 0.1f;

        private void Start()
        {
            if (!autoBuildOnStart) return;
            Invoke(nameof(BuildUI), delaySeconds);
        }

        public void BuildUI()
        {
            var existing = FindObjectOfType<VRUIBootstrapper>();
            if (existing != null)
            {
                existing.BuildUI();
                return;
            }

            var go = new GameObject("VR_UI_Bootstrapper");
            var b = go.AddComponent<VRUIBootstrapper>();
            b.autoBuildOnStart = false;
            b.BuildUI();
        }
    }
}
