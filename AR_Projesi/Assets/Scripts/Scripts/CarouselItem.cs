using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace AREgitim.UI
{
    /// <summary>
    /// Carousel'deki tek bir model kartı.
    /// Tıklandığında ARUIManager üzerinden model seçimini tetikler.
    /// </summary>
    public class CarouselItem : MonoBehaviour, IPointerClickHandler
    {
        public ARUIManager.ModelData modelData;
        public Image background;
        public Image iconRing;
        public TMP_Text label;

        Vector3 _baseScale;
        bool _selected;

        void Awake()
        {
            _baseScale = transform.localScale;
        }

        public void Init(ARUIManager.ModelData data)
        {
            modelData = data;
            if (label != null) label.text = data.displayName;
            if (iconRing != null) iconRing.color = data.accentColor;
            SetSelected(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ARUIManager.Instance != null)
                ARUIManager.Instance.SelectModel(modelData);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            if (background != null)
            {
                background.color = selected
                    ? new Color(modelData.accentColor.r, modelData.accentColor.g, modelData.accentColor.b, 0.22f)
                    : new Color(1f, 1f, 1f, 0.06f);
            }
            transform.localScale = selected ? _baseScale * 1.08f : _baseScale;
        }
    }
}
