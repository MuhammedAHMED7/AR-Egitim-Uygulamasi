## 1. Hafta (5-12 Mart)

<details>
<summary>👉 👤 Shuja Ahmad Tariq: Proje Analizi ve Kapsam Tanımı (Açmak için tıklayın)</summary>

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

</details>


<details>
<summary>👉 👤 Muhammed AHMED (Proje Yöneticisi): Artırılmış Gerçeklik Teknolojileri Araştırma Raporu (Açmak için tıklayın)</summary>

> **Araştırmayı Yapan:** Muhammed AHMED | **Durum:** Tamamlandı

 Proje kapsamında kullanılacak Artırılmış Gerçeklik (AR) altyapısı için sektördeki çeşitli SDK'lar (Vuforia, EasyAR, AR Foundation) teknik açıdan incelenmiştir. Yapılan analizler ve Gereksinim Toplama ve Belgeleme Dokümanı doğrultusunda, projemizde Unity AR Foundation ve Android platformu için Google ARCore eklentisinin kullanılmasına karar verilmiştir.

 **Neden AR Foundation ve ARCore Seçildi?**
 * **Çapraz Platform (Cross-Platform) Desteği:** AR Foundation, projenin temel yapısını bozmadan hem Android (ARCore) hem de opsiyonel olarak iOS (ARKit) cihazlar için geliştirme yapmaya olanak tanır.
 * **Unity Entegrasyonu:** Projede kullanılacak olan Unity 2022 LTS sürümünün paket yöneticisi (Package Manager) üzerinden resmi ve stabil şekilde projeye eklenebilir. Ek bir yazılım kurulumu gerektirmez.
 * **Performans ve Maliyet:** Projemiz eğitim amaçlı bir prototip olduğundan ücretli sistemlerin zorunlu logo (filigran) kısıtlamalarından kaçınmak için tamamen ücretsiz olan bu mimari tercih edilmiştir. Ayrıca ARCore mobil cihazlarda minimum 30 FPS performansı sağlayacak şekilde optimize edilmiştir.
 * **Görsel İşaretleyici (Marker) Algılama:** Ders kitaplarındaki görsellerin kamera ile taranması ve üzerine 3 boyutlu eğitim modellerinin yerleştirilmesi işlemi, AR Foundation'ın sunduğu Image Tracking (Görsel İzleme) teknolojisi sayesinde yüksek doğrulukla gerçekleştirilebilmektedir.

</details>


<details>
<summary>👉 👤 Ahmet Yaman: Sanal Gerçeklik Teknolojileri Araştırma Raporu (Açmak için tıklayın)</summary>

🧭 1. Raporun Amacı
Bu rapor, proje bağlamında kullanılabilecek başlıca artırılmış gerçeklik (AR) ve sanal gerçeklik (VR) teknolojilerini değerlendirmek, güçlü ve zayıf yönlerini analiz etmek ve en uygun teknolojik yol haritasını ortaya koymak için hazırlanmıştır.
________________________________________
🎯 2. Değerlendirilen Teknolojiler / Araçlar

| Teknoloji | Kategori | Platform Desteği | Kullanım Alanı |
| :--- | :--- | :--- | :--- |
| Unity + AR Foundation | AR/VR Geliştirme | iOS, Android | AR/VR Hibrit |
| Unreal Engine AR/VR | AR/VR Geliştirme | iOS, Android | Yüksek Görsel Kalite |
| ARKit | AR SDK | iOS | iPhone/iPad AR |
| ARCore | AR SDK | Android | Android AR |
| Vuforia | AR SDK | iOS, Android | Görsel Takip Odaklı AR |
| Microsoft Mixed Reality | MR Platform | HoloLens, Windows | Gelişmiş MR |
| Google VR / Cardboard | VR SDK | iOS, Android | Basit VR Deneyimleri |
| Oculus SDK (Meta) | VR SDK | Quest/PC | VR Odaklı |
| OpenXR | Standard API | Çoklu Cihaz | Evrensel VR/AR |

________________________________________
🔍 3. AR / VR Teknolojilerinin Detaylı İncelemesi

🎮 3.1 Unity + AR Foundation
Kategori: AR/VR Geliştirme Framework’ü
Öne Çıkan Özellikler:
* Hem ARKit hem ARCore’u tek bir API ile kullanabilme
* Unity Editor ile zengin sahne tasarımı
* 3D model entegrasyonu ve animasyon desteği
* Mobil AR & VR deneyimlerinin aynı projede yönetimi
Artıları:
* Kod tabanı tek — iOS & Android uyumlu
* Zengin topluluk & dökümantasyon
* AR ve VR’i birlikte destekleyebilme
* Mobil performans optimizasyonu kolay
Eksileri:
* Büyük projelerde bellek yönetimi dikkat gerektirir
* AR Foundation’un ileri seviye takip kontrolü sınırlı olabiliyor
Değerlendirme: Proje için en esnek ve en uygun çözüm.

🚀 3.2 Unreal Engine (AR/VR)
Kategori: Geliştirme Motoru
Öne Çıkan Özellikler:
* Yüksek görsel kalite, gerçekçi render
* Blueprint görsel programlama
* Advanced materyal ve ışıklandırma desteği
Artıları:
* Photoreal görseller
* Blueprint ile hızlı prototipleme
Eksileri:
* Mobil AR projelerinde performans zorlayabilir
* Geniş öğrenme eğrisi
Değerlendirme: Görsel kalite öncelikli AR/VR projeleri için uygun.

🧠 3.3 ARKit (Apple)
Kategori: iOS AR SDK
Öne Çıkan Özellikler:
* Yüz takibi, yüz ifadeleri
* Işık haritalama
* Gerçek dünya fonksiyon takibi
Artıları:
* iOS cihazlarda optimize
* Yüksek takip doğruluğu
Eksileri:
* Sadece iOS

