## 1. Hafta (5-12 Mart)

 ## 👤 **Shuja Ahmad Tariq:** Proje Analizi ve Kapsam Tanımı

### 1.1 Proje Özeti
**AR-Edu Uygulaması**, geleneksel statik ders kitapları ile sürükleyici dijital öğrenme arasındaki boşluğu doldurmak için tasarlanmış yeni nesil bir pedagojik araçtır. **Artırılmış Gerçeklik (AR)** teknolojisini kullanan uygulama, 2D diyagramları etkileşimli 3D modellere dönüştürerek öğrencilerin soyut kavramları gerçek zamanlı olarak görselleştirmelerini sağlar.

### 1.2 Genel Hedefler
* **Bilişsel Gelişim:** Soyut konuların (örneğin moleküler yapılar, gezegen yörüngeleri) mekansal görselleştirmesini geliştirmek.
* **Etkileşim:** Öğrenme sürecini oyunlaştırarak öğrenci motivasyonunu artırmak.
* **Erişilebilirlik:** Pahalı fiziksel ekipmanlara ihtiyaç duymadan yüksek kaliteli laboratuvar benzeri deneyimler sunmak.
* **Kalıcılık:** Uzun süreli bellek tutulumunu artırmak için "yaparak öğrenme" ilkesinden yararlanmak.

### 1.3 Proje Kapsamı
* **✅ Kapsam Dahilinde (Faz 1):**
    * **Görüntü İzleme (Image Tracking):** Standart müfredat kitaplarındaki belirli "tetikleyici" görsellerin tanınması.
    * **Etkileşimli 3D Modeller:** Kullanıcıların 3D varlıkları döndürme, yakınlaştırma ve etkileşime girme yeteneği.
    * **Çoklu Platform Desteği:** Android ve iOS mobil cihazlar için ilk sürüm.
    * **İçerik Kütüphanesi:** Üç temel ders için başlangıç desteği: **Biyoloji, Fizik ve Tarih.**
    * **Çevrimdışı Mod:** İndirilen AR içeriklerini internet bağlantısı olmadan görüntüleme yeteneği.
* **❌ Kapsam Dışında:**
    * **VR Entegrasyonu:** Sanal Gerçeklik gözlükleri desteği V1.0 için planlanmamıştır.
    * **Çok Oyunculu İşbirliği:** Birden fazla öğrenci için gerçek zamanlı paylaşımlı AR alanları.
    * **İçerik Oluşturma Paneli:** Öğretmenlerin kendi 3D modellerini oluşturabilecekleri bir araç.

---

### 👥 2. Paydaş Analizi

| Paydaş | Beklentiler ve İhtiyaçlar |
| :--- | :--- |
| **Öğrenciler** | Kullanıcı dostu arayüz, yüksek performanslı görseller ve eğlenceli etkileşimler. |
| **Öğretmenler** | Resmi müfredatla uyum, ders planlarına kolay entegrasyon ve minimum kurulum süresi. |
| **Veliler** | Eğitici değer, veri gizliliği ve orta segment akıllı telefonlarla uyumluluk. |
| **Geliştiriciler** | Net API dokümantasyonu ve mobil performans için optimize edilmiş 3D varlıklar. |

---

### 🛠️ 3. Teknik Mimari

### 3.1 Üst Düzey Sistem Mimarisi
Uygulama, dosya boyutunu küçük tutmak ve içeriği dinamik yönetmek için **Modüler İstemci-Sunucu Mimarisini** takip eder.



* **Frontend (Önyüz):** Unity 3D Motoru.
* **AR Katmanı:** **AR Foundation** (ARKit ve ARCore için platformlar arası sarmalayıcı).
* **Bulut Entegrasyonu:** Kullanıcı kimlik doğrulaması ve model meta verileri (Addressable Assets) için Firebase/AWS.

### 3.2 3D Varlık İş Akışı (Asset Pipeline)
Mobil cihazlarda akıcı performans sağlamak için "Low-Poly" (Düşük Poligon) optimizasyon iş akışı kullanılır.

* **Dosya Formatları:**
    * **Birincil:** `.GLB / .GLTF` (AR için endüstri standardı).
    * **Üretim:** `.FBX` (Blender/Maya üzerinden).
