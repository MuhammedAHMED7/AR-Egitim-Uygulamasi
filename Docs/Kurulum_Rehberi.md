# Unity ve AR Foundation Kurulum Rehberi

Bu proje, mobil cihazlarda artırılmış gerçeklik deneyimi sunmak için **Unity** ve **AR Foundation** üzerine kuruludur. Aşağıdaki adımları takip ederek projenizi Unity içerisinde hazırlayabilirsiniz.

## Adım 1: Unity Hub ve Sürüm Kurulumu
1. **Unity Hub** uygulamasını indirin ve bilgisayarınıza kurun.
2. `Installs` sekmesinden **Unity 2022.3 LTS** veya **2023** sürümlerinden birini yükleyin.
3. Yükleme sırasında **Android Build Support** (ve gereksinim duyuyorsanız **iOS Build Support**) modüllerini işaretlemeyi unutmayın.

## Adım 2: Yeni Proje Oluşturma
1. Unity Hub'da **Projects** sekmesine gidin.
2. **New Project** butonuna tıklayın.
3. Şablon (Template) olarak **3D Core** veya varsa **AR** şablonunu seçin.
4. Projenin adını `AREducationApp` olarak belirleyin ve projeyi oluşturun.

## Adım 3: AR Foundation Paketlerinin Kurulumu
1. Proje açıldıktan sonra üst menüden **Window > Package Manager** yolunu izleyin.
2. Paket yöneticisinde sol üst köşedeki `Packages` açılır menüsünden **Unity Registry** seçeneğini seçin.
3. Sağ üstteki arama çubuğuna şu paket isimlerini yazıp **Install** (Kur) butonuna basın:
   - `AR Foundation`
   - `Apple ARKit XR Plugin` (iOS cihazlar için)
   - `Google ARCore XR Plugin` (Android cihazlar için)

## Adım 4: Proje Klasör Yapısı
Proje içerisinde oluşturduğum C# scriptlerini Unity'deki `Assets/Scripts` klasörünün içine sürükleyip bırakmanız gerekmektedir. Önerilen klasör yapısı:
- `Assets/`
  - `Scripts/` (Benim ürettiğim C# dosyaları buraya gelecek)
  - `Models/` (Blender/Maya'dan çıkaracağınız .fbx uzantılı dosyalar buraya)
  - `Prefabs/` (Oluşturduğumuz 3D modellerin Unity Prefab hali)
  - `Scenes/` (Sahne dosyaları)

## Adım 5: XR Plugin Management Ayarları
1. **Edit > Project Settings > XR Plugin Management** sekmesine gidin.
2. Android sekmesinde (Robot simgesi) **ARCore** seçeneğini işaretleyin.
3. iOS sekmesinde (Apple simgesi) **ARKit** seçeneğini işaretleyin.

Bu kurulum tamamlandıktan sonra, ürettiğimiz kodlar Unity içerisinde çalışmaya hazır hale gelecektir!