🤖 3.4 ARCore (Google)
Kategori: Android AR SDK
Öne Çıkan Özellikler:
* Hareket takibi
* Çevresel anlayış
* Yerleştirme yüzey tespiti
Artıları:
* Android üzerinde güçlü AR
* Geniş cihaz desteği
Eksileri:
* iOS üzerinde kullanılamaz

🔎 3.5 Vuforia
Kategori: AR SDK
Öne Çıkan Özellikler:
* Görsel hedef tanıma
* Kitap, nesne, resim takibi
Artıları:
* Görsel nesne tanıma çok güçlü
* Fiziksel materyalleri takip etmede avantajlı
Eksileri:
* Mobil AR deneyimi Unity AR Foundation kadar esnek değil

🕶 3.6 Microsoft Mixed Reality
Kategori: MR Platform
Öne Çıkan Özellikler:
* HoloLens desteği
* Gerçek dünya + hologram sentezi
Artıları:
* Endüstriyel MR projelerinde güçlü
Eksileri:
* Sadece HoloLens

🌐 3.7 OpenXR
Kategori: Evrensel AR/VR API Standardı
Artıları:
* Birden çok AR/VR cihazı tek çatı altında
Eksileri:
* Geliştirme karmaşıklığı yaratabilir

________________________________________
📊 4. Teknik Karşılaştırma Tablosu

| Kriter | Unity + AR Foundation | ARKit | ARCore | Vuforia | Unreal |
| :--- | :--- | :--- | :--- | :--- | :--- |
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
* Unity + AR Foundation
    * Hem iOS hem Android’de tek çözüm
    * ARKit & ARCore entegrasyonu tek çatı altında
    * 3D model ve interaktif sahneler için güçlü

Ek SDK / Araçlar

| Kullanım Amacı | Önerilen Teknoloji |
| :--- | :--- |
| Nesne / Görsel Takibi | Vuforia |
| Yüz Takibi (iOS) | ARKit Face Tracking |
| 3D Model Oluşturma | Blender / Maya |
| UI Arayüz Geliştirme | Unity UI Toolkit |
| Performans Optimizasyonu | Unity Profiler |

________________________________________
🎓 6. Teknoloji Avantajları & Dezavantajları
Unity + AR Foundation
* Avantaj: Tek projede çapraz platform, zengin eklenti desteği, öğrenmesi kolay
* Dezavantaj: İleri seviye izleme bazı özel SDK’larda daha güçlü olabilir
ARKit & ARCore
* Avantaj: Yerel AR fonksiyonları, yüksek takip doğruluğu
* Dezavantaj: Tek platforma özel
Vuforia
* Avantaj: Görsel hedef takibi mükemmel
* Dezavantaj: Genel AR Foundation kadar esnek değil
Unreal
* Avantaj: Yüksek görsel kalite
* Dezavantaj: Mobil için ağır

________________________________________
📌 7. Özet Değerlendirme

| Kullanım Durumu | En Uygun Teknoloji |
| :--- | :--- |
| Yüksek Performanslı Mobil AR | Unity + AR Foundation |
| Gelişmiş Görsel Takip | Vuforia |
| iOS Esaslı AR | ARKit |
| Android Esaslı AR | ARCore |
| VR Eğitim Deneyimleri | Oculus / OpenXR + Unity |

________________________________________
🏁 8. Sonuç ve Tavsiyeler
* Projenin ana platformu olarak Unity + AR Foundation kullanılmalı.
* Görsel kitap öğeleri için Vuforia entegre edilebilir.
* iOS göz takibi veya özel yüz ifadeleri için ARKit Face Tracking kullanılabilir.
* 3D nesneler için Blender ile özelleştirilmiş modeller üretilebilir.

</details>


<details>
<summary>👉 👤 Burçin Ayyıldız: Geliştirme Ortamı Kurulumu (Açmak için tıklayın)</summary>

Projenin geliştirme ortamı Unity 2022.3 LTS kullanılarak oluşturulmuştur. Kod geliştirme için Visual Studio 2022, sürüm kontrolü için Git ve Github kullanılmıştır. Android cihazlarda test yapabilmek için Android SDK, NDK ve OpenJDK paketleri kurulmuştur. Artırılmış gerçeklik desteği için AR Foundation ve ARCore XR Plugin projeye eklenmiştir.

</details>


<details>
<summary>👉 👤 Perihan Çelikoğlu: Gereksinim Toplama ve Belgeleme Dokümanı (Açmak için tıklayın)</summary>

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

</details>

---

## 2. Hafta (3-10 Nisan)

<details>
<summary>👉 👤 Perihan Çelikoğlu: UI/UX Wireframe ve Arayüz Tasarımı (Açmak için tıklayın)</summary>

Uygulamanın kullanıcı arayüzü ve kullanıcı deneyimi wireframe'lerinin çıkarıldı.

</details>

---

## 3. Hafta (13-20 Nisan)

<details>
<summary>👉 👤 Perihan Çelikoğlu: VR Ortamında Navigasyon ve Temel Etkileşim Mekaniklerinin Uygulanması (Açmak için tıklayın)</summary>

Navigasyon ve Temel Etkileşim

Bu görev kapsamında Unity ortamında temel kullanıcı hareketi ve etkileşim sistemi geliştirilmiştir.

Yapılanlar:
* Kullanıcının WASD tuşları ile sahnede hareket etmesi sağlandı
* Mouse ile sağa-sola bakış (kamera kontrolü) eklendi
* Sahnedeki 3D nesne (cube) ile tıklama etkileşimi oluşturuldu
* Tıklama sonrası:
    * Nesnenin rengi değiştirildi
    * Nesneye ait bilgilendirme yazısı gösterildi
* Nesne üzerine gelindiğinde hover (renk değişimi) efekti eklendi

</details>


<details>
<summary>👉 👤 Perihan Çelikoğlu: VR Uygulamasında Kullanıcı Arayüzü İyileştirmeleri (Açmak için tıklayın)</summary>

