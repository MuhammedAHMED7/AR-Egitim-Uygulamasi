using UnityEngine;

public class PlaySoundOnClick : MonoBehaviour
{
    public AudioSource audioSource;

    void OnMouseDown()
    {
        audioSource.Play();
    }
}