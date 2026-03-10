## 1. Hafta 

* **Muhammed (Proje Yöneticisi):** ### Artırılmış Gerçeklik Teknolojileri Araştırma Raporu
  > **Araştırmayı Yapan:** Muhammed AHMED | **Durum:** Tamamlandı

  Proje kapsamında kullanılacak Artırılmış Gerçeklik (AR) altyapısı için sektördeki çeşitli SDK'lar (Vuforia, EasyAR, AR Foundation) teknik açıdan incelenmiştir. Yapılan analizler ve Gereksinim Toplama ve Belgeleme Dokümanı doğrultusunda, projemizde Unity AR Foundation ve Android platformu için Google ARCore eklentisinin kullanılmasına karar verilmiştir.

  **Neden AR Foundation ve ARCore Seçildi?**
  * **Çapraz Platform (Cross-Platform) Desteği:** AR Foundation, projenin temel yapısını bozmadan hem Android (ARCore) hem de opsiyonel olarak iOS (ARKit) cihazlar için geliştirme yapmaya olanak tanır.
  * **Unity Entegrasyonu:** Projede kullanılacak olan Unity 2022 LTS sürümünün paket yöneticisi (Package Manager) üzerinden resmi ve stabil şekilde projeye eklenebilir. Ek bir yazılım kurulumu gerektirmez.
  * **Performans ve Maliyet:** Projemiz eğitim amaçlı bir prototip olduğundan ücretli sistemlerin zorunlu logo (filigran) kısıtlamalarından kaçınmak için tamamen ücretsiz olan bu mimari tercih edilmiştir. Ayrıca ARCore mobil cihazlarda minimum 30 FPS performansı sağlayacak şekilde optimize edilmiştir.
  * **Görsel İşaretleyici (Marker) Algılama:** Ders kitaplarındaki görsellerin kamera ile taranması ve üzerine 3 boyutlu eğitim modellerinin yerleştirilmesi işlemi, AR Foundation'ın sunduğu Image Tracking (Görsel İzleme) teknolojisi sayesinde yüksek doğrulukla gerçekleştirilebilmektedir.

* **Shuja Ahmad Tariq:** Bu hafta yapılan araştırmalar ve görevler buraya eklenecektir.

* **Ahmet Yaman:** Bu hafta yapılan araştırmalar ve görevler buraya eklenecektir.

* **Burçin Ayyıldız:** Bu hafta yapılan araştırmalar ve görevler buraya eklenecektir.

* **Perihan Çelikoğlu:** Bu hafta yapılan araştırmalar ve görevler buraya eklenecektir.
  
                   ### Artırılmış Gerçeklik Teknolojileri
* Gereksinim Toplama ve Belgeleme Dokümanı
  
* 1. Proje Tanımı    
>Bu projenin amacı, öğrencilerin öğrenme deneyimini geliştirmek için Artırılmış Gerçeklik (Augmented Reality – AR) teknolojisini kullanan bir mobil eğitim uygulaması geliştirmektir. 
Uygulama sayesinde öğrenciler ders kitaplarında bulunan görselleri veya belirli işaretleyicileri (marker) mobil cihazlarının kamerası ile tarayarak bu görsellerin 3 boyutlu modellerini ve etkileşimli içeriklerini görüntüleyebilecektir. 
Bu sayede öğrenciler; 
- Soyut kavramları daha iyi anlayabilecek 
- 3D modeller üzerinden konuları inceleyebilecek 
- Etkileşimli öğrenme deneyimi yaşayacaktır 
Örneğin: 
Bir öğrenci ders kitabında bulunan güneş sistemi görselini mobil uygulama ile taradığında, ekranda 3 boyutlu güneş sistemi modeli görüntülenecek ve öğrenci gezegenleri inceleyebilecektir. 

* 2. Projenin Amaçları 
>Projenin temel amaçları şunlardır: 
- Öğrencilerin görsel ve etkileşimli öğrenme deneyimini artırmak 
- Artırılmış gerçeklik teknolojisini eğitim alanında kullanmak 
- Ders materyallerini daha anlaşılır hale getirmek 
- Mobil cihazlar üzerinden erişilebilir bir öğrenme platformu oluşturmak

* 3. Proje Kapsamı
>Bu proje kapsamında geliştirilecek sistem aşağıdaki özellikleri içerecektir: 
-Mobil AR uygulaması 
-Kamera ile nesne veya görsel tanıma 
-3 boyutlu model görüntüleme 
-Kullanıcı etkileşimi (model döndürme, yakınlaştırma vb.) 
-Temel kullanıcı arayüzü

