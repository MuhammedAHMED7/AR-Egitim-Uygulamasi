using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

namespace AREgitim.VR
{
    /// <summary>
    /// VR sahnesinin tüm bileşenlerini Awake'de programatik olarak inşa eder.
    /// XR Origin, iki kumanda (ray + direct interactor), teleport sağlayıcısı,
    /// hareket sağlayıcıları ve örnek dünya (zemin + tutulabilir nesneler) oluşturur.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class VRSceneBootstrapper : MonoBehaviour
    {
        [Header("Sahne İçeriği")]
        public bool createDemoEnvironment = true;
        public int demoGrabbableCount = 4;

        [Header("Başlangıç Konumu")]
        public Vector3 startPosition = Vector3.zero;

        XROrigin _xrOrigin;
        GameObject _cameraOffset;

        // Input actions — runtime'da oluşturulur, properties ile referans verilir
        InputAction _leftPos, _leftRot, _leftSelect, _leftSelectVal, _leftActivate, _leftUIPress, _leftMove, _leftHaptic;
        InputAction _rightPos, _rightRot, _rightSelect, _rightSelectVal, _rightActivate, _rightUIPress, _rightTurn, _rightHaptic;
        InputAction _rightTeleportSelect, _rightTeleportCancel;
        InputAction _headPos, _headRot;

        void Awake()
        {
            // VRInteractionManager singleton
            if (VRInteractionManager.Instance == null)
            {
                var managerGO = new GameObject("VRInteractionManager");
                managerGO.transform.SetParent(transform, false);
                managerGO.AddComponent<VRInteractionManager>();
            }

            CreateInputActions();
            CreateXRInteractionManager();
            BuildXROrigin();
            BuildControllers();
            BuildMovementProviders();
            BuildEventSystem();

            if (createDemoEnvironment) BuildDemoEnvironment();
        }

        // ───────── Input Actions ─────────
        void CreateInputActions()
        {
            // Sol kumanda
            _leftPos = MakeAction("LeftPosition", InputActionType.Value, "<XRController>{LeftHand}/devicePosition", "Vector3");
            _leftRot = MakeAction("LeftRotation", InputActionType.Value, "<XRController>{LeftHand}/deviceRotation", "Quaternion");
            _leftSelect = MakeAction("LeftSelect", InputActionType.Button, "<XRController>{LeftHand}/triggerPressed");
            _leftSelectVal = MakeAction("LeftSelectVal", InputActionType.Value, "<XRController>{LeftHand}/trigger", "Axis");
            _leftActivate = MakeAction("LeftActivate", InputActionType.Button, "<XRController>{LeftHand}/gripPressed");
            _leftUIPress = MakeAction("LeftUIPress", InputActionType.Button, "<XRController>{LeftHand}/triggerPressed");
            _leftMove = MakeAction("LeftMove", InputActionType.Value, "<XRController>{LeftHand}/{Primary2DAxis}", "Vector2");
            _leftHaptic = MakeAction("LeftHaptic", InputActionType.PassThrough, "<XRController>{LeftHand}");

            // Sağ kumanda
            _rightPos = MakeAction("RightPosition", InputActionType.Value, "<XRController>{RightHand}/devicePosition", "Vector3");
            _rightRot = MakeAction("RightRotation", InputActionType.Value, "<XRController>{RightHand}/deviceRotation", "Quaternion");
            _rightSelect = MakeAction("RightSelect", InputActionType.Button, "<XRController>{RightHand}/triggerPressed");
            _rightSelectVal = MakeAction("RightSelectVal", InputActionType.Value, "<XRController>{RightHand}/trigger", "Axis");
            _rightActivate = MakeAction("RightActivate", InputActionType.Button, "<XRController>{RightHand}/gripPressed");
            _rightUIPress = MakeAction("RightUIPress", InputActionType.Button, "<XRController>{RightHand}/triggerPressed");
            _rightTurn = MakeAction("RightTurn", InputActionType.Value, "<XRController>{RightHand}/{Primary2DAxis}", "Vector2");
            _rightHaptic = MakeAction("RightHaptic", InputActionType.PassThrough, "<XRController>{RightHand}");

            // Teleport (sağ kumanda thumbstick yukarı)
            _rightTeleportSelect = MakeAction("RightTeleportSelect", InputActionType.Button, "<XRController>{RightHand}/{Primary2DAxis}/y");
            _rightTeleportCancel = MakeAction("RightTeleportCancel", InputActionType.Button, "<XRController>{RightHand}/secondaryButton");

            // Kafa
            _headPos = MakeAction("HeadPosition", InputActionType.Value, "<XRHMD>/centerEyePosition", "Vector3");
            _headRot = MakeAction("HeadRotation", InputActionType.Value, "<XRHMD>/centerEyeRotation", "Quaternion");

            // Tümünü etkinleştir
            EnableAll();
        }

        InputAction MakeAction(string name, InputActionType type, string binding, string expectedControlType = null)
        {
            var act = new InputAction(name, type, binding);
            if (!string.IsNullOrEmpty(expectedControlType)) act.expectedControlType = expectedControlType;
            return act;
        }

        void EnableAll()
        {
            _leftPos.Enable(); _leftRot.Enable(); _leftSelect.Enable(); _leftSelectVal.Enable();
            _leftActivate.Enable(); _leftUIPress.Enable(); _leftMove.Enable(); _leftHaptic.Enable();

            _rightPos.Enable(); _rightRot.Enable(); _rightSelect.Enable(); _rightSelectVal.Enable();
            _rightActivate.Enable(); _rightUIPress.Enable(); _rightTurn.Enable(); _rightHaptic.Enable();
            _rightTeleportSelect.Enable(); _rightTeleportCancel.Enable();

            _headPos.Enable(); _headRot.Enable();
        }

        // ───────── XR Interaction Manager ─────────
        void CreateXRInteractionManager()
        {
            var existing = FindObjectOfType<XRInteractionManager>();
            if (existing != null) return;
            var go = new GameObject("XR Interaction Manager");
            go.AddComponent<XRInteractionManager>();
        }

        // ───────── XR Origin ─────────
        void BuildXROrigin()
        {
            var originGO = new GameObject("XR Origin");
            originGO.SetActive(false); // Awake'lerin erken çalışmasını önle
            originGO.transform.position = startPosition;
            _xrOrigin = originGO.AddComponent<XROrigin>();
            _xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            _xrOrigin.CameraYOffset = 1.6f;

            // Camera Offset child
            _cameraOffset = new GameObject("Camera Offset");
            _cameraOffset.transform.SetParent(originGO.transform, false);
            _xrOrigin.CameraFloorOffsetObject = _cameraOffset;

            // Main Camera
            var camGO = new GameObject("Main Camera");
            camGO.transform.SetParent(_cameraOffset.transform, false);
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 1000f;
            camGO.AddComponent<AudioListener>();

            // Tracked Pose Driver (kafa)
            var headPose = camGO.AddComponent<TrackedPoseDriver>();
            headPose.positionInput = new InputActionProperty(_headPos);
            headPose.rotationInput = new InputActionProperty(_headRot);

            _xrOrigin.Camera = cam;

            // Locomotion System
            var locoGO = new GameObject("Locomotion System");
            locoGO.transform.SetParent(originGO.transform, false);
            var loco = locoGO.AddComponent<LocomotionSystem>();
            loco.xrOrigin = _xrOrigin;

            // CharacterController
            var cc = originGO.AddComponent<CharacterController>();
            cc.height = 1.7f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.85f, 0f);
            cc.minMoveDistance = 0f;

            // Driver
            originGO.AddComponent<CharacterControllerDriver>();
        }

