using System;
using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>
    /// VR oturumunun merkezi koordinatörü. Singleton.
    /// Hareket modu (teleport / sürekli), tutma olayları ve ayarları yönetir.
    /// </summary>
    public class VRInteractionManager : MonoBehaviour
    {
        public static VRInteractionManager Instance { get; private set; }

        public enum MovementMode { Teleport, Continuous, Both }

        [Header("Hareket Ayarları")]
        [Tooltip("Varsayılan hareket modu.")]
        public MovementMode movementMode = MovementMode.Both;

        [Tooltip("Sürekli hareket hızı (metre/saniye).")]
        [Range(0.5f, 5f)] public float moveSpeed = 1.8f;

        [Tooltip("Snap turn açısı (derece). Motion sickness'ı azaltır.")]
        [Range(15f, 90f)] public float snapTurnAngle = 30f;

        [Tooltip("Snap turn yerine yumuşak dönüş kullanılsın mı?")]
        public bool useSmoothTurn = false;

        [Tooltip("Yumuşak dönüş hızı (derece/saniye).")]
        [Range(30f, 180f)] public float smoothTurnSpeed = 80f;

        // ───────── Olaylar ─────────
        public static event Action<VRGrabbable> OnObjectGrabbed;
        public static event Action<VRGrabbable> OnObjectReleased;
        public static event Action<VRGrabbable> OnObjectUsed;
        public static event Action<MovementMode> OnMovementModeChanged;

        // ───────── PlayerPrefs anahtarları ─────────
        const string K_MODE   = "vr_movement_mode";
        const string K_SPEED  = "vr_move_speed";
        const string K_SNAP   = "vr_snap_angle";
        const string K_SMOOTH = "vr_smooth_turn";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
        }

        void Load()
        {
            if (PlayerPrefs.HasKey(K_MODE))
                movementMode = (MovementMode)PlayerPrefs.GetInt(K_MODE);
            if (PlayerPrefs.HasKey(K_SPEED))
                moveSpeed = PlayerPrefs.GetFloat(K_SPEED);
            if (PlayerPrefs.HasKey(K_SNAP))
                snapTurnAngle = PlayerPrefs.GetFloat(K_SNAP);
            if (PlayerPrefs.HasKey(K_SMOOTH))
                useSmoothTurn = PlayerPrefs.GetInt(K_SMOOTH) == 1;
        }

        public void Save()
        {
            PlayerPrefs.SetInt(K_MODE, (int)movementMode);
            PlayerPrefs.SetFloat(K_SPEED, moveSpeed);
            PlayerPrefs.SetFloat(K_SNAP, snapTurnAngle);
            PlayerPrefs.SetInt(K_SMOOTH, useSmoothTurn ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetMovementMode(MovementMode mode)
        {
            movementMode = mode;
            Save();
            OnMovementModeChanged?.Invoke(mode);
        }

        // ───────── Statik bildirim metotları (VRGrabbable'dan çağrılır) ─────────
        public static void NotifyGrabbed(VRGrabbable obj)
        {
            OnObjectGrabbed?.Invoke(obj);
            Debug.Log($"[VR] Tutuldu: {obj.displayName}");
        }

        public static void NotifyReleased(VRGrabbable obj)
        {
            OnObjectReleased?.Invoke(obj);
            Debug.Log($"[VR] Bırakıldı: {obj.displayName}");
        }

        public static void NotifyUsed(VRGrabbable obj)
        {
            OnObjectUsed?.Invoke(obj);
            Debug.Log($"[VR] Kullanıldı: {obj.displayName}");
        }
    }
}
