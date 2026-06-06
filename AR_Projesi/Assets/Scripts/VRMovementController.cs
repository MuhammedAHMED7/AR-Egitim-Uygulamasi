using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AREgitim.VR
{
    /// <summary>
    /// VR hareket sağlayıcılarını (continuous, snap turn, smooth turn) ayarlara göre
    /// açıp kapatan yardımcı. VRInteractionManager.OnMovementModeChanged'i dinler.
    ///
    /// Sahnede ContinuousMoveProviderBase, SnapTurnProviderBase ve ContinuousTurnProviderBase
    /// bileşenlerinin XR Origin'e ekli olması gerekir (Bootstrapper bunu yapar).
    /// </summary>
    public class VRMovementController : MonoBehaviour
    {
        public ContinuousMoveProviderBase continuousMove;
        public SnapTurnProviderBase snapTurn;
        public ContinuousTurnProviderBase smoothTurn;
        public TeleportationProvider teleport;

        void Start()
        {
            VRInteractionManager.OnMovementModeChanged += HandleModeChanged;
            ApplyCurrentSettings();
        }

        void OnDestroy()
        {
            VRInteractionManager.OnMovementModeChanged -= HandleModeChanged;
        }

        void HandleModeChanged(VRInteractionManager.MovementMode mode)
        {
            ApplyCurrentSettings();
        }

        public void ApplyCurrentSettings()
        {
            var m = VRInteractionManager.Instance;
            if (m == null) return;

            bool continuousOn = m.movementMode == VRInteractionManager.MovementMode.Continuous
                              || m.movementMode == VRInteractionManager.MovementMode.Both;
            bool teleportOn   = m.movementMode == VRInteractionManager.MovementMode.Teleport
                              || m.movementMode == VRInteractionManager.MovementMode.Both;

            if (continuousMove != null)
            {
                continuousMove.enabled = continuousOn;
                continuousMove.moveSpeed = m.moveSpeed;
            }

            if (teleport != null)
            {
                teleport.enabled = teleportOn;
            }

            // Dönüş: kullanıcı tercihine göre snap veya smooth
            if (snapTurn != null)
            {
                snapTurn.enabled = !m.useSmoothTurn;
                snapTurn.turnAmount = m.snapTurnAngle;
            }
            if (smoothTurn != null)
            {
                smoothTurn.enabled = m.useSmoothTurn;
                smoothTurn.turnSpeed = m.smoothTurnSpeed;
            }
        }
    }
}
