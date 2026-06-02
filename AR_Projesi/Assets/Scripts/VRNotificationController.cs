using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AREgitim.VR
{
    /// <summary>
    /// VR ortamında geçici bildirimler (toast) gösterir.
    /// Kafa hareketiyle birlikte sürüklenen, kullanıcının görüş alanının
    /// alt-orta kısmında belirip kaybolan paneller.
    ///
    /// Üst üste birden çok bildirim sıralanır; her biri yumuşakça
    /// görünür ve solar.
    /// </summary>
    public class VRNotificationController : MonoBehaviour
    {
        public enum NotificationType { Info, Success, Warning, Hint, Error }

        [Tooltip("Her bildirimin ekranda kalma süresi (saniye).")]
        public float displayDuration = VRUITheme.ToastDuration;

        [Tooltip("Aynı anda gösterilecek maksimum bildirim sayısı.")]
        public int maxConcurrent = 3;

        [Tooltip("Bildirimlerin kullanıcıyı takip ederken kafayı geriden takip hızı (smoothing).")]
        public float headFollowSmoothing = 6f;

        Transform _head;
        readonly List<NotificationInstance> _active = new List<NotificationInstance>();

        class NotificationInstance
        {
            public GameObject root;
            public CanvasGroup canvasGroup;
            public TextMeshProUGUI text;
            public Image background;
            public Image accentBar;
            public float spawnTime;
            public float displayUntil;
            public int slotIndex;
        }

        void Awake()
        {
            // Bu Canvas WorldSpace — VRUIBootstrapper yapılandırıyor.
            // Burada sadece konum/yönelimi her karede günceller.
        }

        void Start()
        {
            if (VRUIManager.Instance != null)
                _head = VRUIManager.Instance.headTransform;
            if (_head == null && Camera.main != null)
                _head = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (_head == null) return;

            // Bildirim sahnesini kullanıcının önünde, görüş alanının altında tut
            Vector3 forward = _head.forward;
            // Yatay düzleme yansıt — alanı sabit yükseklikte tut
            Vector3 forwardFlat = new Vector3(forward.x, 0f, forward.z);
            if (forwardFlat.sqrMagnitude < 0.0001f) forwardFlat = Vector3.forward;
            forwardFlat.Normalize();

            float targetY = _head.position.y + VRUITheme.NotificationHeightOffset;
            Vector3 targetPos = _head.position + forwardFlat * VRUITheme.NotificationDistance;
            targetPos.y = targetY;

            // Smooth follow — kafa hareketi anında değil hafif gecikmeyle
            transform.position = Vector3.Lerp(transform.position, targetPos,
                                               Time.unscaledDeltaTime * headFollowSmoothing);

            // Yüzü kullanıcıya dön
            Vector3 toUser = _head.position - transform.position;
            toUser.y = 0f;
            if (toUser.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(-toUser.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target,
                                                       Time.unscaledDeltaTime * headFollowSmoothing);
            }
        }

        public void Show(string message, NotificationType type = NotificationType.Info)
        {
            // Çok bildirim varsa en eskiyi kapat
            if (_active.Count >= maxConcurrent && _active.Count > 0)
            {
                var oldest = _active[0];
                _active.RemoveAt(0);
                if (oldest.root != null) StartCoroutine(FadeAndDestroy(oldest));
            }

            var inst = CreateNotification(message, type);
            inst.spawnTime = Time.unscaledTime;
            inst.displayUntil = inst.spawnTime + displayDuration;
            _active.Add(inst);
            RecomputeSlots();
            StartCoroutine(NotificationLifecycle(inst));
        }

        void RecomputeSlots()
        {
            // Aktif bildirimleri dikey diz: en yeni en altta veya en üstte?
            // Görüş alanının alt-orta'sında olduğumuz için yenilerini yukarı yığıyoruz.
            for (int i = 0; i < _active.Count; i++)
            {
                var n = _active[i];
                n.slotIndex = i;
                if (n.root == null) continue;
                var rt = (RectTransform)n.root.transform;
                // 0=en alt, üstte +height kadar
                float yOffset = i * (VRUITheme.NotificationHeight + 12f);
                rt.anchoredPosition = new Vector2(0, yOffset);
            }
        }

        NotificationInstance CreateNotification(string message, NotificationType type)
        {
            var inst = new NotificationInstance();

            var go = new GameObject("Notification", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(VRUITheme.NotificationWidth, VRUITheme.NotificationHeight);

            inst.canvasGroup = go.AddComponent<CanvasGroup>();
            inst.canvasGroup.alpha = 0f;
            inst.canvasGroup.blocksRaycasts = false;
            inst.canvasGroup.interactable = false;

            // Arka plan
            var bgGO = new GameObject("BG", typeof(RectTransform));
            bgGO.transform.SetParent(go.transform, false);
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = VRUIFactory.RoundedSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = VRUITheme.PanelBackground;
            bgImg.raycastTarget = false;
            inst.background = bgImg;

            // Sol tarafa renk şeridi (tip göstergesi)
            var barGO = new GameObject("AccentBar", typeof(RectTransform));
            barGO.transform.SetParent(go.transform, false);
            var barRT = (RectTransform)barGO.transform;
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(0, 1);
            barRT.pivot = new Vector2(0, 0.5f);
            barRT.sizeDelta = new Vector2(10, 0);
            barRT.anchoredPosition = new Vector2(8, 0);
            var barImg = barGO.AddComponent<Image>();
            barImg.sprite = VRUIFactory.RoundedSprite;
            barImg.type = Image.Type.Sliced;
            barImg.color = GetTypeColor(type);
            barImg.raycastTarget = false;
            inst.accentBar = barImg;

            // İkon (tipine göre)
            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(go.transform, false);
            var iRT = (RectTransform)iconGO.transform;
            iRT.anchorMin = new Vector2(0, 0.5f);
            iRT.anchorMax = new Vector2(0, 0.5f);
            iRT.pivot = new Vector2(0, 0.5f);
            iRT.sizeDelta = new Vector2(60, 60);
            iRT.anchoredPosition = new Vector2(32, 0);
            var iconTmp = iconGO.AddComponent<TextMeshProUGUI>();
            iconTmp.text = GetTypeIcon(type);
            iconTmp.fontSize = VRUITheme.FontHeading;
            iconTmp.color = GetTypeColor(type);
            iconTmp.alignment = TextAlignmentOptions.Center;
            iconTmp.raycastTarget = false;

            // Mesaj
            var msgGO = new GameObject("Message", typeof(RectTransform));
            msgGO.transform.SetParent(go.transform, false);
            var mRT = (RectTransform)msgGO.transform;
            mRT.anchorMin = new Vector2(0, 0);
            mRT.anchorMax = new Vector2(1, 1);
            mRT.offsetMin = new Vector2(98, 8);
            mRT.offsetMax = new Vector2(-16, -8);
            var mTmp = msgGO.AddComponent<TextMeshProUGUI>();
            mTmp.text = message;
            mTmp.fontSize = VRUITheme.FontBody;
            mTmp.color = VRUITheme.TextPrimary;
            mTmp.alignment = TextAlignmentOptions.MidlineLeft;
            mTmp.enableWordWrapping = true;
            mTmp.overflowMode = TextOverflowModes.Ellipsis;
            mTmp.raycastTarget = false;
            inst.text = mTmp;

            inst.root = go;
            return inst;
        }

        static Color GetTypeColor(NotificationType t) => t switch
        {
            NotificationType.Success => VRUITheme.Success,
            NotificationType.Warning => VRUITheme.Warning,
            NotificationType.Error   => VRUITheme.Danger,
            NotificationType.Hint    => VRUITheme.Accent,
            _ => VRUITheme.Accent
        };

        static string GetTypeIcon(NotificationType t) => t switch
        {
            NotificationType.Success => "✓",
            NotificationType.Warning => "⚠",
            NotificationType.Error   => "✕",
            NotificationType.Hint    => "💡",
            _ => "ⓘ"
        };

        IEnumerator NotificationLifecycle(NotificationInstance inst)
        {
            // Fade in
            float t = 0f;
            float fadeIn = 0.25f;
            while (t < fadeIn && inst.root != null)
            {
                t += Time.unscaledDeltaTime;
                inst.canvasGroup.alpha = t / fadeIn;
                yield return null;
            }
            if (inst.root != null) inst.canvasGroup.alpha = 1f;

            // Beklet
            while (Time.unscaledTime < inst.displayUntil && inst.root != null)
            {
                yield return null;
            }

            // Fade out + temizle
            yield return FadeAndDestroy(inst);
        }

        IEnumerator FadeAndDestroy(NotificationInstance inst)
        {
            if (inst.root == null) yield break;
            float t = 0f;
            float fadeOut = 0.35f;
            float startAlpha = inst.canvasGroup.alpha;
            while (t < fadeOut && inst.root != null)
            {
                t += Time.unscaledDeltaTime;
                if (inst.canvasGroup != null)
                    inst.canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeOut);
                yield return null;
            }
            if (inst.root != null) Destroy(inst.root);
            _active.Remove(inst);
            RecomputeSlots();
        }

        public void ClearAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].root != null) Destroy(_active[i].root);
            }
            _active.Clear();
        }
    }
}