        // ───────── Kumandalar ─────────
        void BuildControllers()
        {
            BuildController("LeftHand Controller", isRight: false);
            BuildController("RightHand Controller", isRight: true);
        }

        void BuildController(string name, bool isRight)
        {
            var go = new GameObject(name);
            go.SetActive(false); // Önce referansları bağla, sonra aktif et
            go.transform.SetParent(_cameraOffset.transform, false);

            var controller = go.AddComponent<ActionBasedController>();
            controller.positionAction = new InputActionProperty(isRight ? _rightPos : _leftPos);
            controller.rotationAction = new InputActionProperty(isRight ? _rightRot : _leftRot);
            controller.selectAction = new InputActionProperty(isRight ? _rightSelect : _leftSelect);
            controller.selectActionValue = new InputActionProperty(isRight ? _rightSelectVal : _leftSelectVal);
            controller.activateAction = new InputActionProperty(isRight ? _rightActivate : _leftActivate);
            controller.uiPressAction = new InputActionProperty(isRight ? _rightUIPress : _leftUIPress);
            controller.hapticDeviceAction = new InputActionProperty(isRight ? _rightHaptic : _leftHaptic);

            // Ray Interactor (uzak nesneler için)
            var rayGO = new GameObject("Ray Interactor");
            rayGO.transform.SetParent(go.transform, false);
            var ray = rayGO.AddComponent<XRRayInteractor>();
            ray.lineType = XRRayInteractor.LineType.BezierCurve;
            ray.endPointDistance = 10f;

            var line = rayGO.AddComponent<LineRenderer>();
            var visual = rayGO.AddComponent<XRInteractorLineVisual>();
            visual.lineWidth = 0.005f;
            visual.lineLength = 10f;
            visual.smoothMovement = true;
            var lineShader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (lineShader != null) line.material = new Material(lineShader);

            // Direct Interactor (yakın nesneler için)
            var directGO = new GameObject("Direct Interactor");
            directGO.transform.SetParent(go.transform, false);
            var sphereCol = directGO.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.08f;
            directGO.AddComponent<XRDirectInteractor>();

            // Görsel ipucu — küçük bir küp kumandayı temsil eder
            var visualGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualGO.name = "ControllerVisual";
            visualGO.transform.SetParent(go.transform, false);
            visualGO.transform.localScale = new Vector3(0.05f, 0.05f, 0.12f);
            Object.Destroy(visualGO.GetComponent<Collider>());
            var vr = visualGO.GetComponent<Renderer>();
            if (vr != null) ApplyColor(vr, isRight ? new Color(0.30f, 0.78f, 1f) : new Color(0.95f, 0.55f, 0.30f));

            go.SetActive(true);
        }