>Proje kapsamında geliştirilmeyecek özellikler: 
-Karmaşık kullanıcı hesap sistemi 
-Çevrim içi veri yönetimi 
-Büyük ölçekli içerik yönetim sistemi 
Bu proje eğitim amaçlı bir prototip olarak geliştirilecektir.

* 4. Hedef Kullanıcılar 
>Bu sistemin hedef kullanıcıları şunlardır:
-Öğrenciler 
Ders materyallerini inceleyen kullanıcılar 
-Öğretmenler 
Eğitim içeriklerini sınıf ortamında kullanabilecek kişiler

* 5. Kullanılacak Teknolojiler
Bu proje üniversite 1. sınıf yazılım öğrencileri için uygun teknolojiler kullanılarak geliştirilecektir.

>5.1 Oyun Motoru 
- Unity 2022 LTS 
Sebebi: 
AR geliştirme için yaygın kullanılır 
Öğrenmesi kolaydır 
Mobil uygulama geliştirmeye uygundur 

>5.2 Programlama Dili 
- C# 
Unity içinde kullanılan ana programlama dilidir. 
Kullanım amacı: 
Uygulama mantığını oluşturmak 
Kullanıcı etkileşimlerini yönetmek
AR sahnelerini kontrol etmek 

>5.3 Artırılmış Gerçeklik Teknolojisi
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

>5.4 3D Modelleme
-Blender
Kullanım amacı: 
Eğitimde kullanılacak 3D modelleri oluşturmak 
Modelleri optimize etmek

>5.5 Mobil Platform
Proje aşağıdaki platformlar için geliştirilecektir: 
Android (öncelikli) 
iOS (opsiyonel) 

* 6. Sistem Mimarisi
Sistem aşağıdaki bileşenlerden oluşacaktır: 
> Mobil Uygulama 
Kullanıcının uygulamayı çalıştırdığı ana sistemdir.

>AR Kamera Sistemi 
Telefon kamerası ile gerçek dünyayı algılar ve sahneye 3D model yerleştirir. 

>3D Model Sistemi 
Eğitim içeriklerinin üç boyutlu modellerini içerir. 

>Kullanıcı Arayüzü 
Kullanıcı ile uygulama arasındaki etkileşimi sağlar.

* 7. Fonksiyonel Gereksinimler
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

* 8. Fonksiyonel Olmayan Gereksinimler
>Performans 
Uygulama mobil cihazlarda akıcı çalışmalıdır. 
Minimum: 30 FPS 
>Kullanılabilirlik 
Arayüz basit ve anlaşılır olmalıdır. 
>Uyumluluk 
Android 10 ve üzeri cihazlarda çalışmalıdır. 
>Güvenlik 
Uygulama kullanıcıdan sadece kamera erişim izni isteyecektir.

* 9. Kullanıcı Hikayeleri (User Stories)
Kullanıcı Hikayesi 1 
-Bir öğrenci olarak ders kitabındaki bir görseli taramak istiyorum, çünkü konuyu 3 boyutlu olarak incelemek istiyorum.

Kullanıcı Hikayesi 2 
-Bir öğrenci olarak 3D modeli döndürmek istiyorum, çünkü modelin farklı açılardan nasıl göründüğünü incelemek istiyorum. 

-Kullanıcı Hikayesi 3 
Bir öğrenci olarak model üzerinde bilgi etiketlerini görmek istiyorum, çünkü konuyu daha iyi anlamak istiyorum. 

Kullanıcı Hikayesi 4 
-Bir öğrenci olarak uygulamayı kolay bir şekilde kullanmak istiyorum, çünkü hızlı bir şekilde öğrenmek istiyorum. 

* 10. Test Gereksinimleri 
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

* 11. Teslim Edilecek Çıktılar 
>Proje sonunda aşağıdaki çıktılar teslim edilecektir: 
>Artırılmış gerçeklik sahneleri 
>Etkileşimli 3D modeller 
>Mobil uygulama arayüzü 
>Test senaryoları 
>Kullanıcı kılavuzu

* 12. Proje Ekibi 
Proje ekibi aşağıdaki rollere ayrılacaktır: 
>AR Geliştirici 
Unity ve AR sistemlerini geliştirir. 
>3D Model Tasarımcısı 
Blender kullanarak 3D modeller oluşturur. 
>Arayüz Geliştirici 
Mobil uygulama arayüzünü tasarlar. 
>Proje Analisti 
Gereksinim dokümanını ve planlamayı hazırlar. 