UI / UX İyileştirme
Bu görev kapsamında kullanıcı deneyimini artırmaya yönelik basit ama etkili iyileştirmeler yapılmıştır.
Yapılanlar:
* Kullanıcıya yönlendirme sağlamak için bilgilendirme metni eklendi
* Etkileşimlerde kullanıcıya geri bildirim vermek için:
    * Renk değişimi (hover ve click) kullanıldı
* Bilgi metni sistemi:
    * Aç/Kapa (toggle) şeklinde çalışacak şekilde düzenlendi
* Kullanıcının sistemi kolay anlaması için basit ve anlaşılır bir arayüz oluşturuldu

</details>


<details>
<summary>👉 👤 Shuja Ahmad Tariq: AR Educational Toolkit (Açmak için tıklayın)</summary>

# 📱 AR Educational Toolkit
### Augmented Reality Supported Training Application

This repository contains the development progress for an AR-based educational platform designed to bring textbook concepts to life using 3D visualization and interactive mobile technology.

---

## 📖 Project Context
The core mission of this application is to bridge the gap between static 2D learning and immersive 3D exploration. By utilizing **Augmented Reality (AR)**, we transform the student's physical environment into a dynamic classroom where complex biological, mechanical, and physical structures can be manipulated in real-time.

### 🎯 Key Objectives
* **Enhanced Visualization:** Turning abstract concepts (like molecular bonds or planetary motion) into tangible 3D models.
* **Interactivity:** Empowering students to rotate, zoom, and inspect objects as if they were holding them.
* **Hardware Accessibility:** Developed for standard AR-enabled iOS and Android devices.

---

## 🛠️ Technical Stack
* **Engine:** Unity 2023.x (URP)
* **AR Framework:** AR Foundation (ARKit & ARCore)
* **Language:** C#
* **Assets:** Low-poly 3D models optimized for mobile (Blender/Maya)

---

## 📅 Development Roadmap

### ✅ Week 1: Analysis & Scope
* Requirement gathering and stakeholder analysis.
* Selection of initial educational modules.
* Hardware compatibility verification.

### 🏗️ Week 2: Interaction Infrastructure (Current)
* Implementation of **AR Foundation** tracking.
* Development of the **Interaction System**:
    * **Placement:** Raycasting for surface detection.
    * **Rotation:** Single-finger swipe logic.
    * **Scaling:** Two-finger pinch-to-zoom.
* Import pipeline for `.fbx` and `.glb` assets.

### ⏳ Week 3: UI & Content Integration (Upcoming)
* Designing the Mobile HUD and instructional overlays.
* Finalizing high-fidelity educational models.

---

## 🚀 How to Run the Prototype
1.  Clone this repository.
2.  Open the project in **Unity Hub** (Version 2023.x recommended).
3.  Ensure the **AR Foundation** package is installed via Package Manager.
4.  Build to an Android (APK) or iOS (Xcode) device with AR capabilities.

</details>



<details>
<summary>👉 👤 Shuja Ahmad Tariq: AR Training Module: Interactive Learning & Assessment (Açmak için tıklayın)</summary>

# 🎓 AR Training Module: Interactive Learning & Assessment
### Week 4: Pedagogical Layer & User Feedback Systems

Building upon the 3D infrastructure of Week 2 and the content integration of Week 3, Week 4 focuses on turning 3D models into active learning tools through **Gamification**, **Quizzing**, and **Feedback Loops**.

---

## 📋 Task Overview
The goal of this phase is to implement a logic layer that allows students to interact with specific "hotspots" on a model, answer contextual questions, and receive real-time validation of their knowledge.

---

## 🛠️ New Features Implemented

### 1. Interactive Hotspots (Object Interaction)
* **Feature:** Users can tap specific parts of a 3D model (e.g., the 'Left Ventricle' of a heart) to trigger informational pop-ups.
* **Logic:** Implemented using **Physics Raycasters** on specific child-colliders of the 3D model.
* **UI:** World-space canvases that "billboard" (always face the camera) to display labels.

### 2. Assessment Engine (Knowledge Testing)
* **Multiple Choice Questions (MCQ):** Integrated a dynamic UI overlay that presents questions based on the current 3D scene.
* **Spatial Quizzing:** A "Find the Part" mode where the app asks the student to tap a specific component of the AR model.

### 3. Feedback & Validation System
* **Visual Feedback:** Correct answers trigger a green highlight/particle effect; incorrect answers trigger a red pulse.
* **Progress Tracking:** A simple session manager that tracks how many questions the student answered correctly.
* **Audio Cues:** Integration of "Success" and "Try Again" sound effects to reinforce learning.

---

## 💻 Technical Implementation (C#)

This week introduced the `ARQuizManager.cs`, which coordinates the interaction between the 3D model and the UI.

```csharp
using UnityEngine;
using TMPro;

public class ARQuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public GameObject feedbackPanel;

    [Header("State")]
    private string currentTargetPart = "Engine_Piston";

    public void OnObjectTapped(string partName)
    {
        if (partName == currentTargetPart)
        {
            ShowFeedback("Correct! You've identified the Piston.", Color.green);
        }
        else
        {
            ShowFeedback("Not quite. That was the " + partName, Color.red);
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        feedbackPanel.SetActive(true);
        var text = feedbackPanel.GetComponentInChildren<TextMeshProUGUI>();
        text.text = message;
        text.color = color;
    }
}
```
</details>

---

## 4. Hafta (23-30 Nisan)

<details>
<summary>👉 👤 Burçin Ayyıldız: Sanal Gerçeklik Projesi için Ses Entegrasyonu ve Mekansal Ses Deneyimi Oluşturma</summary>
Bu çalışmada, VR projesinin gerçekçilik seviyesini artırmak amacıyla ses efektleri ve mekansal ses teknikleri başarıyla entegre edilmiştir. Öncelikle sahnedeki farklı nesne ve olaylar analiz edilerek her biri için uygun ses efektleri belirlenmiş ve projeye dahil edilmiştir.