* **Optimizasyon Standartları:**
    * **Poligon Sayısı:** Model başına maksimum 15.000 üçgen.
    * **Doku Boyutu:** Atlaslar halinde paketlenmiş 1024x1024px.
    * **Shaderlar:** Mobil cihazlar için optimize edilmiş "Lit" veya "Unlit" shaderlar.

### 3.3 Temel Teknoloji Yığını
* **Motor:** Unity 6 (2023 LTS veya daha yenisi).
* **Programlama:** C# (.NET Standard 2.1).
* **Versiyon Kontrolü:** Git + Git LFS (Büyük Dosya Desteği).

---

### 📊 4. Beklenen Sonuçlar ve Başarı Kriterleri
* **Nicel:** AR destekli derslerde öğrenci başarı puanlarında %20 artış.
* **Nitel:** Pilot gruptaki öğretmenlerin %80'inden "kullanım kolaylığı" konusunda olumlu geri bildirim.
* **Performans:** Uygulama açılışından itibaren 10 saniyenin altında "İlk AR Deneyimi"; stabil 30+ FPS.

---

### 📂 5. Proje Yapısı (Depo Haritası)

          # 3D Modeller, Dokular ve Sesler
          # Kapsam, Teknik Şartname ve Araştırmalar
          # Unity Proje Dosyaları
          # C# Mantığı (Takip, UI, Etkileşimler)
          # Tekrar Kullanılabilir AR Nesneleri
          # Ana Menü, Biyoloji Laboratuvarı, Fizik Laboratuvarı
          # Takip kararlılığı için birim testler


   ## 👤 **Muhammed (Proje Yöneticisi):** Artırılmış Gerçeklik Teknolojileri Araştırma Raporu
  > **Araştırmayı Yapan:** Muhammed AHMED | **Durum:** Tamamlandı

  Proje kapsamında kullanılacak Artırılmış Gerçeklik (AR) altyapısı için sektördeki çeşitli SDK'lar (Vuforia, EasyAR, AR Foundation) teknik açıdan incelenmiştir. Yapılan analizler ve Gereksinim Toplama ve Belgeleme Dokümanı doğrultusunda, projemizde Unity AR Foundation ve Android platformu için Google ARCore eklentisinin kullanılmasına karar verilmiştir.

  **Neden AR Foundation ve ARCore Seçildi?**
  * **Çapraz Platform (Cross-Platform) Desteği:** AR Foundation, projenin temel yapısını bozmadan hem Android (ARCore) hem de opsiyonel olarak iOS (ARKit) cihazlar için geliştirme yapmaya olanak tanır.
  * **Unity Entegrasyonu:** Projede kullanılacak olan Unity 2022 LTS sürümünün paket yöneticisi (Package Manager) üzerinden resmi ve stabil şekilde projeye eklenebilir. Ek bir yazılım kurulumu gerektirmez.
  * **Performans ve Maliyet:** Projemiz eğitim amaçlı bir prototip olduğundan ücretli sistemlerin zorunlu logo (filigran) kısıtlamalarından kaçınmak için tamamen ücretsiz olan bu mimari tercih edilmiştir. Ayrıca ARCore mobil cihazlarda minimum 30 FPS performansı sağlayacak şekilde optimize edilmiştir.
  * **Görsel İşaretleyici (Marker) Algılama:** Ders kitaplarındaki görsellerin kamera ile taranması ve üzerine 3 boyutlu eğitim modellerinin yerleştirilmesi işlemi, AR Foundation'ın sunduğu Image Tracking (Görsel İzleme) teknolojisi sayesinde yüksek doğrulukla gerçekleştirilebilmektedir.



