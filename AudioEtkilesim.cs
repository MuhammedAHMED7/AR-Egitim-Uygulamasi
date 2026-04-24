using UnityEngine;

public class AudioEtkilesim : MonoBehaviour
{
    // Bileþenleri tutacak deðiþkenler
    private AudioSource audioSource;

    [Header("Ses Dosyalari")]
    [Tooltip("Küp yere çarptýðýnda çalacak ses")]
    public AudioClip dropSesi;

    [Tooltip("Küpe týklandýðýnda çalacak ses")]
    public AudioClip tiklamaSesi;

    // Oyun baþladýðýnda bir kez çalýþýr
    void Start()
    {
        // Küpün üzerindeki AudioSource bileþenine ulaþýyoruz
        audioSource = GetComponent<AudioSource>();

        // Eðer AudioSource eklemeyi unuttuysan seni uyarýr
        if (audioSource == null)
        {
            Debug.LogError("Hata: Küpün üzerinde Audio Source bileþeni eksik! Lütfen Inspector'dan ekle.");
        }
    }

    // Küp fiziksel bir nesneye (Plane gibi) çarptýðýnda çalýþýr
    private void OnCollisionEnter(Collision collision)
    {
        // Console'da neye çarptýðýný görmeni saðlar (Hata ayýklama için)
        Debug.Log("Çarpýþma Algýlandý! Çarpýlan nesne: " + collision.gameObject.name);

        // Ses dosyasý ve AudioSource yerindeyse sesi çal
        if (dropSesi != null && audioSource != null)
        {
            audioSource.PlayOneShot(dropSesi);
        }
    }

    // Küpün üzerine mouse ile týklandýðýnda çalýþýr
    private void OnMouseDown()
    {
        Debug.Log("Küpe týklandý!");

        if (tiklamaSesi != null && audioSource != null)
        {
            audioSource.PlayOneShot(tiklamaSesi);
        }
    }
}
