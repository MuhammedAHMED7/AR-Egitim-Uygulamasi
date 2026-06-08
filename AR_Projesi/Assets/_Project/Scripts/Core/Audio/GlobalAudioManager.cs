using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    /// <summary>
    /// Projenin mekansal ses ve global ses yönetiminden sorumlu merkezi mimari sýnýf.
    /// </summary>
    [RequireComponent(typeof(AudioSource))] // Bu script eklenen objede otomatik AudioSource oluþturur.
    public class GlobalAudioManager : MonoBehaviour
    {
        public static GlobalAudioManager Instance { get; private set; }

        [Header("Audio Mixer & Routing")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup spatialSFXGroup;

        [Header("Spatial Audio Settings")]
        [Tooltip("3D Seslerin maksimum duyulma mesafesi")]
        [SerializeField] private float maxAudioDistance = 20f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        private AudioSource backgroundMusicSource;

        private void Awake()
        {
            InitializeSingleton();
            SetupBackgroundMusicSource();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void SetupBackgroundMusicSource()
        {
            backgroundMusicSource = GetComponent<AudioSource>();
            backgroundMusicSource.playOnAwake = false;
            backgroundMusicSource.loop = true;

            // Arka plan müziði 2D olmalýdýr (Her yerden eþit duyulur)
            backgroundMusicSource.spatialBlend = 0f;

            if (musicGroup != null)
            {
                backgroundMusicSource.outputAudioMixerGroup = musicGroup;
            }
        }

        /// <summary>
        /// Sahneler arasý kesintisiz çalacak arka plan müziðini baþlatýr.
        /// </summary>
        public void PlayBackgroundMusic(AudioClip clip, float volume = 1f)
        {
            if (clip == null || backgroundMusicSource.clip == clip) return;

            backgroundMusicSource.clip = clip;
            backgroundMusicSource.volume = volume;
            backgroundMusicSource.Play();
        }

        /// <summary>
        /// Belirtilen 3 boyutlu konumda tek seferlik mekansal bir ses efekti tetikler.
        /// Peri veya Þuca'nýn objeleri spawn olduðunda bu metod kullanýlabilir.
        /// </summary>
        public void PlaySpatialAudioAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            // Dinamik olarak runtime'da bir ses objesi oluþturuyoruz
            GameObject audioObject = new GameObject($"SpatialAudio_{clip.name}");
            audioObject.transform.position = position;

            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;

            // KRÝTÝK AYAR: 0f tamamen 2D, 1f tamamen 3D (Mekansal) sestir.
            source.spatialBlend = 1f;

            // Oluþturduðun Mixer grubuna baðlýyoruz
            if (spatialSFXGroup != null)
            {
                source.outputAudioMixerGroup = spatialSFXGroup;
            }

            // Mesafe ayarlarý (Yaklaþtýkça çok, uzaklaþtýkça az duyulmasý için)
            source.rolloffMode = rolloffMode;
            source.minDistance = 1f;
            source.maxDistance = maxAudioDistance;

            // Sesi çal
            source.Play();

            // Ses bittiðinde objeyi hafýzadan temizle (Memory Leak / Bellek sýzýntýsýný önler)
            Destroy(audioObject, clip.length);
        }
    }
}