## 👤 **Ahmet Yaman:**  Sanal Gerçeklik Teknolojileri Araştırma Raporu
🧭 1. Raporun Amacı
Bu rapor, proje bağlamında kullanılabilecek başlıca artırılmış gerçeklik (AR) ve sanal gerçeklik (VR) teknolojilerini değerlendirmek, güçlü ve zayıf yönlerini analiz etmek ve en uygun teknolojik yol haritasını ortaya koymak için hazırlanmıştır.
________________________________________
🎯 2. Değerlendirilen Teknolojiler / Araçlar
Teknoloji	Kategori	Platform Desteği	Kullanım Alanı
Unity + AR Foundation	AR/VR Geliştirme	iOS, Android	AR/VR Hibrit
Unreal Engine AR/VR	AR/VR Geliştirme	iOS, Android	Yüksek Görsel Kalite
ARKit	AR SDK	iOS	iPhone/iPad AR
ARCore	AR SDK	Android	Android AR
Vuforia	AR SDK	iOS, Android	Görsel Takip Odaklı AR
Microsoft Mixed Reality	MR Platform	HoloLens, Windows	Gelişmiş MR
Google VR / Cardboard	VR SDK	iOS, Android	Basit VR Deneyimleri
Oculus SDK (Meta)	VR SDK	Quest/PC	VR Odaklı
OpenXR	Standard API	Çoklu Cihaz	Evrensel VR/AR
________________________________________
🔍 3. AR / VR Teknolojilerinin Detaylı İncelemesi
🎮 3.1 Unity + AR Foundation
Kategori: AR/VR Geliştirme Framework’ü
Öne Çıkan Özellikler:
•	Hem ARKit hem ARCore’u tek bir API ile kullanabilme
•	Unity Editor ile zengin sahne tasarımı
•	3D model entegrasyonu ve animasyon desteği
•	Mobil AR & VR deneyimlerinin aynı projede yönetimi
Artıları:
•	Kod tabanı tek — iOS & Android uyumlu
•	Zengin topluluk & dökümantasyon
•	AR ve VR’i birlikte destekleyebilme
•	Mobil performans optimizasyonu kolay
Eksileri:
•	Büyük projelerde bellek yönetimi dikkat gerektirir
•	AR Foundation’un ileri seviye takip kontrolü sınırlı olabiliyor
Değerlendirme: Proje için en esnek ve en uygun çözüm.
________________________________________
🚀 3.2 Unreal Engine (AR/VR)
Kategori: Geliştirme Motoru
Öne Çıkan Özellikler:
•	Yüksek görsel kalite, gerçekçi render
•	Blueprint görsel programlama
•	Advanced materyal ve ışıklandırma desteği
Artıları:
•	Photoreal görseller
•	Blueprint ile hızlı prototipleme
Eksileri:
•	Mobil AR projelerinde performans zorlayabilir
•	Geniş öğrenme eğrisi
Değerlendirme: Görsel kalite öncelikli AR/VR projeleri için uygun.
________________________________________
🧠 3.3 ARKit (Apple)
Kategori: iOS AR SDK
Öne Çıkan Özellikler:
•	Yüz takibi, yüz ifadeleri
•	Işık haritalama
•	Gerçek dünya fonksiyon takibi
Artıları:
•	iOS cihazlarda optimize AR
•	Yüksek takip doğruluğu
Eksileri:
•	Sadece iOS
________________________________________
🤖 3.4 ARCore (Google)
Kategori: Android AR SDK
Öne Çıkan Özellikler:
•	Hareket takibi
•	Çevresel anlayış
•	Yerleştirme yüzey tespiti
Artıları:
•	Android üzerinde güçlü AR
•	Geniş cihaz desteği
Eksileri:
•	iOS üzerinde kullanılamaz
________________________________________
🔎 3.5 Vuforia
Kategori: AR SDK
Öne Çıkan Özellikler:
•	Görsel hedef tanıma
•	Kitap, nesne, resim takibi
Artıları:
•	Görsel nesne tanıma çok güçlü
•	Fiziksel materyalleri takip etmede avantajlı
Eksileri:
•	Mobil AR deneyimi Unity AR Foundation kadar esnek değil
________________________________________
🕶 3.6 Microsoft Mixed Reality
Kategori: MR Platform
Öne Çıkan Özellikler:
•	HoloLens desteği
•	Gerçek dünya + hologram sentezi
Artıları:
•	Endüstriyel MR projelerinde güçlü
Eksileri:
•	Sadece HoloLens
________________________________________
🌐 3.7 OpenXR
Kategori: Evrensel AR/VR API Standardı
Artıları:
•	Birden çok AR/VR cihazı tek çatı altında
Eksileri:
•	Geliştirme karmaşıklığı yaratabilir
________________________________________
📊 4. Teknik Karşılaştırma Tablosu

