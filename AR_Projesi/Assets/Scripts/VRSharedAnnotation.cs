// VRSharedAnnotation.cs
// Oyuncular dünya üzerinde "buraya bak" tarzı görsel işaretler bırakabilir.
// Bu işaretler tüm istemcilerde görünür ve birkaç saniye sonra kaybolur.

using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace AREgitim.VR
{
    public class VRSharedAnnotation : NetworkBehaviour
    {
        public static VRSharedAnnotation Instance { get; private set; }

        [Header("Görsel")]
        public float lifetimeSeconds = 6f;
        public float pulseSize = 0.12f;
        public Color defaultColor = new Color(1f, 0.55f, 0.30f);

        readonly List<GameObject> _liveMarkers = new List<GameObject>();

        public override void OnNetworkSpawn()
        {
            if (Instance == null) Instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
            ClearAll();
        }

        /// <summary>Bir konuma not bırak. Tüm istemcilere yayınlar.</summary>
        public void DropAnnotation(Vector3 worldPos, Color color, string label = "")
        {
            FixedString64Bytes lbl = new FixedString64Bytes(label ?? "");
            DropAnnotationServerRpc(worldPos, color, lbl);
        }

        [ServerRpc(RequireOwnership = false)]
        void DropAnnotationServerRpc(Vector3 worldPos, Color color, FixedString64Bytes label)
        {
            // Sunucu tüm istemcilere yayın yapar
            ShowAnnotationClientRpc(worldPos, color, label);
        }

        [ClientRpc]
        void ShowAnnotationClientRpc(Vector3 worldPos, Color color, FixedString64Bytes label)
        {
            SpawnMarker(worldPos, color, label.ToString());
        }

        void SpawnMarker(Vector3 worldPos, Color color, string label)
        {
            var marker = new GameObject("Annotation");
            marker.transform.position = worldPos;
            _liveMarkers.Add(marker);

            // Pulsasyonlu küre
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Pulse";
            sphere.transform.SetParent(marker.transform, false);
            sphere.transform.localScale = Vector3.one * pulseSize;
            var col = sphere.GetComponent<Collider>(); if (col) Destroy(col);
            var r = sphere.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            r.material = mat;

            // Etiket
            if (!string.IsNullOrEmpty(label))
            {
                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(marker.transform, false);
                labelGO.transform.localPosition = new Vector3(0, pulseSize * 1.8f, 0);
                var tmp = labelGO.AddComponent<TextMeshPro>();
                tmp.text = label;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 0.5f;
                tmp.color = Color.white;
                tmp.outlineColor = Color.black;
                tmp.outlineWidth = 0.2f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.rectTransform.sizeDelta = new Vector2(2f, 0.4f);
            }

            // Yaşam süresi sonunda yok et
            var lifecycle = marker.AddComponent<AnnotationLifecycle>();
            lifecycle.lifetime = lifetimeSeconds;
            lifecycle.mat = mat;
        }

        void ClearAll()
        {
            foreach (var m in _liveMarkers) { if (m != null) Destroy(m); }
            _liveMarkers.Clear();
        }

        class AnnotationLifecycle : MonoBehaviour
        {
            public float lifetime = 6f;
            public Material mat;
            float _t;

            void Update()
            {
                _t += Time.deltaTime;
                // Pulsasyon
                float pulse = 1f + 0.25f * Mathf.Sin(_t * 5f);
                transform.localScale = Vector3.one * pulse;
                // Soldurma
                if (mat != null)
                {
                    var c = mat.color;
                    c.a = Mathf.Clamp01(1f - (_t / lifetime));
                    mat.color = c;
                }
                // Kameraya doğru bak (etiket için)
                if (Camera.main != null)
                {
                    var label = transform.Find("Label");
                    if (label != null)
                    {
                        Vector3 dir = label.position - Camera.main.transform.position;
                        if (dir.sqrMagnitude > 0.0001f)
                            label.rotation = Quaternion.LookRotation(dir);
                    }
                }
                if (_t >= lifetime) Destroy(gameObject);
            }
        }
    }
}
