using UnityEngine;

public class AudioObjectInteraction : MonoBehaviour
{
    // Bu script 3D Nesnelerin (Hücre organeli, atom, geometrik şekil vb.) üzerine eklenecektir.
    
    /// <summary>
    /// Nesne bir oyuncu tarafından tutulduğunda bu fonksiyon çağrılır.
    /// Bunu arkadaşınızın SharedARObject.TryGrab() veya nesneyi tutan el/gesture koduna bağlayacağız.
    /// </summary>
    public void PlayGrabSound()
    {
        if (AudioManager.Instance != null)
        {
            // Nesnenin o anki 3D pozisyonunda ses çalar
            AudioManager.Instance.PlayObjectGrab(transform.position);
        }
    }

    /// <summary>
    /// Nesne bırakıldığında bu fonksiyon çağrılır.
    /// Bunu arkadaşınızın SharedARObject.Release() veya nesne yerleştirme koduna bağlayacağız.
    /// </summary>
    public void PlayReleaseSound()
    {
        if (AudioManager.Instance != null)
        {
            // Nesnenin bırakıldığı pozisyonda yerleştirme sesi çalar
            AudioManager.Instance.PlayObjectPlace(transform.position);
        }
    }
}
