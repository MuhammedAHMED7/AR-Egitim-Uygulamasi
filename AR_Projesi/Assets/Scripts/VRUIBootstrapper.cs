using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

namespace AREgitim.VR
{
    /// <summary>
    /// VR UI'ını sahnede programatik olarak inşa eder. VRSceneBootstrapper
    /// XR Origin'i ve kumandaları kurduktan sonra Awake'de devreye girer
    /// (DefaultExecutionOrder -900: bootstrap'tan sonra, oyun mantığından önce).
    ///
    /// Şunları oluşturur:
    ///  - VRUIManager (singleton)
    ///  - Ana menü, Ayarlar, Yardım panelleri (dünya uzayı, gizli başlar)
    ///  - Bildirim alanı (toast)
    ///  - Sol bilek menüsü
    ///  - Sağ radial menü
    ///  - Her kumandanın yanında tooltip
    ///  - Ray Interactor'ların görsel feedback'i
    ///  - Mevcut VRGrabbable'lara VRInfoPanel ekler
    ///  - Menu/A düğmesi input action'larını bağlar
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class VRUIBootstrapper : MonoBehaviour
    {
        [Header("Yapılandırma")]
        [Tooltip("Mevcut tutulabilir nesnelere otomatik olarak bilgi paneli ekle.")]
        public bool addInfoPanelsToGrabbables = true;

        [Tooltip("Sol bilek menüsü oluşturulsun mu?")]
        public bool createWristMenu = true;

        [Tooltip("Sağ kumanda için radial menü oluşturulsun mu?")]
        public bool createRadialMenu = true;

        [Tooltip("Kumandaların yanında bağlam ipuçları gösterilsin mi?")]
        public bool createTooltips = true;

        VRUIManager _ui;
        VRRadialMenu _radial;

        // Input actions — UI tetikleri için
        InputAction _leftMenuAction;        // Sol Menu (≡) düğmesi → ana menü toggle
        InputAction _rightPrimaryAction;    // Sağ A → radial menü
        InputAction _rightStickAction;      // Sağ joystick → radial menü yön

        bool _radialOpen;

        void Awake()
        {
            // VRInteractionManager mutlaka kurulu olsun — VRSceneBootstrapper kurar
            if (VRInteractionManager.Instance == null)
            {
                var managerGO = new GameObject("VRInteractionManager (fallback)");
                managerGO.AddComponent<VRInteractionManager>();
            }

            // VRUIManager
            var uiManagerGO = new GameObject("VRUIManager");
            uiManagerGO.transform.SetParent(transform, false);
            _ui = uiManagerGO.AddComponent<VRUIManager>();

            // XR Origin / kamera / kumandaları bul
            ResolveSceneReferences();

            // Ana paneller (dünya uzayı)
            BuildMainMenu();
            BuildSettingsPanel();
            BuildHelpPanel();
            BuildNotifications();

            // Kumanda-bağımlı UI'lar
            if (createWristMenu)  BuildWristMenu();
            if (createRadialMenu) BuildRadialMenu();
            if (createTooltips)   BuildTooltips();

            // Ray Interactor'lara görsel feedback ekle
            AddPointerVisuals();

            // Grabbable'lara bilgi paneli ekle
            if (addInfoPanelsToGrabbables) AttachInfoPanelsToGrabbables();

            // Input actions
            CreateInputActions();
        }

        void Start()
        {
            // VRGrabbable.selectEntered üzerinden haptic puls için subscribe
            VRInteractionManager.OnObjectGrabbed += HandleHapticOnGrab;
            VRInteractionManager.OnObjectUsed += HandleHapticOnUse;
        }

        void OnDestroy()
        {
            VRInteractionManager.OnObjectGrabbed -= HandleHapticOnGrab;
            VRInteractionManager.OnObjectUsed -= HandleHapticOnUse;
            DisposeInputActions();
        }

        // ───────── Sahne referansları ─────────
        void ResolveSceneReferences()
        {
            var origin = FindObjectOfType<XROrigin>();
            if (origin != null)
            {
                _ui.headTransform = origin.Camera != null ? origin.Camera.transform : null;
            }

            // Sol/sağ kumanda transform'ları
            var controllers = FindObjectsOfType<ActionBasedController>();
            foreach (var c in controllers)
            {
                if (c == null) continue;
                if (c.name.ToLower().Contains("left")) _ui.leftControllerTransform = c.transform;
                else if (c.name.ToLower().Contains("right")) _ui.rightControllerTransform = c.transform;
            }
        }

        // ───────── Paneller ─────────
        void BuildMainMenu()
        {
            var canvas = VRUIFactory.CreateWorldCanvas("VRMainMenu", transform,
                VRUITheme.MainMenuWidth, VRUITheme.MainMenuHeight);
            var panel = canvas.gameObject.AddComponent<VRMainMenuPanel>();
            _ui.mainMenu = panel;
        }

        void BuildSettingsPanel()
        {
            var canvas = VRUIFactory.CreateWorldCanvas("VRSettings", transform,
                VRUITheme.SettingsWidth, VRUITheme.SettingsHeight);
            var panel = canvas.gameObject.AddComponent<VRSettingsPanel>();
            _ui.settingsPanel = panel;
        }

        void BuildHelpPanel()
        {
            var canvas = VRUIFactory.CreateWorldCanvas("VRHelp", transform,
                VRUITheme.SettingsWidth, VRUITheme.SettingsHeight);
            var panel = canvas.gameObject.AddComponent<VRHelpPanel>();
            _ui.helpPanel = panel;
        }

        void BuildNotifications()
        {
            // Bildirim alanı — bildirimler dünyada yüzer ama bu canvas head-locked tracking yapar
            var canvas = VRUIFactory.CreateWorldCanvas("VRNotifications", transform,
                600f, 600f);
            // Bu canvas raycaster'a ihtiyaç duymaz — bildirimler etkileşimsizdir
            var raycaster = canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
            if (raycaster != null) Destroy(raycaster);
            var ctl = canvas.gameObject.AddComponent<VRNotificationController>();
            _ui.notifications = ctl;
        }

        // ───────── Bilek Menüsü ─────────
        void BuildWristMenu()
        {
            if (_ui.leftControllerTransform == null) return;
            var canvas = VRUIFactory.CreateWorldCanvas("WristMenu", _ui.leftControllerTransform,
                VRUITheme.WristMenuWidth, VRUITheme.WristMenuHeight);

            // Yerel konum/dönüş — bilek üzerinde
            canvas.transform.localPosition = VRUITheme.WristMenuLocalPosition;
            canvas.transform.localRotation = Quaternion.Euler(VRUITheme.WristMenuLocalEuler);

            var wrist = canvas.gameObject.AddComponent<VRWristMenu>();
            _ui.wristMenu = wrist;
        }

        // ───────── Radial Menü ─────────
        void BuildRadialMenu()
        {
            if (_ui.rightControllerTransform == null) return;
            var canvas = VRUIFactory.CreateWorldCanvas("RadialMenu", _ui.rightControllerTransform,
                500f, 500f);
            // Kumandanın hafif önünde
            canvas.transform.localPosition = new Vector3(0f, 0.06f, 0.18f);
            canvas.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            _radial = canvas.gameObject.AddComponent<VRRadialMenu>();
            _ui.radialMenu = _radial;
        }

        // ───────── Tooltipler ─────────
        void BuildTooltips()
        {
            if (_ui.leftControllerTransform != null)
            {
                var canvas = VRUIFactory.CreateWorldCanvas("LeftTooltip", _ui.leftControllerTransform,
                    VRUITheme.TooltipWidth, VRUITheme.TooltipHeight);
                canvas.transform.localPosition = new Vector3(0f, 0.16f, 0f);
                canvas.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
                var tt = canvas.gameObject.AddComponent<VRTooltipController>();
                tt.defaultMessage = "Joystick: Hareket  •  Menu: Ana menü";
                // Tooltip etkileşimsizdir, raycaster sökülebilir
                var rc = canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
                if (rc != null) Destroy(rc);
                _ui.leftTooltip = tt;
            }
            if (_ui.rightControllerTransform != null)
            {
                var canvas = VRUIFactory.CreateWorldCanvas("RightTooltip", _ui.rightControllerTransform,
                    VRUITheme.TooltipWidth, VRUITheme.TooltipHeight);
                canvas.transform.localPosition = new Vector3(0f, 0.16f, 0f);
                canvas.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f);
                var tt = canvas.gameObject.AddComponent<VRTooltipController>();
                tt.defaultMessage = "Trigger: Seç  •  A: Hızlı menü";
                var rc = canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
                if (rc != null) Destroy(rc);
                _ui.rightTooltip = tt;
            }
        }

