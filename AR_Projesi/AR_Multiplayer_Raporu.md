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
