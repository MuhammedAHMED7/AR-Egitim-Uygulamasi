using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>
    /// VR UI için merkezi tema sabitleri. AR tarafındaki renklere paralel
    /// tutulmuştur; iki uygulamada da benzer görsel kimlik korunur.
    /// Tüm VR UI script'leri renk ve boyutları buradan okur — böylece
    /// tek bir yerden tema değiştirmek mümkündür.
    /// </summary>
    public static class VRUITheme
    {
        // ───────── Renkler ─────────
        public static readonly Color PanelBackground    = new Color(0.06f, 0.07f, 0.10f, 0.92f);
        public static readonly Color PanelBackgroundDim = new Color(0.03f, 0.04f, 0.07f, 0.85f);
        public static readonly Color PanelBorder        = new Color(1f, 1f, 1f, 0.10f);
        public static readonly Color Accent             = new Color(0.30f, 0.78f, 1.00f, 1f);
        public static readonly Color AccentHover        = new Color(0.45f, 0.86f, 1.00f, 1f);
        public static readonly Color AccentPressed      = new Color(0.20f, 0.62f, 0.92f, 1f);
        public static readonly Color Success            = new Color(0.40f, 0.85f, 0.45f, 1f);
        public static readonly Color Warning            = new Color(1.00f, 0.72f, 0.25f, 1f);
        public static readonly Color Danger             = new Color(0.95f, 0.40f, 0.42f, 1f);
        public static readonly Color TextPrimary        = new Color(1f, 1f, 1f, 0.96f);
        public static readonly Color TextSecondary      = new Color(1f, 1f, 1f, 0.65f);
        public static readonly Color TextDisabled       = new Color(1f, 1f, 1f, 0.35f);
        public static readonly Color ButtonNormal       = new Color(0.15f, 0.18f, 0.24f, 1f);
        public static readonly Color ButtonHover        = new Color(0.22f, 0.28f, 0.36f, 1f);
        public static readonly Color ButtonPressed      = new Color(0.12f, 0.15f, 0.20f, 1f);
        public static readonly Color RayHover           = new Color(0.30f, 0.78f, 1.00f, 1f);
        public static readonly Color RayDefault         = new Color(1f, 1f, 1f, 0.60f);
        public static readonly Color RaySelect          = new Color(0.40f, 0.85f, 0.45f, 1f);

        // ───────── Font boyutları (VR'da büyük olmalı) ─────────
        // VR'da metin küçükse okunmaz; minimum ~24 punto öneriyoruz.
        public const int FontTitle      = 42;
        public const int FontHeading    = 32;
        public const int FontBody       = 26;
        public const int FontSmall      = 22;
        public const int FontButton     = 28;
        public const int FontTooltip    = 20;

        // ───────── Boyutlar (metrik — Unity birimi: metre) ─────────
        // World-space Canvas'ı 1:1000 ölçekte tutuyoruz: 1 px ≈ 1 mm.
        public const float CanvasScale = 0.001f;

        public const float MainMenuWidth   = 700f;
        public const float MainMenuHeight  = 900f;
        public const float SettingsWidth   = 800f;
        public const float SettingsHeight  = 1000f;
        public const float InfoPanelWidth  = 420f;
        public const float InfoPanelHeight = 240f;
        public const float WristMenuWidth  = 260f;
        public const float WristMenuHeight = 360f;
        public const float NotificationWidth  = 520f;
        public const float NotificationHeight = 110f;
        public const float TooltipWidth  = 260f;
        public const float TooltipHeight = 80f;

        // ───────── Konumlandırma ─────────
        // Ana menü kullanıcının önünde, göz hizasında, ulaşabileceği mesafede
        public const float MainMenuDistance = 1.5f;
        public const float MainMenuHeightFromFloor = 1.4f;

        // Bildirimler kafanın hafifçe altında, biraz uzakta
        public const float NotificationDistance = 1.2f;
        public const float NotificationHeightOffset = -0.15f; // göz hizasından aşağı

        // Bilek menüsü kola yapışacak (offset)
        public static readonly Vector3 WristMenuLocalPosition = new Vector3(0f, 0.05f, -0.08f);
        public static readonly Vector3 WristMenuLocalEuler = new Vector3(-30f, 0f, 0f);

        // ───────── Haptic ─────────
        public const float HapticHoverAmplitude = 0.15f;
        public const float HapticHoverDuration  = 0.04f;
        public const float HapticClickAmplitude = 0.45f;
        public const float HapticClickDuration  = 0.08f;
        public const float HapticAlertAmplitude = 0.65f;
        public const float HapticAlertDuration  = 0.12f;

        // ───────── Animasyon süreleri ─────────
        public const float FadeDuration   = 0.25f;
        public const float SlideDuration  = 0.30f;
        public const float ToastDuration  = 3.0f;

        // ───────── Yardımcı materyal/sprite üreticileri ─────────

        /// <summary>
        /// Düz beyaz dolu kare bir Sprite üretir — UI Image arka planı için.
        /// Sahnede bir kez oluşturulup paylaşılabilir.
        /// </summary>
        public static Sprite CreateSolidSprite()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var cols = new Color[16];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Yuvarlatılmış köşeli (4x4 piksel border-radius simülasyonu) Sprite üretir.
        /// 9-slice ile büyütüldüğünde köşeleri yumuşak görünür.
        /// </summary>
        public static Sprite CreateRoundedSprite(int radius = 12)
        {
            int size = radius * 2 + 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var cols = new Color[size * size];
            Vector2 c1 = new Vector2(radius, radius);
            Vector2 c2 = new Vector2(size - radius - 1, radius);
            Vector2 c3 = new Vector2(radius, size - radius - 1);
            Vector2 c4 = new Vector2(size - radius - 1, size - radius - 1);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = true;
                    var p = new Vector2(x, y);
                    if (x < radius && y < radius) inside = Vector2.Distance(p, c1) <= radius;
                    else if (x > size - radius - 1 && y < radius) inside = Vector2.Distance(p, c2) <= radius;
                    else if (x < radius && y > size - radius - 1) inside = Vector2.Distance(p, c3) <= radius;
                    else if (x > size - radius - 1 && y > size - radius - 1) inside = Vector2.Distance(p, c4) <= radius;
                    cols[y * size + x] = inside ? Color.white : new Color(1, 1, 1, 0);
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            var sp = Sprite.Create(tex, new Rect(0, 0, size, size),
                                   new Vector2(0.5f, 0.5f), 100f, 0,
                                   SpriteMeshType.FullRect,
                                   new Vector4(radius, radius, radius, radius));
            return sp;
        }
    }
}