        // ───────── Pointer Visuals ─────────
        void AddPointerVisuals()
        {
            var rays = FindObjectsOfType<XRRayInteractor>();
            foreach (var ray in rays)
            {
                if (ray == null) continue;
                if (ray.GetComponent<VRPointerVisuals>() == null)
                {
                    var pv = ray.gameObject.AddComponent<VRPointerVisuals>();
                    // Sol/sağ ayırt et — sadece tek alan
                    var parent = ray.transform.parent;
                    if (parent != null)
                    {
                        if (parent.name.ToLower().Contains("left"))
                            _ui.leftPointerVisuals = pv;
                        else if (parent.name.ToLower().Contains("right"))
                            _ui.rightPointerVisuals = pv;
                    }
                }
            }
        }

        // ───────── Grabbable bilgi panelleri ─────────
        void AttachInfoPanelsToGrabbables()
        {
            var grabbables = FindObjectsOfType<VRGrabbable>();
            foreach (var g in grabbables)
            {
                if (g == null) continue;
                if (g.GetComponent<VRInfoPanel>() == null)
                    g.gameObject.AddComponent<VRInfoPanel>();
            }
        }

        // ───────── Input Actions ─────────
        void CreateInputActions()
        {
            // Menu (≡) düğmesi — Oculus'ta sol kumandanın menü düğmesi.
            _leftMenuAction = new InputAction("LeftMenu", InputActionType.Button,
                "<XRController>{LeftHand}/menu");
            _leftMenuAction.AddBinding("<XRController>{LeftHand}/menuButton"); // bazı runtime'larda
            _leftMenuAction.performed += _ => _ui.ToggleMainMenu();
            _leftMenuAction.Enable();

            // A düğmesi — sağ kumandanın primaryButton'ı (Quest A)
            _rightPrimaryAction = new InputAction("RightPrimary", InputActionType.Button,
                "<XRController>{RightHand}/primaryButton");
            _rightPrimaryAction.started += _ => OpenRadial();
            _rightPrimaryAction.canceled += _ => CloseRadial();
            _rightPrimaryAction.Enable();

            // Sağ joystick — radial menü yönü için
            _rightStickAction = new InputAction("RightStick", InputActionType.Value,
                "<XRController>{RightHand}/{Primary2DAxis}");
            _rightStickAction.expectedControlType = "Vector2";
            _rightStickAction.Enable();
        }