        // ───────── Hareket Sağlayıcıları ─────────
        void BuildMovementProviders()
        {
            var originGO = _xrOrigin.gameObject;
            var locoSystem = originGO.GetComponentInChildren<LocomotionSystem>();

            // Continuous Move (sol thumbstick)
            var moveProv = originGO.AddComponent<ActionBasedContinuousMoveProvider>();
            moveProv.system = locoSystem;
            moveProv.leftHandMoveAction = new InputActionProperty(_leftMove);
            moveProv.rightHandMoveAction = new InputActionProperty(new InputAction()); // sağ kullanılmıyor, boş
            moveProv.moveSpeed = 1.8f;
            moveProv.useGravity = true;
            moveProv.enableStrafe = true;

            // Snap Turn (sağ thumbstick yatay)
            var snapProv = originGO.AddComponent<ActionBasedSnapTurnProvider>();
            snapProv.system = locoSystem;
            snapProv.rightHandSnapTurnAction = new InputActionProperty(_rightTurn);
            snapProv.leftHandSnapTurnAction = new InputActionProperty(new InputAction());
            snapProv.turnAmount = 30f;

            // Continuous Turn (alternatif, başlangıçta kapalı)
            var smoothProv = originGO.AddComponent<ActionBasedContinuousTurnProvider>();
            smoothProv.system = locoSystem;
            smoothProv.rightHandTurnAction = new InputActionProperty(_rightTurn);
            smoothProv.leftHandTurnAction = new InputActionProperty(new InputAction());
            smoothProv.turnSpeed = 80f;
            smoothProv.enabled = false;

            // Teleport
            var teleport = originGO.AddComponent<TeleportationProvider>();
            teleport.system = locoSystem;

            // Movement Controller'ı kur ve bağla
            var movementCtrl = originGO.AddComponent<VRMovementController>();
            movementCtrl.continuousMove = moveProv;
            movementCtrl.snapTurn = snapProv;
            movementCtrl.smoothTurn = smoothProv;
            movementCtrl.teleport = teleport;

            // CharacterControllerDriver locomotion provider'ı ata
            var driver = originGO.GetComponent<CharacterControllerDriver>();
            if (driver != null) driver.locomotionProvider = moveProv;

            // Tüm referanslar bağlandı; şimdi XR Origin'i etkinleştir
            originGO.SetActive(true);
        }