Mekansal ses teknolojisi kullanılarak, seslerin kullanıcıya göre konumlandırılması sağlanmıştır. Böylece kullanıcı, sanal ortamda bir sesin hangi yönden geldiğini algılayabilmekte ve bu durum deneyimin daha sürükleyici olmasına katkı sağlamaktadır. Ses kaynaklarına 3D Audio (Spatial Audio) özellikleri eklenmiş, mesafe ve yön faktörlerine bağlı olarak ses şiddeti ve dağılımı ayarlanmıştır.

Ayrıca kullanıcı etkileşimlerine (örneğin nesneye dokunma, hareket etme veya belirli olayların tetiklenmesi) uygun sesler atanmıştır. Bu sayede sistem geri bildirimleri daha anlaşılır ve doğal hale getirilmiştir.

Sonuç olarak, yapılan ses entegrasyonu sayesinde VR ortamının gerçekçiliği ve kullanıcı deneyimi önemli ölçüde geliştirilmiştir.
</details>

<details>
<summary>👉 👤 Burçin Ayyıldız: AR/VR Projelerinde Performans Optimizasyonu</summary>
AR VE VR PERFORMANS ANALIZIVE OPTIMIZASYON RAPORU
Proje Tanımı
Oyun Motoru: Unity
Proje Türü: Sanal Gerçeklik (VR)
Sahne Detayı: Orta düzey karmaşıklığa sahip, etkileşimli nesneler, ışıklandırma vemekansal ses içeren ortam.
   
Başlangıç Performansı
FPS: 45
CPU Kullanımı: %70
GPU Kullanımı: %80
Draw Call: 1200

Son Performans
FPS: 60+
CPU Kullanımı: %50
GPU Kullanımı: %65
Stabilite: Yüksek

Uygulanan İşlemler
Ses Entegrasyonu
Oculus Spatializer kullanılarak mekansal ses sistemi eklendi. Ortam ve etkileşimsesleri başarıyla entegre edildi.
Optimizasyon Teknikleri
LOD (Level of Detail): Nesne mesafesine göre detay seviyeleri düzenlendi.
Batching: Draw call sayısını azaltmak için statik ve dinamik gruplandırma yapıldı.
Ses Optimizasyonu: Ses sıkıştırma yöntemleri ve ses limiti (voice limit) uygulandı

Analiz ve Değerlendirme
En büyük performans artışı Batching ve LOD tekniklerinden elde edilmiştir. Bu sayede draw call sayısı optimize edilmiş ve GPU üzerindeki yük belirgin şekilde
düşürülmüştür.
Ses entegrasyonu, mekansal sesin getirdiği hesaplama yüküne rağmen sistemi olumsuz etkilememiştir. Sıkıştırma ve voice limit kullanımı dengeli bir performans
sağlamıştır.
Sonuç
• 
• 
• 
FPS artışı ile kullanıcı deneyimi akıcı hale getirildi.
Mekansal ses ile daldırma (immersion) hissi artırıldı.
Stabil kare hızı sayesinde VR platformlarında kritik olan hareket tutması (motion sickness) riski minimize edildi.

</details>

<details>
<summary>👉 👤 Ahmet Yaman: Sanal Gerçeklik Ortamında Çoklu Kullanıcı Desteği Entegrasyonu</summary>
# AR Eğitim Uygulaması — Çoklu Kullanıcı Altyapısı Raporu

> **Hazırlayan:** Ahmet Yaman  
> **Konu:** Unity AR Foundation | Netcode for GameObjects | Unity Relay  
> **Tarih:** Nisan 2025

---

## İçindekiler

