using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class AudioUIButtonHelper : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // Fare veya parmak butonun üzerine geldiğinde (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && _button != null && _button.interactable)
        {
            AudioManager.Instance.PlayUIHover();
        }
    }

    // Butona tıklandığı (Dokunulduğu) anda (Click)
    public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && _button != null && _button.interactable)
        {
            AudioManager.Instance.PlayUIClick();
        }
    }
}
