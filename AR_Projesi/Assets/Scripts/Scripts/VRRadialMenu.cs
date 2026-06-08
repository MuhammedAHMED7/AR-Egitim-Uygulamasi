using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// Sağ kumandadan A butonuna basınca açılan dairesel menü.
    /// Dünya uzayında değil, sağ kumandanın önünde belirir.
    /// Joystick yönüne göre dilim seçilir; A bırakıldığında uygulanır.
    /// </summary>
    public class VRRadialMenu : MonoBehaviour
    {
        [Serializable]
        public class RadialEntry
        {
            public string label;
            public Color color = Color.white;
            public Action callback;
        }

        [Header("Görsel")]
        [Tooltip("Dairesel menünün dış yarıçapı (canvas px).")]
        public float radius = 180f;

        [Tooltip("İçi boş halka kalınlığı (px).")]
        public float ringThickness = 80f;

        [Tooltip("Görünüme yumuşak gelme süresi.")]
        public float fadeDuration = 0.15f;

        CanvasGroup _canvasGroup;
        readonly List<RadialEntry> _entries = new List<RadialEntry>();
        readonly List<Image> _slices = new List<Image>();
        TextMeshProUGUI _centerLabel;
        int _highlightIndex = -1;
        bool _open;

        protected void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            // Varsayılan girişler
            SetEntries(new List<RadialEntry> {
                new RadialEntry { label = "Menü", color = VRUITheme.Accent,
                                  callback = () => { if (VRUIManager.Instance != null) VRUIManager.Instance.OpenMainMenu(); } },
                new RadialEntry { label = "Ayarlar", color = VRUITheme.ButtonHover,
                                  callback = () => { if (VRUIManager.Instance != null) VRUIManager.Instance.OpenSettings(); } },
                new RadialEntry { label = "Yardım", color = VRUITheme.Warning,
                                  callback = () => { if (VRUIManager.Instance != null) VRUIManager.Instance.OpenHelp(); } },
                new RadialEntry { label = "Ortala", color = VRUITheme.Success,
                                  callback = () => { if (VRUIManager.Instance != null) VRUIManager.Instance.RequestRecenter(); } },
            });

            BuildCenter();
        }

        public void SetEntries(List<RadialEntry> entries)
        {
            _entries.Clear();
            _entries.AddRange(entries);
            RebuildSlices();
        }

        void BuildCenter()
        {
            // İç daire — etiket için
            var centerGO = new GameObject("Center", typeof(RectTransform));
            centerGO.transform.SetParent(transform, false);
            var rt = (RectTransform)centerGO.transform;
            rt.sizeDelta = new Vector2((radius - ringThickness) * 2f, (radius - ringThickness) * 2f);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            var img = centerGO.AddComponent<Image>();
            img.sprite = VRUIFactory.RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = VRUITheme.PanelBackground;
            img.raycastTarget = false;

            _centerLabel = VRUIFactory.CreateText(centerGO.transform, "Yön seç",
                VRUITheme.FontBody, VRUITheme.TextSecondary,
                TextAlignmentOptions.Center, "CenterLabel");
        }

        void RebuildSlices()
        {
            // Önce eskileri temizle
            for (int i = _slices.Count - 1; i >= 0; i--)
            {
                if (_slices[i] != null) Destroy(_slices[i].gameObject);
            }
            _slices.Clear();

            int count = _entries.Count;
            if (count == 0) return;
            float anglePer = 360f / count;

            for (int i = 0; i < count; i++)
            {
                var entry = _entries[i];

                var sliceGO = new GameObject("Slice_" + i, typeof(RectTransform));
                sliceGO.transform.SetParent(transform, false);
                var rt = (RectTransform)sliceGO.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(radius * 2f, radius * 2f);
                rt.anchoredPosition = Vector2.zero;

                var img = sliceGO.AddComponent<Image>();
                img.sprite = VRUIFactory.RoundedSprite;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Radial360;
                img.fillOrigin = (int)Image.Origin360.Top;
                img.fillClockwise = true;
                img.fillAmount = 1f / count - 0.01f; // küçük boşluk
                img.color = entry.color * new Color(1, 1, 1, 0.35f);
                img.raycastTarget = false;
                // Her dilimi kendi açısına döndür
                rt.localRotation = Quaternion.Euler(0, 0, -anglePer * i);

                // Etiket
                float midAngle = (i * anglePer + anglePer * 0.5f) * Mathf.Deg2Rad;
                float labelRadius = radius - ringThickness * 0.5f;
                var lblGO = new GameObject("Label_" + i, typeof(RectTransform));
                lblGO.transform.SetParent(transform, false);
                var lrt = (RectTransform)lblGO.transform;
                lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.pivot = new Vector2(0.5f, 0.5f);
                lrt.sizeDelta = new Vector2(140, 50);
                // 12 yön: ekran üstü = 90°. Bizim açımız saat yönüne, 0° üstte.
                float screenAngle = 90f - (i * anglePer + anglePer * 0.5f);
                float rad = screenAngle * Mathf.Deg2Rad;
                lrt.anchoredPosition = new Vector2(Mathf.Cos(rad) * labelRadius,
                                                   Mathf.Sin(rad) * labelRadius);
                var tmp = lblGO.AddComponent<TextMeshProUGUI>();
                tmp.text = entry.label;
                tmp.fontSize = VRUITheme.FontBody;
                tmp.color = VRUITheme.TextPrimary;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold;
                tmp.raycastTarget = false;

                _slices.Add(img);
            }
        }

        /// <summary>Menüyü aç. Yön seçimi olmadan kapatılırsa hiçbir şey çalıştırmaz.</summary>
        public void OpenMenu()
        {
            _open = true;
            _canvasGroup.alpha = 1f;
            _highlightIndex = -1;
            UpdateHighlight();
        }

        public void CloseMenu(bool applySelection)
        {
            _open = false;
            _canvasGroup.alpha = 0f;
            if (applySelection && _highlightIndex >= 0 && _highlightIndex < _entries.Count)
            {
                _entries[_highlightIndex].callback?.Invoke();
            }
            _highlightIndex = -1;
            UpdateHighlight();
        }

        /// <summary>
        /// Joystick yön vektörünü (x,y) ile aktif dilimi belirler.
        /// VRSceneBootstrapper bunu her karede günceller (menü açıkken).
        /// </summary>
        public void UpdateDirection(Vector2 stick)
        {
            if (!_open || _entries.Count == 0) return;
            if (stick.sqrMagnitude < 0.25f)
            {
                if (_highlightIndex != -1)
                {
                    _highlightIndex = -1;
                    UpdateHighlight();
                }
                return;
            }
            // Yön → açı (üst = 0°, saat yönü pozitif)
            float angleDeg = Mathf.Atan2(stick.x, stick.y) * Mathf.Rad2Deg;
            if (angleDeg < 0f) angleDeg += 360f;
            int count = _entries.Count;
            float anglePer = 360f / count;
            int index = Mathf.FloorToInt(angleDeg / anglePer) % count;
            if (index != _highlightIndex)
            {
                _highlightIndex = index;
                UpdateHighlight();
            }
        }

        void UpdateHighlight()
        {
            for (int i = 0; i < _slices.Count; i++)
            {
                var entry = _entries[i];
                var img = _slices[i];
                if (img == null) continue;
                bool active = (i == _highlightIndex);
                img.color = entry.color * new Color(1, 1, 1, active ? 1f : 0.35f);
            }
            if (_centerLabel != null)
            {
                _centerLabel.text = _highlightIndex >= 0 && _highlightIndex < _entries.Count
                    ? _entries[_highlightIndex].label : "Yön seç";
                _centerLabel.color = _highlightIndex >= 0
                    ? VRUITheme.TextPrimary : VRUITheme.TextSecondary;
            }
        }

        public bool IsOpen => _open;
    }
}