1. [Yönetici Özeti](#1-yönetici-özeti)
2. [Mimari Tasarım](#2-mimari-tasarım)
3. [Bileşen Detayları](#3-bileşen-detayları)
4. [Ağ Protokolleri ve Veri Akışı](#4-ağ-protokolleri-ve-veri-akışı)
5. [Kurulum ve Entegrasyon](#5-kurulum-ve-entegrasyon)
6. [Performans Analizi ve Sınırlamalar](#6-performans-analizi-ve-sınırlamalar)
7. [Gelecek Geliştirme Yol Haritası](#7-gelecek-geliştirme-yol-haritası)

---

## 1. Yönetici Özeti

Bu rapor, AR Eğitim Uygulaması kapsamında geliştirilen çoklu kullanıcı (multiplayer) altyapısının teknik tasarımını, bileşen mimarisini ve uygulama detaylarını açıklamaktadır.

Proje, Unity AR Foundation üzerine inşa edilmiş olup Unity Relay ve Netcode for GameObjects (NGO) teknolojileri kullanılarak öğrencilerin aynı artırılmış gerçeklik ortamında eş zamanlı etkileşim kurmasına olanak tanımaktadır.

> **Temel Hedef:** Coğrafi olarak farklı konumlarda bulunan öğrencilerin, tek bir öğretmenin yönetiminde, paylaşımlı bir AR sahnesinde birlikte öğrenmesini ve işbirliği yapmasını sağlamak.

### 1.1 Kapsam ve Teknoloji Yığını

| Katman | Teknoloji | Versiyon | Görev |
|---|---|---|---|
| AR Çerçevesi | AR Foundation | 5.x | Yüzey tespiti, kamera yönetimi |
| Ağ Katmanı | Netcode for GameObjects | 1.8.x | RPC, NetworkVariable, senkronizasyon |
| Relay Servisi | Unity Relay | 1.4.x | NAT geçişi, bağlantı aracılığı |
| Kimlik Doğrulama | Unity Authentication | 3.3.x | Anonim oturum, Player ID |
| Platform | iOS / Android | — | ARKit (iOS), ARCore (Android) |
| Dil | C# | .NET 6 | Tüm uygulama mantığı |

---

## 2. Mimari Tasarım

### 2.1 Genel Sistem Mimarisi

Sistem, **Host-Client** modeli üzerine kurulmuştur. Öğretmenin cihazı Host rolünü üstlenerek hem sunucu hem de istemci işlevini yerine getirir. Öğrencilerin cihazları ise Client olarak bağlanır.

```
Öğretmen (Host)
      │
      └──► Unity Relay Sunucusu ──► Öğrenci 1 (Client)
                                ──► Öğrenci 2 (Client)
                                ──► Öğrenci N (Client, maks 6)
```

### 2.2 Bileşen Haritası

Geliştirilen altyapı 6 ana C# bileşeninden oluşmaktadır. Her bileşen tek bir sorumluluğa sahiptir (Single Responsibility Principle):

| Bileşen | Sorumluluk | Ağ Rolü |
|---|---|---|
| `ARMultiplayerManager` | Relay bağlantısı, oda yönetimi, lifecycle | Host + Client |
| `ARAnchorSynchronizer` | AR dünya koordinat sistemi senkronizasyonu | Server Authority |
| `ARPlayerAvatar` | Oyuncu avatarı, konum & rotasyon yayını | Owner Authority |
| `SharedARObject` | 3D nesne taşıma, ölçek, kilitleme | Server Authority |
| `ARCollaborativeTaskManager` | Ortak görev akışı, adım takibi, zamanlayıcı | Server Authority |
| `ARChatSystem` | Metin mesajlaşma, duyurular, mikrofon durumu | Server Broadcast |

### 2.3 Veri Akışı ve NetworkVariable Kullanımı

Ağ üzerinde paylaşılan durum bilgileri `NetworkVariable` ile yönetilmektedir. Bu yaklaşım, değer değişikliklerinin otomatik olarak tüm istemcilere iletilmesini sağlar ve manuel RPC çağrılarını en aza indirir.

```csharp
// Örnek: SharedARObject içinde konum senkronizasyonu
private NetworkVariable<Vector3> _syncPosition =
    new NetworkVariable<Vector3>(Vector3.zero,
        NetworkVariableReadPermission.Everyone,  // Herkes okuyabilir
        NetworkVariableWritePermission.Server);  // Sadece sunucu yazar

// Sunucu tarafında güncelleme (ServerRpc üzerinden tetiklenir)
_syncPosition.Value = newPosition; // Otomatik olarak tüm istemcilere yayınlanır
```

---

## 3. Bileşen Detayları

### 3.1 ARMultiplayerManager — Ana Ağ Yöneticisi

Sistemin giriş noktasıdır. Uygulama başladığında Unity Services ve Authentication servislerini asenkron olarak başlatır. Singleton pattern kullanılarak sahneler arasında kalıcıdır.

**Temel İşlevler:**

- `InitializeUnityServices()` — Unity Dashboard'a bağlanır, anonim Player ID alır
- `CreateRoom()` — Relay sunucusunda yer ayırır, 6 karakterli JOIN kodu üretir
- `JoinRoom(code)` — Kodu kullanarak mevcut odaya bağlanır
- `Disconnect()` — Bağlantıyı güvenli şekilde kapatır, oyuncu listesini temizler

```csharp
// Oda oluşturma — öğretmen cihazında
Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
_joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
_transport.SetHostRelayData(allocation.RelayServer.IpV4, ...);
_networkManager.StartHost();

// Odaya katılma — öğrenci cihazında
JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(joinCode);
_transport.SetClientRelayData(join.RelayServer.IpV4, ...);
_networkManager.StartClient();
```

---

### 3.2 ARAnchorSynchronizer — AR Çıpa Senkronizasyonu

Çoklu kullanıcı AR uygulamalarında en kritik teknik zorluk, farklı cihazların aynı fiziksel konumda aynı sanal içeriği göstermesidir. Bu bileşen, öğretmenin tarattığı AR düzlemini tüm öğrencilerin cihazlarına iletir.

> ⚠️ **Neden Kritik?**  
> Her cihazın AR koordinat sistemi başlangıçta birbirinden bağımsızdır. Çıpa senkronizasyonu olmadan bir öğrencinin ekranında "masanın üzerinde" görünen nesne, başka bir öğrencinin ekranında havada görünür. Bu bileşen tüm cihazları aynı dünya koordinatına hizalar.

**Çalışma Prensibi:**

1. Host (öğretmen) ekrana dokunarak ilk AR düzlemini çıpa olarak belirler
2. Çıpa konumu `NetworkVariable` aracılığıyla tüm istemcilere iletilir
3. Her istemci `ARSessionOrigin`'i bu koordinata hizalar
4. Sonuç: Tüm cihazlar aynı koordinat sistemini paylaşır

---

### 3.3 ARPlayerAvatar — Oyuncu Avatarı

Her bağlı oyuncuyu AR sahnede görsel olarak temsil eder. Kamera konumu 10-20 Hz frekansta ağa yayınlanır, uzak oyuncuların konumları Lerp/Slerp ile yumuşatılır.

| NetworkVariable | Açıklama | Yazma Yetkisi |
|---|---|---|
| `_networkPosition` | AR kamera dünya konumu (Vector3) | Owner |
| `_networkRotation` | AR kamera yönü (Quaternion) | Owner |
| `_colorIndex` | Oyuncu rengi (0-5 arası) | Owner |
| `_playerName` | Görünen isim (maks 64 karakter) | Owner |

Bant genişliği tasarrufu için **delta kontrolü** uygulanmıştır: konum 1 mm'den az, rotasyon 0.1 dereceden az değişirse ağa veri gönderilmez.

---

### 3.4 SharedARObject — Paylaşımlı 3D Nesneler

Öğrencilerin etkileşime girdiği tüm 3D modeller (hücre organeli, atom, geometrik şekil vb.) bu bileşeni kullanır. Nesne taşıma için **optimistik kilit mekanizması** uygulanmıştır.

**Kilit Mekanizması:**

- Bir oyuncu `TryGrab()` çağırdığında sunucuya kilit isteği gönderilir
- Sunucu `_currentHolderClientId` değişkenini günceller
- Nesne zaten tutuluyorsa ikinci istek reddedilir (race condition koruması)
- `Release()` çağrıldığında kilit serbest bırakılır

```csharp
sharedARObject.TryGrab();              // Kilidi al
sharedARObject.MoveObject(pos, rot);   // Frame bazlı güncelleme
sharedARObject.ScaleObject(1.1f);      // Pinch gesture ile %10 büyüt
sharedARObject.Release();              // Kilidi serbest bırak
```

---

### 3.5 ARCollaborativeTaskManager — Görev Sistemi

Eğitimsel aktivitelerin yapılandırılmış bir şekilde yönetilmesini sağlar. `ARTask` ve `ARTaskStep` veri modelleri Unity Inspector'dan doldurularak ders planı oluşturulabilir.

**Görev Yaşam Döngüsü:**

1. Öğretmen `StartTaskServerRpc(index)` ile görevi başlatır
2. Her adım tamamlandığında `CompleteStepServerRpc` çağrılır ve ilerleme tüm oyunculara yayınlanır
3. Tüm adımlar tamamlandığında tüm cihazlara tamamlanma bildirimi gönderilir
4. Oyuncuların %50'si hazır olduğunda sonraki göreve geçilir (oy sistemi)

Zamanlama sistemi **sunucu tarafında** çalışır; istemciler sadece UI güncellemesi alır. Bu, zamanlama hilelerini önler ve tüm cihazlarda tutarlı durum sağlar.

---

### 3.6 ARChatSystem — İletişim Altyapısı

Ders sırasında kesintisiz iletişim için iki katmanlı mesajlaşma sunar: serbest metin ve önceden tanımlı hızlı yanıtlar.

| Hızlı Mesaj | Kullanım Senaryosu |
|---|---|
| Anladım! ✓ | Adımı takip ettiğini bildirme |
| Tekrar açıklar mısın? | Açıklamayı tekrar isteme |
| Harika fikir! | Takım arkadaşını onaylama |
| Dur, bir saniye... | Duraksamayı bildirme |
| Hazırım! | Sonraki adıma geçmeye onay |
| Yardım lazım 🙋 | Destek talep etme |

---

## 4. Ağ Protokolleri ve Veri Akışı

### 4.1 RPC vs NetworkVariable Karar Matrisi

| Mekanizma | Kullanıldığı Yer | Neden? |
|---|---|---|
| `NetworkVariable` | Konum, rotasyon, skor, görev durumu | Sürekli güncellenen durum bilgisi |
| `ServerRpc` | Nesne yakalama, adım tamamlama, mesaj gönderme | Bir kez gerçekleşen eylemler |
| `ClientRpc` | Duyuru yayını, animasyon tetikleme | Sunucudan istemcilere bildirim |

### 4.2 Bant Genişliği Optimizasyon Teknikleri

- **Delta Kontrolü:** Pozisyon 1 mm, rotasyon 0.1° altında değiştiğinde veri gönderilmez
- **Güncelleme Frekansı:** NetworkTransform 10-20 Hz olarak sınırlandırılmalıdır
- **Mesaj Boyutu:** `FixedString64Bytes` kullanımı `string`'e göre yaklaşık %40 daha az bant genişliği tüketir
- **Interpolasyon:** Uzak nesneler Lerp/Slerp ile yumuşatılır, her frame veri gönderilmez

> **Önerilen NetworkTransform Ayarları:**  
> `PositionThreshold: 0.001` | `RotationThreshold: 0.1` | `ScaleThreshold: 0.01` | `Tick Rate: 15 Hz`

### 4.3 Güvenlik ve Yetkilendirme

Tüm state-changing işlemler sunucu tarafında doğrulanmaktadır. İstemciler doğrudan `NetworkVariable` yazmaz; bunun yerine `ServerRpc` aracılığıyla sunucudan yetki ister.

```csharp
// Güvenli nesne güncelleme akışı:
// 1. İstemci → ServerRpc gönderir
[ServerRpc(RequireOwnership = false)]
private void UpdateTransformServerRpc(Vector3 pos, Quaternion rot, Vector3 scale)
{
    // Sunucu doğrular ve NetworkVariable'ı günceller
    _syncPosition.Value = pos;
}
// 2. NetworkVariable → Tüm istemcilere otomatik yayın yapılır
```

---

## 5. Kurulum ve Entegrasyon

### 5.1 Gerekli Paketler

| Paket Adı | Versiyon | Kaynak |
|---|---|---|
| `com.unity.netcode.gameobjects` | 1.8.x | Unity Package Manager |
| `com.unity.services.relay` | 1.4.x | Unity Package Manager |
| `com.unity.services.authentication` | 3.3.x | Unity Package Manager |
| `com.unity.xr.arfoundation` | 5.x | Unity Package Manager |
| `com.unity.xr.arkit` | 5.x | Unity Package Manager (iOS) |
| `com.unity.xr.arcore` | 5.x | Unity Package Manager (Android) |
| `com.unity.textmeshpro` | 3.x | Unity Package Manager |

### 5.2 Unity Services Yapılandırması

1. Unity Dashboard'da yeni proje oluşturun veya mevcut projeyi seçin
2. Services menüsünden **Relay** ve **Authentication** servislerini etkinleştirin
3. `Edit > Project Settings > Services` altından Project ID'yi projeye bağlayın
4. Authentication servisinde **Anonymous Sign-in** metodunu aktifleştirin

### 5.3 Sahne Hiyerarşisi

```
[AR Session]
├── AR Session (component)
└── AR Session Origin
    ├── AR Camera
    ├── AR Plane Manager      ← ARAnchorSynchronizer'a referans ver
    ├── AR Raycast Manager    ← ARAnchorSynchronizer'a referans ver
    └── AR Anchor Manager

[Network]
├── NetworkManager
│   ├── NetworkManager (component) → PlayerPrefab: ARPlayerAvatar
│   └── UnityTransport (component)
├── ARMultiplayerManager
├── ARAnchorSynchronizer
├── ARCollaborativeTaskManager
└── ARChatSystem
```

### 5.4 Temel Kullanım Örnekleri

```csharp
// Oda oluşturma (Öğretmen)
string joinCode = await ARMultiplayerManager.Instance.CreateRoom();

// Odaya katılma (Öğrenci)
await ARMultiplayerManager.Instance.JoinRoom("AB3X9K");

// Görev başlatma
ARCollaborativeTaskManager.Instance.StartTaskServerRpc(0);

// Mesaj gönderme
ARChatSystem.Instance.SendMessage("Organeli doğru yere koydum!");
ARChatSystem.Instance.SendQuickMessage(0); // "Anladım! ✓"

// Event dinleme
ARMultiplayerManager.Instance.OnPlayerJoined += name => Debug.Log($"{name} katıldı");
ARCollaborativeTaskManager.Instance.OnTaskCompleted += task => ShowCelebration();
```

---

## 6. Performans Analizi ve Sınırlamalar

### 6.1 Performans Önerileri

| Alan | Öneri | Beklenen Etki |
|---|---|---|
| Güncelleme Hızı | NetworkTransform: 10-15 Hz | Bant genişliği %40 azalır |
| Oyuncu Sayısı | Ders başına 2-6 öğrenci önerilir | Sunucu yükü optimal kalır |
| 3D Model Poligonu | Mobil için maks 10.000 poly/nesne | 60 FPS hedef kararlı olur |
| AR Çıpası | Uygulama başında bir kez kurulur | Koordinat kayması önlenir |
| Mesaj Boyutu | `FixedString64Bytes` kullanımı | Heap allokasyonu azalır |

### 6.2 Bilinen Sınırlamalar

- **AR Çıpası Hassasiyeti:** Farklı cihaz modellerinde ARKit/ARCore hassasiyeti değişebilir
- **Işık Koşulları:** Düşük ışıklı ortamlarda yüzey tespiti başarısız olabilir
- **Sesli İletişim:** Mevcut yapıda sesli sohbet entegre değildir; Vivox veya Agora ile genişletilmesi gerekir
- **Offline Mod:** Tüm işlevler internet bağlantısı gerektirir
- **Eş Zamanlı Nesne Düzenleme:** Aynı nesneyi iki oyuncu aynı anda taşıyamaz (tasarım gereği)

> 💡 **Sesli İletişim için Öneri:**  
> Unity Vivox (`com.unity.services.vivox`) paketi, Unity Relay ile aynı oturum altyapısını paylaşır ve en düşük entegrasyon maliyetiyle sesli iletişim sağlar. `ARChatSystem.SetMicStatusServerRpc()` metodu zaten bu entegrasyon için hazırdır.

---

## 7. Gelecek Geliştirme Yol Haritası

| Öncelik | Özellik | Bağımlılık |
|---|---|---|
| 🔴 Yüksek | Sesli iletişim (Unity Vivox) | `com.unity.services.vivox` |
| 🔴 Yüksek | Kalıcı oda sistemi (Lobby Service) | `com.unity.services.lobby` |
| 🟡 Orta | Öğrenci ilerleme kaydı ve analytics | Unity Analytics / Backend DB |
| 🟡 Orta | Öğretmen ekranı — tüm öğrencilerin perspektifi | Ek RPC tasarımı |
| 🟢 Düşük | Haptic feedback ile nesne etkileşimi | Platform haptic API |
| 🟢 Düşük | 3D ses konumlandırma (spatial audio) | Unity Spatializer plugin |

---

## Dosya Listesi

| Dosya | Açıklama |
|---|---|
| `ARMultiplayerManager.cs` | Ana ağ yöneticisi, Relay bağlantısı |
| `ARAnchorSynchronizer.cs` | AR dünya çıpası senkronizasyonu |
| `ARPlayerAvatar.cs` | Oyuncu avatar & konum senkronizasyonu |
| `SharedARObject.cs` | Paylaşılan etkileşimli 3D nesneler |
| `ARCollaborativeTaskManager.cs` | Ortak görev/öğrenme sistemi |
| `ARChatSystem.cs` | Metin mesajlaşma + hızlı yanıtlar |

---

*Hazırlayan: Ahmet Yaman — AR Eğitim Uygulaması, Çoklu Kullanıcı Altyapısı*

</details>

<details>
<summary>👉 👤 Ahmet Yaman: Artırılmış Gerçeklik Uygulaması için Basit Bir Kullanıcı Arayüzü Tasarımı ve Entegrasyonu</summary>
Bu haftada yapılan görev buraya yapıştırılacak!!

   # AR Kullanıcı Arayüzü Tasarım Raporu

**Proje:** Unity AR Arayüzü  
**Teknoloji:** Unity 2022.3 LTS · AR Foundation 5.x · ARKit / ARCore  
**Tarih:** Mayıs 2026

---

## 1. Genel Bakış

Bu rapor, artırılmış gerçeklik (AR) ortamında çalışan bir kullanıcı arayüzünün tasarım kararlarını, bileşen mimarisini ve teknik entegrasyon sürecini özetlemektedir. Arayüz; model seçimi, sistem ayarları ve temel nesne kontrollerini kapsayan, sezgisel ve düşük bilişsel yüklü bir deneyim sunmak amacıyla geliştirilmiştir.

---

## 2. Tasarım Prensipleri

Arayüz üç temel prensip üzerine inşa edilmiştir.

**Uzamsal Dürüstlük:** Kullanıcının fiziksel çevreyi görmesini engelleyen hiçbir UI elemanı ekran merkezini kapatmaz. Tüm kalıcı kontroller ekranın kenar bölgelerine yerleştirilmiştir.

**İki Katmanlı Mimari:** Screen Space ve World Space paradigmaları birbirini tamamlayacak şekilde kullanılmıştır. Navigasyon ve ayar kontrolleri Screen Space'te (ekrana sabit), nesneye özel kontroller ise World Space'te (3D dünyada) konumlandırılmıştır.

**Aşamalı İfşa:** Kullanıcı yalnızca o an ihtiyaç duyduğu bilgiyle karşılaşır. Onboarding, ayarlar ve bağlamsal kontroller gerektiğinde açılıp kapanır.

---

## 3. Arayüz Bileşenleri

### 3.1 Screen Space — Ekrana Sabit Katman

**HUD Üst Bar** ekranın tepesinde yer alır ve seçili modelin adını ile ayarlar butonunu (⚙) gösterir. AR oturum durumu yeşil yanıp sönen nokta ile anlık olarak iletilir.

**Carousel Model Seçici** ekranın alt bölümünde, başparmakla kolayca erişilebilen "Thumb Zone" alanında konumlanır. Yatay kaydırmalı yapısı sayesinde kamera görüntüsünü dikey eksende boğmaz. Seçilen model kartı renk ve boyut değişimiyle anlık görsel geri bildirim verir.

**Aksiyon Satırı** Carousel'in hemen altında üç butonu barındırır: Ses Komutu, Yerleştir ve Sıfırla.

**Ayarlar Paneli** ⚙ butonuyla açılan glassmorphism estetiğinde bir kaplama olarak gelir. Işık tahmini, oklüzyon, uzamsal ses ve ses komutu toggle'ları ile beş kademeli render kalitesi slider'ı içerir. Açıkken arka plan kamerası tamamen kapanmaz; fiziksel dünya ile bağlantı korunur.

### 3.2 World Space — 3D Dünyaya Gömülü Katman

Bir model sahnede yerleştirildiğinde, modelin yanında 3D uzayda asılı duran bir kontrol paneli belirir. Bu panel dört işlevi destekler: döndürme, büyütme, küçültme ve silme. Panel, kullanıcı modelin etrafında yürürken de okunabilir kalmak için her zaman AR kamerasına döner (billboard mekanizması).

### 3.3 Onboarding ve Kalibrasyon

Uygulama açıldığında ekranda tek seferlik bir zemin tarama rehberi görünür. "Kameranızı yavaşça hareket ettirin" gibi eyleme yönelik açıklamalar sunulur; jenerik hata mesajları yerine kullanıcıya neyin yanlış gittiği ve nasıl düzelteceği söylenir. Zemin tespit edilince progress bar tamamlanır ve kullanıcı "Başlat" butonuyla deneyime geçer.

### 3.4 Bildirim Sistemi

Her kullanıcı eylemi ekranın üst bölümünde kısa bir bildirimle doğrulanır. Bildirimler fade-in / fade-out animasyonuyla 2 saniye sonra kaybolur ve birbirini ezmez; yeni bir bildirim öncekini iptal ederek görünür.

---

## 4. Etkileşim Modelleri

| Eylem | Yöntem |
|---|---|
| Model seçme | Carousel'de tek dokunuş |
| Model yerleştirme | "Yerleştir" butonu, zemin reticle üzerinde |
| Boyutlandırma | İki parmak pinch-to-zoom |
| Döndürme | İki parmak rotasyon jesti |
| Nesne kontrolü | World Space panel butonları |
| Ses komutu | Mikrofon butonu → NLP simülasyonu |
| Ayarlar | FAB (⚙) → Glassmorphism panel |

---

## 5. Teknik Yığın

```
Unity 2022.3 LTS
├── AR Foundation 5.x
│   ├── ARRaycastManager   — zemin tespiti
│   ├── ARPlaneManager     — düzlem haritalama
│   ├── ARCameraManager    — ışık tahmini
│   └── AROcclusionManager — kişi oklüzyonu
├── TextMeshPro            — UI tipografisi
└── Unity UI (UGUI)        — tüm arayüz bileşenleri
```

**Script Mimarisi:**

- `ARUIManager.cs` — merkezi koordinatör, tüm bileşenler arası iletişim
- `CarouselItem.cs` — tekrarlanabilir model kartı bileşeni
- `SettingsManager.cs` — AR Foundation API bağlantılı ayar yönetimi
- `TouchGestureController.cs` — çok dokunuşlu jest algılama
- `ReticleController.cs` — zemin hedefleyici ve yüzey geri bildirimi

---

## 6. Test Senaryoları

Arayüzün doğrulanması için üç ortam koşulunda test yapılması önerilir.

**İdeal koşul:** İyi aydınlatılmış, düz dokulu zemin. Tüm bileşenlerin temel işlevleri burada doğrulanır.

**Düşük ışık:** Karanlık ortamda ışık tahmini toggle'ının davranışı ve kalibrasyon yönergelerinin etkinliği gözlemlenir.

**Hareketli ortam:** Kullanıcı modelin etrafında yürürken World Space panelin billboard davranışı ve okunabilirliği test edilir.

---

## 7. Bilinen Kısıtlamalar

Ses komutu şu an simülasyon modunda çalışmaktadır; gerçek NLP entegrasyonu için Unity Sentis veya platform yerel API'lerinin (SpeechRecognizer / SFSpeechRecognizer) bağlanması gerekir. Pinch ve döndürme jestleri aynı anda kullanıldığında küçük öncelik çakışmaları yaşanabilir; bu durum gesture threshold değerlerinin hassas ayarlanmasıyla giderilebilir.

---

## 8. Sonuç

Geliştirilen arayüz, fiziksel dünyayla çatışmayan, bağlama duyarlı ve katmanlı bir AR deneyimi sunmaktadır. Screen Space ile World Space arasındaki bilinçli denge, kullanıcının hem navigasyona hem de yerleştirilen nesneye olan dikkatini verimli şekilde yönetir. LeanTween gibi üçüncü parti bağımlılıklar bilinçli olarak dışarıda bırakılmış; tüm sistem yalnızca Unity'nin yerleşik bileşenleri ve AR Foundation üzerine inşa edilmiştir.

</details>
