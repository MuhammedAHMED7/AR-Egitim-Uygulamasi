using UnityEngine;
using TMPro;

namespace ARApp.Core.ContextualLabel
{
    /// <summary>
    /// World Space Canvas üzerindeki UI elementlerini yönetir ve Billboard (Kameraya dönme) efektini uygular.
    /// </summary>
    [DisallowMultipleComponent]
    public class ContextualLabelController : MonoBehaviour
    {
        [Header("UI Bileþen Referanslarý")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI conceptText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject visualContainer; // Kapatýp açacaðýmýz arka plan paneli (Panel_Background)

        private Transform _mainCameraTransform;

        private void Start()
        {
            // Performans için ana kamera transform referansý cache'lenir.
            if (Camera.main != null)
            {
                _mainCameraTransform = Camera.main.transform;
            }

            SetVisibility(false);
        }

        private void LateUpdate()
        {
            // UI elemanlarýnýn titremesini (jitter) önlemek için rotasyon hesaplamasý LateUpdate içinde yapýlýr.
            if (_mainCameraTransform != null && visualContainer.activeSelf)
            {
                // UI'ýn kameraya düz bakmasý için kameranýn ileri yön vektörü (forward) baz alýnýr.
                transform.LookAt(transform.position + _mainCameraTransform.forward);
            }
        }

        /// <summary>
        /// ScriptableObject verilerini UI bileþenlerine aktarýr.
        /// </summary>
        public void Initialize(ContextualLabelData data)
        {
            if (data == null) return;

            if (titleText != null) titleText.text = data.Title;
            if (conceptText != null) conceptText.text = data.Concept;
            if (descriptionText != null) descriptionText.text = data.Description;
        }

        /// <summary>
        /// Etiketin görünürlük durumunu deðiþtirir.
        /// </summary>
        public void SetVisibility(bool isVisible)
        {
            if (visualContainer != null)
            {
                visualContainer.SetActive(isVisible);
            }
        }

        public bool IsVisible => visualContainer != null && visualContainer.activeSelf;
    }
}
