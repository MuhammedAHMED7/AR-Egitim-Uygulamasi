using System;
using System.Collections.Generic;
using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>
    /// VR arayüzünün merkezi koordinatörü. Sahnede sadece bir tane bulunur ve
    /// VRSceneBootstrapper tarafından (veya VRUIBootstrapper tarafından) kurulur.
    /// Tüm panel açma/kapama, tooltip, toast ve odak yönetimi buradan geçer.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class VRUIManager : MonoBehaviour
    {
        public static VRUIManager Instance { get; private set; }

        [Header("Referanslar (Bootstrapper tarafından doldurulur)")]
        public Transform headTransform;          // XR Origin -> Camera
        public Transform leftControllerTransform;
        public Transform rightControllerTransform;

        [Header("UI Yüzeyleri")]
        public VRWristMenu wristMenu;            // Sol kumandadaki menü
        public VRMainMenuPanel mainMenuPanel;    // Ana menü (orta floating)
        public VRSettingsPanel settingsPanel;    // Ayarlar paneli
        public VRInfoPanel infoPanel;            // Bilgi/yardım paneli
        public VRToastController toast;          // Sağ üst geçici mesajlar
        public VRTooltipController tooltip;      // Hover tooltip'leri

        [Header("Davranış Ayarları")]
        [Tooltip("Floating panellerin kullanıcıya olan mesafesi (metre)")]
        public float panelDistance = 1.5f;

        [Tooltip("Paneller kullanıcının başını her zaman takip etsin mi?")]
        public bool panelsFollowHead = false;

        [Tooltip("Panel boyunca yumuşatma katsayısı (büyüdükçe daha yavaş)")]
        [Range(0.5f, 10f)] public float followSmoothing = 4f;

        private readonly List<VRFloatingPanel> _activePanels = new List<VRFloatingPanel>();

        public event Action<VRFloatingPanel> OnPanelOpened;
        public event Action<VRFloatingPanel> OnPanelClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            if (!panelsFollowHead || headTransform == null) return;

            foreach (var panel in _activePanels)
            {
                if (panel == null || !panel.isActiveAndEnabled) continue;
                if (!panel.followHead) continue;

                Vector3 forward = headTransform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.001f) continue;
                forward.Normalize();

                Vector3 targetPos = headTransform.position + forward * panelDistance;
                targetPos.y = headTransform.position.y + panel.verticalOffset;

                panel.transform.position = Vector3.Lerp(panel.transform.position, targetPos,
                    Time.deltaTime * followSmoothing);

                Vector3 lookDir = panel.transform.position - headTransform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    panel.transform.rotation = Quaternion.Slerp(panel.transform.rotation,
                        targetRot, Time.deltaTime * followSmoothing);
                }
            }
        }

        /// <summary>Paneli kullanıcının önünde uygun mesafede konumlandırır ve açar.</summary>
        public void OpenPanel(VRFloatingPanel panel, bool inFrontOfUser = true)
        {
            if (panel == null) return;

            if (inFrontOfUser && headTransform != null)
            {
                PositionInFrontOfUser(panel.transform, panelDistance, panel.verticalOffset);
            }

            panel.gameObject.SetActive(true);
            panel.OnPanelOpened();

            if (!_activePanels.Contains(panel))
                _activePanels.Add(panel);

            OnPanelOpened?.Invoke(panel);
        }

        public void ClosePanel(VRFloatingPanel panel)
        {
            if (panel == null) return;
            panel.OnPanelClosed();
            panel.gameObject.SetActive(false);
            _activePanels.Remove(panel);
            OnPanelClosed?.Invoke(panel);
        }

        public void TogglePanel(VRFloatingPanel panel)
        {
            if (panel == null) return;
            if (panel.gameObject.activeSelf) ClosePanel(panel);
            else OpenPanel(panel);
        }

        public void CloseAllPanels()
        {
            // Listeyi kopyalayıp döndür, çünkü ClosePanel listeyi değiştirir.
            var copy = _activePanels.ToArray();
            foreach (var p in copy) ClosePanel(p);
        }

        /// <summary>Bir transform'u kullanıcının önünde, ona bakacak şekilde konumlar.</summary>
        public void PositionInFrontOfUser(Transform t, float distance, float verticalOffset)
        {
            if (headTransform == null || t == null) return;

            Vector3 forward = headTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            t.position = headTransform.position + forward * distance + Vector3.up * verticalOffset;

            Vector3 lookDir = t.position - headTransform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
                t.rotation = Quaternion.LookRotation(lookDir);
        }

        // ---- Kısa yollar (HUD'dan ve diğer scriptlerden çağrılır) ----

        public void ShowToast(string message, float duration = 2.5f, ToastType type = ToastType.Info)
        {
            if (toast != null) toast.Show(message, duration, type);
        }

        public void ShowTooltip(string text, Vector3 worldPos)
        {
            if (tooltip != null) tooltip.Show(text, worldPos);
        }

        public void HideTooltip()
        {
            if (tooltip != null) tooltip.Hide();
        }
    }

    public enum ToastType { Info, Success, Warning, Error }
}