| Kriter | Unity + AR Foundation | ARKit | ARCore | Vuforia | Unreal |
|------|----------------------|------|-------|--------|--------|
| Mobil Uyum | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| Platform Desteği | iOS + Android | iOS | Android | iOS + Android | iOS + Android |
| Takip / İzleme | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| 3D Model Entegrasyonu | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| Performans | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| Öğrenme Eğrisi | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ |
| Topluluk / Dökümantasyon | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
________________________________________
📌 5. Proje İçin Önerilen Teknoloji Yığını
Ana Teknoloji
•	Unity + AR Foundation
o	Hem iOS hem Android’de tek çözüm
o	ARKit & ARCore entegrasyonu tek çatı altında
o	3D model ve interaktif sahneler için güçlü
Ek SDK / Araçlar
Kullanım Amacı	Önerilen Teknoloji
Nesne / Görsel Takibi	Vuforia
Yüz Takibi (iOS)	ARKit Face Tracking
3D Model Oluşturma	Blender / Maya
UI Arayüz Geliştirme	Unity UI Toolkit
Performans Optimizasyonu	Unity Profiler
________________________________________
🎓 6. Teknoloji Avantajları & Dezavantajları
Unity + AR Foundation
•	Avantaj: Tek projede çapraz platform, zengin eklenti desteği, öğrenmesi kolay
•	Dezavantaj: İleri seviye izleme bazı özel SDK’larda daha güçlü olabilir
ARKit & ARCore
•	Avantaj: Yerel AR fonksiyonları, yüksek takip doğruluğu
•	Dezavantaj: Tek platforma özel
Vuforia
•	Avantaj: Görsel hedef takibi mükemmel
•	Dezavantaj: Genel AR Foundation kadar esnek değil
Unreal
•	Avantaj: Yüksek görsel kalite
•	Dezavantaj: Mobil için ağır
________________________________________
📌 7. Özet Değerlendirme
Kullanım Durumu	En Uygun Teknoloji
Yüksek Performanslı Mobil AR	Unity + AR Foundation
Gelişmiş Görsel Takip	Vuforia
iOS Esaslı AR	ARKit
Android Esaslı AR	ARCore
VR Eğitim Deneyimleri	Oculus / OpenXR + Unity
________________________________________
🏁 8. Sonuç ve Tavsiyeler
•	Projenin ana platformu olarak Unity + AR Foundation kullanılmalı.
•	Görsel kitap öğeleri için Vuforia entegre edilebilir.
•	iOS göz takibi veya özel yüz ifadeleri için ARKit Face Tracking kullanılabilir.
•	3D nesneler için Blender ile özelleştirilmiş modeller üretilebilir.



## 👤 **Burçin Ayyıldız:** Geliştirme Ortamı Kurulumu

Projenin geliştirme ortamı Unity 2022.3 LTS kullanılarak oluşturulmuştur. Kod geliştirme için Visual Studio 2022, sürüm kontrolü için Git ve Github kullanılmıştır. Android cihazlarda test yapabilmek için Android SDK, NDK ve OpenJDK paketleri kurulmuştur. Artırılmış gerçeklik desteği için AR Foundation ve ARCore XR Plugin projeye eklenmiştir.

## 👤 **Perihan Çelikoğlu:** Gereksinim Toplama ve Belgeleme Dokümanı

  
* 1. Kullanılacak Teknolojiler
Bu proje üniversite 1. sınıf yazılım öğrencileri için uygun teknolojiler kullanılarak geliştirilecektir.

>1.1 Oyun Motoru 
- Unity 2022 LTS 
Sebebi: 
AR geliştirme için yaygın kullanılır 
Öğrenmesi kolaydır 
Mobil uygulama geliştirmeye uygundur 

>1.2 Programlama Dili 
- C# 
Unity içinde kullanılan ana programlama dilidir. 
Kullanım amacı: 
Uygulama mantığını oluşturmak 
Kullanıcı etkileşimlerini yönetmek
AR sahnelerini kontrol etmek 

>1.3 Artırılmış Gerçeklik Teknolojisi
-AR Foundation 
Unity'nin resmi AR geliştirme kütüphanesidir. 
Avantajları: 
-Hem Android hem iOS destekler 
-ARCore ve ARKit ile uyumludur
>>Android için 
-ARCore 
-Google tarafından geliştirilmiştir. 
>>iOS için 
-ARKit 
-Apple tarafından geliştirilmiştir.

>1.4 3D Modelleme
-Blender
Kullanım amacı: 
Eğitimde kullanılacak 3D modelleri oluşturmak 
Modelleri optimize etmek

