using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AREducationApp
{
    /// <summary>
    /// Eğitim içeriklerinin verilerini saklayan ScriptableObject veri yapısı.
    /// Unity Editor üzerinden kolayca yeni ders içerikleri (modeller, yazılar) oluşturmak için kullanılır.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEducationalContent", menuName = "AR Education/Content Data", order = 1)]
    public class EducationalContent : ScriptableObject
    {
        [Tooltip("Ders/Model Başlığı")]
        public string Title;

        [Tooltip("Ders içeriği, açıklama metni")]
        [TextArea(3, 10)]
        public string Description;

        [Tooltip("Gösterilecek olan 3D Modelin Prefab'ı")]
        public GameObject ModelPrefab;

        [Tooltip("İsteğe Bağlı: Bu modelin ait olduğu kategori (Örn: Biyoloji, Astronomi)")]
        public string Category;
    }
}