        void DisposeInputActions()
        {
            _leftMenuAction?.Dispose();     _leftMenuAction = null;
            _rightPrimaryAction?.Dispose(); _rightPrimaryAction = null;
            _rightStickAction?.Dispose();   _rightStickAction = null;
        }

        void Update()
        {
            // Radial menü açıkken joystick yönüyle dilim seç
            if (_radial != null && _radialOpen && _rightStickAction != null)
            {
                Vector2 stick = _rightStickAction.ReadValue<Vector2>();
                _radial.UpdateDirection(stick);
            }
        }

        void OpenRadial()
        {
            if (_radial == null) return;
            // Eğer büyük bir panel açıksa kapat, sonra aç (odak çakışmasını önle)
            if (_ui.IsAnyPanelOpen) return;
            _radialOpen = true;
            _radial.OpenMenu();
            VRHapticFeedback.Pulse(GetRightController(),
                VRUITheme.HapticHoverAmplitude * 2f, VRUITheme.HapticHoverDuration);
        }

        void CloseRadial()
        {
            if (_radial == null || !_radialOpen) return;
            _radialOpen = false;
            _radial.CloseMenu(applySelection: true);
            VRHapticFeedback.Pulse(GetRightController(),
                VRUITheme.HapticClickAmplitude, VRUITheme.HapticClickDuration);
        }

        XRBaseController GetRightController()
        {
            if (_ui.rightControllerTransform == null) return null;
            return _ui.rightControllerTransform.GetComponent<XRBaseController>();
        }

        // ───────── Etkileşim → Haptic ─────────
        void HandleHapticOnGrab(VRGrabbable obj)
        {
            // Her iki elde de hafif puls — hangi elin yakaladığını bilmek zor olduğundan
            VRHapticFeedback.PulseBoth(VRUITheme.HapticClickAmplitude, VRUITheme.HapticClickDuration);
        }

        void HandleHapticOnUse(VRGrabbable obj)
        {
            VRHapticFeedback.PulseBoth(VRUITheme.HapticAlertAmplitude, VRUITheme.HapticAlertDuration);
        }
    }
}
