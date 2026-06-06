using UnityEngine;
using System.Reflection;

namespace AREgitim.VR
{
    /// <summary>
    /// VRInteractionManager'a UI tarafının ihtiyaç duyduğu ayar alanlarını ekler.
    /// Mevcut dosyayı kırmamak için partial olarak eklenmiştir.
    ///
    /// KURULUM:
    /// 1. VRInteractionManager.cs'i açın
    /// 2. "public class VRInteractionManager" yazısını
    ///    "public partial class VRInteractionManager" yapın
    /// 3. Bu dosyayı Scripts klasörüne kopyalayın
    ///
    /// VRMovementController'a setter metot eklemek istemiyorsanız bu dosya
    /// reflection ile alan/property uyumluluğunu kendi başına çözer:
    /// hangi isimler mevcutsa onlara yazar, eksik olanı atlar (hata vermez).
    /// </summary>
    public partial class VRInteractionManager
    {
        [Header("--- UI Görev 2: Genişletilmiş Ayarlar ---")]

        [Tooltip("Sürekli hareket aktif mi?")]
        public bool continuousMoveEnabled = true;

        [Tooltip("Teleport aktif mi?")]
        public bool teleportEnabled = true;

        [Tooltip("Sürekli hareket hızı (m/s)")]
        [Range(0.5f, 5f)] public float moveSpeed = 2.5f;

        [Tooltip("Dönüş modu")]
        public VRTurnMode turnMode = VRTurnMode.Snap;

        [Tooltip("Snap turn açısı (derece)")]
        [Range(15f, 90f)] public float snapTurnAngle = 45f;

        [Tooltip("Smooth turn hızı (derece/saniye)")]
        [Range(30f, 180f)] public float smoothTurnSpeed = 60f;

        [Tooltip("Hareket sırasında görüş alanı daraltma efekti")]
        public bool comfortVignetteEnabled = true;

        [Tooltip("UI ölçek çarpanı")]
        [Range(0.5f, 1.5f)] public float uiScale = 1f;

        public event System.Action OnSettingsApplied;

        /// <summary>Tüm ayarları varsayılan değerlere döndürür.</summary>
        public void ResetToDefaults()
        {
            continuousMoveEnabled  = true;
            teleportEnabled        = true;
            moveSpeed              = 2.5f;
            turnMode               = VRTurnMode.Snap;
            snapTurnAngle          = 45f;
            smoothTurnSpeed        = 60f;
            comfortVignetteEnabled = true;
            uiScale                = 1f;
        }

        /// <summary>
        /// Ayarları VRMovementController'a yansıtır. Reflection ile uyumluluk:
        /// hedef sınıfta hangi isimde alan/property/metot varsa otomatik bulur.
        /// Hiçbiri yoksa sessizce atlar; derleme kırılmaz.
        /// </summary>
        public void ApplySettings()
        {
            var movement = Object.FindObjectOfType<VRMovementController>();
            if (movement != null)
            {
                TrySet(movement, "continuousMoveEnabled", continuousMoveEnabled);
                TrySet(movement, "ContinuousMoveEnabled", continuousMoveEnabled);
                TrySet(movement, "teleportEnabled",       teleportEnabled);
                TrySet(movement, "TeleportEnabled",       teleportEnabled);
                TrySet(movement, "moveSpeed",             moveSpeed);
                TrySet(movement, "MoveSpeed",             moveSpeed);
                TrySet(movement, "turnMode",              turnMode);
                TrySet(movement, "TurnMode",              turnMode);
                TrySet(movement, "snapTurnAngle",         snapTurnAngle);
                TrySet(movement, "SnapTurnAngle",         snapTurnAngle);
                TrySet(movement, "smoothTurnSpeed",       smoothTurnSpeed);
                TrySet(movement, "SmoothTurnSpeed",       smoothTurnSpeed);
                TrySet(movement, "comfortVignetteEnabled", comfortVignetteEnabled);
                TrySet(movement, "ComfortVignetteEnabled", comfortVignetteEnabled);

                // Apply/Refresh/UpdateSettings gibi bir metot varsa çağır
                CallIfExists(movement, "ApplySettings");
                CallIfExists(movement, "RefreshSettings");
                CallIfExists(movement, "UpdateSettings");
            }

            OnSettingsApplied?.Invoke();
        }

        private static void TrySet(object target, string memberName, object value)
        {
            if (target == null) return;
            var type = target.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var field = type.GetField(memberName, flags);
            if (field != null && field.FieldType.IsInstanceOfType(value))
            {
                field.SetValue(target, value);
                return;
            }
            // Enum dönüşümü
            if (field != null && field.FieldType.IsEnum && value != null)
            {
                try { field.SetValue(target, System.Convert.ChangeType(value, System.Enum.GetUnderlyingType(field.FieldType))); }
                catch { /* sessizce atla */ }
                return;
            }

            var prop = type.GetProperty(memberName, flags);
            if (prop != null && prop.CanWrite && prop.PropertyType.IsInstanceOfType(value))
            {
                prop.SetValue(target, value);
                return;
            }
        }

        private static void CallIfExists(object target, string methodName)
        {
            if (target == null) return;
            var m = target.GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, System.Type.EmptyTypes, null);
            if (m != null) m.Invoke(target, null);
        }
    }
}
