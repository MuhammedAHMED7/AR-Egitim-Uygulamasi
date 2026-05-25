using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton Tanımlaması (Diğer scriptlerden kolayca erişmek için)
    public static AudioManager Instance { get; private set; }

    [Header("2D Arayüz Ses Klipleri")]
    public AudioClip uiClickSound;
    public AudioClip uiHoverSound;
    public AudioClip successSound;
    public AudioClip failSound;

    [Header("3D Mekansal Ses Klipleri")]
    public AudioClip playerJoinedSound;
    public AudioClip objectGrabSound;
    public AudioClip objectPlaceSound;
    public AudioClip chatMessageSound;

    // 2D sesleri çalmak için kullanılacak yerleşik AudioSource
    private AudioSource _2dAudioSource;

    private void Awake()
    {
        // Singleton Yapılandırması
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arası geçişte ses yöneticisinin silinmesini önler
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2D Sesler için AudioSource bileşenini otomatik ekle
        _2dAudioSource = gameObject.AddComponent<AudioSource>();
        _2dAudioSource.spatialBlend = 0.0f; // 0 = Tamamen 2D (Stereo)
        _2dAudioSource.playOnAwake = false;
    }

    /// <summary>
    /// 2D (Stereo) bir ses çalar. Genellikle UI tıklamaları, başarı/hata bildirimleri için kullanılır.
    /// </summary>
    public void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _2dAudioSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// 3D (Mekansal/Spatial) yönlü bir ses çalar. Ses, belirtilen 3D konumdan (position) gelir.
    /// </summary>
    /// <param name="clip">Çalınacak ses dosyası</param>
    /// <param name="position">Sesin çıkacağı 3D koordinat</param>
    /// <param name="volume">Ses yüksekliği (0 ile 1 arası)</param>
    /// <param name="minDistance">Sesin en yüksek seviyede duyulacağı maksimum yakınlık mesafesi (metre)</param>
    /// <param name="maxDistance">Sesin tamamen sönümleneceği uzaklık mesafesi (metre)</param>
    public void Play3D(AudioClip clip, Vector3 position, float volume = 1f, float minDistance = 0.5f, float maxDistance = 15f)
    {
        if (clip == null) return;

        // Geçici bir GameObject oluşturup ses bittiğinde otomatik yok eden güvenli mekanizma
        GameObject tempAudioGo = new GameObject("Temp3DAudio_" + clip.name);
        tempAudioGo.transform.position = position;

        AudioSource audioSource = tempAudioGo.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        
        // 3D Ses Yapılandırması
        audioSource.spatialBlend = 1.0f; // 1.0 = Tamamen 3D (Mekansal)
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // Gerçekçi uzaklık sönümlemesi
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.playOnAwake = false;

        audioSource.Play();

        // Ses dosyası çalmayı bitirdikten sonra oluşturduğumuz geçici nesneyi sahneden temizleriz (hafıza yönetimi)
        Destroy(tempAudioGo, clip.length);
    }

    // --- Kolay Erişim Fonksiyonları ---
    public void PlayUIClick() => Play2D(uiClickSound);
    public void PlayUIHover() => Play2D(uiHoverSound);
    public void PlaySuccess() => Play2D(successSound);
    public void PlayFail() => Play2D(failSound);
    
    public void PlayPlayerJoined(Vector3 pos) => Play3D(playerJoinedSound, pos);
    public void PlayObjectGrab(Vector3 pos) => Play3D(objectGrabSound, pos, 0.8f, 0.2f, 10f);
    public void PlayObjectPlace(Vector3 pos) => Play3D(objectPlaceSound, pos, 1f, 0.2f, 10f);
    public void PlayChatMessage(Vector3 pos) => Play3D(chatMessageSound, pos);
}
