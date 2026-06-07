// VRNetworkAvatar.cs
// Uzak oyuncuların görsel temsili: prosedürel oluşturulmuş kafa ve iki el.
// Sahip istemci için gizlenir (kendi avatarı görünmemeli).
// NetworkVariable değerlerini okuyup yumuşatma uygulayarak hareket eder.

using TMPro;
using UnityEngine;

namespace AREgitim.VR
{
    public class VRNetworkAvatar : MonoBehaviour
    {
        [Header("Görsel Ayarlar")]
        public float headRadius = 0.10f;
        public float handRadius = 0.04f;
        public float positionSmoothing = 18f;
        public float rotationSmoothing = 18f;
        public float nameTagHeight = 0.18f;

        // Görsel parçalar (prosedürel)
        Transform _headVisual;
        Transform _leftHandVisual;
        Transform _rightHandVisual;
        Transform _nameTagAnchor;
        Renderer _headRenderer;
        Renderer _leftRenderer;
        Renderer _rightRenderer;
        TextMeshPro _nameTag;
        Color _color = Color.white;
        VRNetworkPlayer _player;
        bool _isOwner;

        public void Initialize(VRNetworkPlayer player)
        {
            _player = player;
            _isOwner = player.IsOwner;
            BuildVisuals();
        }

        public void SetVisibleForOwner(bool visibleForOwner)
        {
            // Kendi avatarımız gizli olur — sadece diğerleri için görünür
            if (_headVisual) _headVisual.gameObject.SetActive(visibleForOwner);
            if (_leftHandVisual) _leftHandVisual.gameObject.SetActive(visibleForOwner);
            if (_rightHandVisual) _rightHandVisual.gameObject.SetActive(visibleForOwner);
            if (_nameTagAnchor) _nameTagAnchor.gameObject.SetActive(visibleForOwner);
        }

        public void SetDisplayName(string name)
        {
            if (_nameTag != null) _nameTag.text = name;
        }

        public void SetColor(Color c)
        {
            _color = c;
            ApplyColor();
        }

        void BuildVisuals()
        {
            // Tüm avatar GO'su içinde alt nesneler oluştur
            _headVisual = CreateSphere("HeadVisual", headRadius * 2f, transform);
            _leftHandVisual = CreateHandShape("LeftHandVisual", handRadius * 2f, transform);
            _rightHandVisual = CreateHandShape("RightHandVisual", handRadius * 2f, transform);

            _headRenderer = _headVisual.GetComponent<Renderer>();
            _leftRenderer = _leftHandVisual.GetComponent<Renderer>();
            _rightRenderer = _rightHandVisual.GetComponent<Renderer>();

            // İsim etiketi
            _nameTagAnchor = new GameObject("NameTagAnchor").transform;
            _nameTagAnchor.SetParent(_headVisual, false);
            _nameTagAnchor.localPosition = new Vector3(0, nameTagHeight, 0);

            var nameGO = new GameObject("NameTag");
            nameGO.transform.SetParent(_nameTagAnchor, false);
            _nameTag = nameGO.AddComponent<TextMeshPro>();
            _nameTag.text = "";
            _nameTag.alignment = TextAlignmentOptions.Center;
            _nameTag.fontSize = 0.5f;
            _nameTag.color = Color.white;
            _nameTag.fontStyle = FontStyles.Bold;
            var rt = _nameTag.rectTransform;
            rt.sizeDelta = new Vector2(2f, 0.4f);
            // İsim etiketi kameraya dönecek (LateUpdate'te yapılır)

            // Gözler için ufak detay: kafanın önüne iki küçük küre
            CreateEye(_headVisual, new Vector3(-0.035f, 0.01f, headRadius * 0.92f));
            CreateEye(_headVisual, new Vector3(0.035f, 0.01f, headRadius * 0.92f));

            ApplyColor();
        }

        Transform CreateSphere(string name, float diameter, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * diameter;
            // Fiziği kapat
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            return go.transform;
        }

        Transform CreateHandShape(string name, float diameter, Transform parent)
        {
            // Daha okunaklı: dikdörtgen bir küp + ön tarafta küçük bir küre
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);

            var palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            palm.name = "Palm";
            palm.transform.SetParent(root, false);
            palm.transform.localScale = new Vector3(diameter * 1.1f, diameter * 0.55f, diameter * 1.4f);
            var col1 = palm.GetComponent<Collider>(); if (col1) Destroy(col1);

            var thumb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            thumb.name = "Thumb";
            thumb.transform.SetParent(root, false);
            thumb.transform.localPosition = new Vector3(diameter * 0.55f, 0, diameter * 0.30f);
            thumb.transform.localScale = Vector3.one * diameter * 0.55f;
            var col2 = thumb.GetComponent<Collider>(); if (col2) Destroy(col2);

            return root;
        }

        void CreateEye(Transform headParent, Vector3 localPos)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(headParent, false);
            eye.transform.localPosition = localPos;
            eye.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f); // headVisual zaten ölçeklenmiş — relatif
            var col = eye.GetComponent<Collider>(); if (col) Destroy(col);
            var r = eye.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = new Material(Shader.Find("Standard"));
                r.material.color = Color.black;
            }
        }

        void ApplyColor()
        {
            var shader = Shader.Find("Standard");
            if (_headRenderer != null) _headRenderer.material = new Material(shader) { color = _color };
            if (_leftRenderer != null)
            {
                foreach (var r in _leftHandVisual.GetComponentsInChildren<Renderer>())
                    r.material = new Material(shader) { color = _color };
            }
            if (_rightRenderer != null)
            {
                foreach (var r in _rightHandVisual.GetComponentsInChildren<Renderer>())
                    r.material = new Material(shader) { color = _color };
            }
            if (_nameTag != null)
            {
                _nameTag.outlineColor = new Color32(0, 0, 0, 200);
                _nameTag.outlineWidth = 0.2f;
            }
        }

        void LateUpdate()
        {
            if (_player == null) return;
            // Sadece uzaktaki oyuncular için hareket uygulanır.
            // Sahip için: avatar zaten gizli — gene de güvenlik için atla.
            if (_isOwner) return;

            // Hedef pozları al
            var hp = _player.headPose.Value;
            var lp = _player.leftHandPose.Value;
            var rp = _player.rightHandPose.Value;

            // Yumuşatılmış konum/dönüş
            if (_headVisual != null)
            {
                _headVisual.position = Vector3.Lerp(_headVisual.position, hp.position, Time.deltaTime * positionSmoothing);
                _headVisual.rotation = Quaternion.Slerp(_headVisual.rotation, hp.rotation, Time.deltaTime * rotationSmoothing);
            }
            if (_leftHandVisual != null)
            {
                _leftHandVisual.position = Vector3.Lerp(_leftHandVisual.position, lp.position, Time.deltaTime * positionSmoothing);
                _leftHandVisual.rotation = Quaternion.Slerp(_leftHandVisual.rotation, lp.rotation, Time.deltaTime * rotationSmoothing);
            }
            if (_rightHandVisual != null)
            {
                _rightHandVisual.position = Vector3.Lerp(_rightHandVisual.position, rp.position, Time.deltaTime * positionSmoothing);
                _rightHandVisual.rotation = Quaternion.Slerp(_rightHandVisual.rotation, rp.rotation, Time.deltaTime * rotationSmoothing);
            }

            // İsim etiketi her zaman kameraya bakar
            if (_nameTag != null && Camera.main != null)
            {
                Vector3 dir = _nameTag.transform.position - Camera.main.transform.position;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    _nameTag.transform.rotation = Quaternion.LookRotation(dir);
                }
            }
        }
    }
}
