using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AREgitim.UI
{
    /// <summary>
    /// Yatay kaydırmalı model seçici (Thumb Zone).
    /// </summary>
    public class CarouselController : MonoBehaviour
    {
        public RectTransform itemContainer;
        public GameObject itemPrefab;

        readonly List<CarouselItem> _items = new List<CarouselItem>();

        void Start()
        {
            if (ARUIManager.Instance != null)
            {
                ARUIManager.Instance.OnModelSelected += HandleModelSelected;
                Populate(ARUIManager.Instance.availableModels);
            }
        }

        void OnDestroy()
        {
            if (ARUIManager.Instance != null)
                ARUIManager.Instance.OnModelSelected -= HandleModelSelected;
        }

        public void Populate(List<ARUIManager.ModelData> models)
        {
            foreach (var existing in _items)
                if (existing != null) Destroy(existing.gameObject);
            _items.Clear();

            if (itemContainer == null || itemPrefab == null) return;

            foreach (var m in models)
            {
                var go = Instantiate(itemPrefab, itemContainer);
                var item = go.GetComponent<CarouselItem>();
                if (item != null)
                {
                    item.Init(m);
                    _items.Add(item);
                }
            }
        }

        void HandleModelSelected(ARUIManager.ModelData selected)
        {
            foreach (var item in _items)
            {
                if (item == null) continue;
                item.SetSelected(item.modelData != null && item.modelData.id == selected.id);
            }
        }
    }
}
