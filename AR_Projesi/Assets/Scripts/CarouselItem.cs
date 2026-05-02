using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Her carousel kartına bu scripti ekle.
/// Prefab yapısı:
///   CarouselItem (Bu script + Button + Image)
///     ├── Icon     (Image)
///     └── Label    (TextMeshProUGUI)
/// </summary>
public class CarouselItem : MonoBehaviour
{
    [Header("UI Referansları")]
    public Image           iconImage;
    public TextMeshProUGUI labelText;
    public Image           backgroundImage;

    [Header("Renkler")]
    public Color normalBg      = new Color(1f, 1f, 1f, 0.06f);
    public Color selectedBg    = new Color(0f, 0.82f, 0.47f, 0.20f);
    public Color normalBorder  = new Color(1f, 1f, 1f, 0.10f);
    public Color selectedBorder= new Color(0f, 0.82f, 0.47f, 1f);

    // Dışarıdan atanacak tıklama olayı
    public Action OnClicked;

    private Button  _btn;
    private Outline _outline;

    void Awake()
    {
        _btn     = GetComponent<Button>();
        _outline = GetComponent<Outline>();

        if (_btn)
            _btn.onClick.AddListener(() => OnClicked?.Invoke());
    }

    /// <summary>
    /// Carousel öğesini doldurur ve seçili durumunu ayarlar.
    /// </summary>
    public void Setup(Sprite icon, string name, bool isSelected)
    {
        if (iconImage)  iconImage.sprite = icon;
        if (labelText)  labelText.text   = name;
        SetSelected(isSelected);
    }

    public void SetSelected(bool selected)
    {
        // Arka plan rengi
        if (backgroundImage)
            backgroundImage.color = selected ? selectedBg : normalBg;

        // Kenarlık rengi (Outline bileşeni varsa)
        if (_outline)
            _outline.effectColor = selected ? selectedBorder : normalBorder;

        // Boyut — LeanTween yok, direkt scale
        float target = selected ? 1.08f : 1f;
        transform.localScale = Vector3.one * target;
    }
}