>1.5 Mobil Platform
Proje aşağıdaki platformlar için geliştirilecektir: 
Android (öncelikli) 
iOS (opsiyonel) 

* 2. Sistem Mimarisi
Sistem aşağıdaki bileşenlerden oluşacaktır: 
> Mobil Uygulama 
Kullanıcının uygulamayı çalıştırdığı ana sistemdir.

>AR Kamera Sistemi 
Telefon kamerası ile gerçek dünyayı algılar ve sahneye 3D model yerleştirir. 

>3D Model Sistemi 
Eğitim içeriklerinin üç boyutlu modellerini içerir. 

>Kullanıcı Arayüzü 
Kullanıcı ile uygulama arasındaki etkileşimi sağlar.

* 3. Fonksiyonel Gereksinimler
Fonksiyonel gereksinimler sistemin ne yapması gerektiğini tanımlar.
>FR1 
Kullanıcı uygulamayı mobil cihazında başlatabilmelidir. 
>FR2 
Uygulama mobil cihazın kamerasına erişebilmelidir. 
>FR3 
Kullanıcı bir görseli veya işaretleyiciyi kameraya gösterdiğinde sistem bunu algılayabilmelidir. 
>FR4 
Algılanan görsel üzerine ilgili 3D model görüntülenebilmelidir. 
>FR5 
Kullanıcı 3D modeli döndürebilmelidir. 
>FR6 
Kullanıcı 3D modeli yakınlaştırıp uzaklaştırabilmelidir. 
>FR7 
Kullanıcı model üzerinde bilgi etiketlerini görüntüleyebilmelidir.
>FR8 
Kullanıcı uygulama menüsüne erişebilmelidir. 
>FR9 
Uygulama farklı eğitim modellerini gösterebilmelidir.

* 4. Fonksiyonel Olmayan Gereksinimler
>Performans 
Uygulama mobil cihazlarda akıcı çalışmalıdır. 
Minimum: 30 FPS 
>Kullanılabilirlik 
Arayüz basit ve anlaşılır olmalıdır. 
>Uyumluluk 
Android 10 ve üzeri cihazlarda çalışmalıdır. 
>Güvenlik 
Uygulama kullanıcıdan sadece kamera erişim izni isteyecektir.

* 5. Kullanıcı Hikayeleri (User Stories)
Kullanıcı Hikayesi 1 
-Bir öğrenci olarak ders kitabındaki bir görseli taramak istiyorum, çünkü konuyu 3 boyutlu olarak incelemek istiyorum.

Kullanıcı Hikayesi 2 
-Bir öğrenci olarak 3D modeli döndürmek istiyorum, çünkü modelin farklı açılardan nasıl göründüğünü incelemek istiyorum. 

-Kullanıcı Hikayesi 3 
Bir öğrenci olarak model üzerinde bilgi etiketlerini görmek istiyorum, çünkü konuyu daha iyi anlamak istiyorum. 

Kullanıcı Hikayesi 4 
-Bir öğrenci olarak uygulamayı kolay bir şekilde kullanmak istiyorum, çünkü hızlı bir şekilde öğrenmek istiyorum. 

* 6. Test Gereksinimleri 
Projenin doğruluğunu kontrol etmek için aşağıdaki testler yapılacaktır. 
Test 1 
Kamera açılıyor mu? 

Test 2 
Marker algılanıyor mu? 

Test 3 
3D model doğru şekilde görüntüleniyor mu? 

Test 4 
Model döndürülebiliyor mu? 

Test 5 
Uygulama çökmeden çalışıyor mu? 

* 7. Teslim Edilecek Çıktılar 
>Proje sonunda aşağıdaki çıktılar teslim edilecektir: 
>Artırılmış gerçeklik sahneleri 
>Etkileşimli 3D modeller 
>Mobil uygulama arayüzü 
>Test senaryoları 
>Kullanıcı kılavuzu

* 8. Proje Ekibi 
Proje ekibi aşağıdaki rollere ayrılacaktır: 
>AR Geliştirici 
Unity ve AR sistemlerini geliştirir. 
>3D Model Tasarımcısı 
Blender kullanarak 3D modeller oluşturur. 
>Arayüz Geliştirici 
Mobil uygulama arayüzünü tasarlar. 
>Proje Analisti 
Gereksinim dokümanını ve planlamayı hazırlar. 
