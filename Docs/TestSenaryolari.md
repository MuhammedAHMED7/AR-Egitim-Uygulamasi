# AR Eğitim Uygulaması - Test Senaryoları ve Doğrulama Adımları

Uygulamanın hem Unity Editor (Play Mode) içerisinde hem de fiziksel bir cihazda (Telefon/Tablet) doğru çalıştığını doğrulamak için aşağıdaki test adımlarını izleyin.

## 1. Düzlem (Plane) Algılama Testi
- **Amaç:** Cihaz kamerasının fiziksel zemini doğru bir şekilde algılaması.
- **Adım 1:** Uygulamayı telefonunuzda açın ve bir eğitim içeriği (örn: Güneş Sistemi) seçin.
- **Adım 2:** Telefon kamerasını iyi aydınlatılmış, dokulu bir zemine (halı, ahşap masa vb.) doğrultun ve cihazı yavaşça hareket ettirin.
- **Beklenen Sonuç:** Zeminde AR Foundation'ın algıladığını gösteren görsel noktacıkların veya karelerin (Plane Indicator) belirmesi gerekir.

## 2. 3D Model Yerleştirme Testi (Object Spawning)
- **Amaç:** Algılanan zemin üzerine modelin başarıyla yerleştirilmesi.
- **Adım 1:** Zemin algılandıktan sonra (görsel belirteçler çıktığında) ekranın rastgele bir noktasına dokunun.
- **Beklenen Sonuç:** Seçilen eğitim modelinin (örneğin hücre modeli) dokunduğunuz noktada, doğru yöne bakacak şekilde belirmesi.

## 3. Etkileşim Testi (Döndürme ve Boyutlandırma)
- **Amaç:** Öğrencinin modeli inceleyebilmesi için doğru etkileşimlerin çalışması.
- **Adım 1 (Döndürme):** Ekrana tek parmağınızla dokunun ve parmağınızı sağa veya sola sürükleyin.
  - **Beklenen Sonuç:** 3D modelin kendi Y ekseni etrafında düzgünce dönmesi.
- **Adım 2 (Boyutlandırma):** Ekrana iki parmağınızla (Pinch) dokunun ve parmaklarınızı birbirinden uzaklaştırın (Zoom In) veya yakınlaştırın (Zoom Out).
  - **Beklenen Sonuç:** Modelin gerçek dünyadaki boyutunun büyümesi veya küçülmesi. Aşırı büyüme ve küçülmenin sınırlandırılmış (Clamp) olması.

## 4. UI ve İçerik Bilgisi Testi
- **Amaç:** Bilgi panellerinin doğru veriyi çekmesi ve geçişlerin sorunsuz olması.
- **Adım 1:** AR sahnesindeyken "Bilgi" veya "Detay" butonuna basın.
- **Beklenen Sonuç:** Ekranda `EducationalContent` (ScriptableObject) içerisinden çekilen başlık (Title) ve açıklama (Description) metinlerinin görünmesi.
- **Adım 2:** "Geri Dön" butonuna basarak Ana Menüye geçin ve farklı bir konuyu seçin.
- **Beklenen Sonuç:** Önceki AR oturumunun temizlenmesi (eski modelin yok olması) ve yeni modelin yerleştirilmeye hazır hale gelmesi.

## 5. Hata Toleransı Testleri
- **Karanlık Ortam:** Işığın yetersiz olduğu bir ortamda zemin algılamayı deneyin. Sistemin çökmek yerine algılamayı beklemeye devam etmesi gerekir.
- **Pürüzsüz Zemin:** Dümdüz ve tek renk bir zemine (örneğin beyaz bir kağıt) tutulduğunda zemin algılaması gecikebilir. Kamerayı yavaşça hareket ettirmek gerekir.

> **Not:** Eğer bu testlerden birisi başarısız olursa, Unity üzerinde `ARManager` ve `ObjectSpawner` betiklerindeki referansların doğru atandığından emin olun.
