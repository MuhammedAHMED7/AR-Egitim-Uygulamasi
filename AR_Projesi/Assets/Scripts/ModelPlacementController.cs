using UnityEngine;

namespace AREgitim.UI
{
    /// <summary>
    /// Model yerleştirme orkestrasyonu.
    /// ARUIManager.OnModelPlaceRequested olayını dinler;
    /// Reticle pozisyonunda model instantiate eder ve World-Space paneli ona bağlar.
    /// </summary>
    public class ModelPlacementController : MonoBehaviour
    {
        public ReticleController reticle;
        public WorldSpacePanel worldPanel;
        public TouchGestureController gestureController;
        public Transform placedRoot;

        GameObject _currentInstance;

        void Start()
        {
            if (ARUIManager.Instance != null)
            {
                ARUIManager.Instance.OnModelPlaceRequested += HandlePlace;
                ARUIManager.Instance.OnResetRequested += HandleReset;
            }

            if (worldPanel != null) worldPanel.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (ARUIManager.Instance != null)
            {
                ARUIManager.Instance.OnModelPlaceRequested -= HandlePlace;
                ARUIManager.Instance.OnResetRequested -= HandleReset;
            }
        }

        void HandlePlace(ARUIManager.ModelData data)
        {
            if (reticle == null || !reticle.HasValidPose)
            {
                if (ARUIManager.Instance != null)
                    ARUIManager.Instance.ShowNotification("Zemin henüz hazır değil");
                return;
            }

            // Önceki örneği sil
            if (_currentInstance != null) Destroy(_currentInstance);

            GameObject go;
            if (data.prefab != null)
            {
                go = Instantiate(data.prefab, reticle.CurrentPose.position, reticle.CurrentPose.rotation);
            }
            else
            {
                // Prefab yoksa primitive oluştur
                go = CreatePrimitiveForId(data.id, data.accentColor);
                go.transform.position = reticle.CurrentPose.position;
                go.transform.rotation = reticle.CurrentPose.rotation;
                go.transform.localScale = Vector3.one * 0.25f;
            }
            go.name = "AR_Placed_" + data.id;
            if (placedRoot != null) go.transform.SetParent(placedRoot, true);
            _currentInstance = go;

            // World-space paneli ve gesture'i hedefe bağla
            if (worldPanel != null)
            {
                worldPanel.target = go.transform;
                worldPanel.gameObject.SetActive(true);
            }
            if (gestureController != null) gestureController.target = go.transform;

            if (ARUIManager.Instance != null)
                ARUIManager.Instance.ShowNotification($"{data.displayName} yerleştirildi");
        }

        void HandleReset()
        {
            if (_currentInstance != null)
            {
                Destroy(_currentInstance);
                _currentInstance = null;
            }
            if (worldPanel != null)
            {
                worldPanel.target = null;
                worldPanel.gameObject.SetActive(false);
            }
            if (gestureController != null) gestureController.target = null;
        }

        GameObject CreatePrimitiveForId(string id, Color color)
        {
            PrimitiveType t = id switch
            {
                "cube"     => PrimitiveType.Cube,
                "sphere"   => PrimitiveType.Sphere,
                "cylinder" => PrimitiveType.Cylinder,
                "capsule"  => PrimitiveType.Capsule,
                _          => PrimitiveType.Cube
            };
            var go = GameObject.CreatePrimitive(t);
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                var shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    if (mat.HasProperty("_Color"))     mat.SetColor("_Color", color);
                    r.sharedMaterial = mat;
                }
            }
            return go;
        }
    }
}
