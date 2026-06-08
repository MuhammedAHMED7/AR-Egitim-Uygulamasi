using UnityEngine;

namespace ARApp.Core.ContextualLabel
{
    /// <summary>
    /// Ekran dokunmalarýný dinleyerek AR ortamýndaki etiket hedeflerini (Raycast ile) tetikler.
    /// </summary>
    [DisallowMultipleComponent]
    public class ARLabelTouchManager : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Start()
        {
            // Kamera referansý her karede çaðrýlýp CPU'yu yormasýn diye Start içinde bir kez cache'lenir.
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            // 1. MOBÝL CÝHAZ DOKUNMA KONTROLÜ (Runtime - Telefon Testi)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // Sadece ekrana ilk dokunulduðu aný yakalýyoruz. 
                // Bu sayede Þuca'nýn parmaðý sürükleme (Drag/Rotate) iþlemleriyle çakýþma yaþanmaz.
                if (touch.phase == TouchPhase.Began)
                {
                    ProcessRaycast(touch.position);
                }
            }

#if UNITY_EDITOR
            // 2. UNITY EDITÖR ÝÇÝ FARE KONTROLÜ (Geliþtirme aþamasýnda PC'de test kolaylýðý için)
            if (Input.GetMouseButtonDown(0))
            {
                ProcessRaycast(Input.mousePosition);
            }
#endif
        }

        /// <summary>
        /// Dokunulan ekrandan dünyaya görünmez bir ýþýn (Ray) göndererek ARContextualTarget bileþenini arar.
        /// </summary>
        /// <param name="screenPosition">Ekrandaki dokunma veya týklama koordinatý.</param>
        private void ProcessRaycast(Vector2 screenPosition)
        {
            if (_mainCamera == null) return;

            // Ekrandaki 2D piksel noktasýný 3D uzayda bir ýþýna dönüþtürür.
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);

            // Performans için Ahmet'in optimizasyon mantýðýna uygun olarak ýþýn çarpmalarýný tarýyoruz.
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Iþýnýn çarptýðý objede bizim yazdýðýmýz ARContextualTarget script'i var mý?
                ARContextualTarget target = hit.collider.GetComponent<ARContextualTarget>();

                if (target != null)
                {
                    // Varsa dokunma fonksiyonunu tetikle (Etiketi aç veya kapat)
                    target.HandleTouchInteraction();
                }
            }
        }
    }
}