        // ───────── UI EventSystem ─────────
        void BuildEventSystem()
        {
            var existing = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (existing != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ───────── Demo Ortam ─────────
        void BuildDemoEnvironment()
        {
            // Zemin (teleport edilebilir)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor (Teleport)";
            floor.transform.localScale = new Vector3(2f, 1f, 2f);
            floor.transform.position = Vector3.zero;
            var floorR = floor.GetComponent<Renderer>();
            if (floorR != null) ApplyColor(floorR, new Color(0.20f, 0.22f, 0.28f));
            floor.AddComponent<VRTeleportTarget>();

            // Birkaç tutulabilir nesne
            Color[] colors = {
                new Color(0.30f, 0.78f, 1f),
                new Color(0.95f, 0.55f, 0.30f),
                new Color(0.60f, 0.95f, 0.50f),
                new Color(0.95f, 0.45f, 0.85f)
            };
            for (int i = 0; i < demoGrabbableCount; i++)
            {
                float angle = (i / (float)demoGrabbableCount) * Mathf.PI * 2f;
                float radius = 1.2f;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * radius, 1.0f, Mathf.Cos(angle) * radius + 1.5f);
                CreateGrabbableCube($"Grabbable_{i + 1}", pos, colors[i % colors.Length]);
            }

            // Masa (görsel)
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.position = new Vector3(0f, 0.4f, 1.5f);
            table.transform.localScale = new Vector3(1.5f, 0.8f, 1f);
            var tr = table.GetComponent<Renderer>();
            if (tr != null) ApplyColor(tr, new Color(0.45f, 0.35f, 0.25f));
        }

        GameObject CreateGrabbableCube(string name, Vector3 pos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.18f;

            var r = go.GetComponent<Renderer>();
            if (r != null) ApplyColor(r, color);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var grab = go.AddComponent<VRGrabbable>();
            grab.displayName = name;
            grab.highlightColor = Color.Lerp(color, Color.white, 0.4f);
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;

            return go;
        }

        static void ApplyColor(Renderer r, Color c)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) return;
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            r.sharedMaterial = mat;
        }

        void OnDestroy()
        {
            // Tüm action'ları temizle
            _leftPos?.Dispose(); _leftRot?.Dispose(); _leftSelect?.Dispose(); _leftSelectVal?.Dispose();
            _leftActivate?.Dispose(); _leftUIPress?.Dispose(); _leftMove?.Dispose(); _leftHaptic?.Dispose();
            _rightPos?.Dispose(); _rightRot?.Dispose(); _rightSelect?.Dispose(); _rightSelectVal?.Dispose();
            _rightActivate?.Dispose(); _rightUIPress?.Dispose(); _rightTurn?.Dispose(); _rightHaptic?.Dispose();
            _rightTeleportSelect?.Dispose(); _rightTeleportCancel?.Dispose();
            _headPos?.Dispose(); _headRot?.Dispose();
        }
    }
}