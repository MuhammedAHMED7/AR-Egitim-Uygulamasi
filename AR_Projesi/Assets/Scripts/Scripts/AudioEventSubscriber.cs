using UnityEngine;

public class AudioEventSubscriber : MonoBehaviour
{
    private void OnEnable()
    {
        // Sahne yüklendiğinde ve bu bileşen aktif olduğunda olaylara (event) abone oluruz.
        
        // ----------------------------------------------------------------------------------
        // [ÖNEMLİ NOT]: Arkadaşlarınız kendi ağ ve görev kodlarını (ARMultiplayerManager ve 
        // ARCollaborativeTaskManager) projeye eklediğinde, aşağıdaki kodların başındaki 
        // iki eğik çizgiyi (//) silerek aktif hale getirebilirsiniz. Şimdilik hata vermemesi 
        // için yorum satırı yapılmıştır.
        // ----------------------------------------------------------------------------------
        
        /*
        if (ARMultiplayerManager.Instance != null)
        {
            ARMultiplayerManager.Instance.OnPlayerJoined += HandlePlayerJoined;
        }

        if (ARCollaborativeTaskManager.Instance != null)
        {
            ARCollaborativeTaskManager.Instance.OnTaskCompleted += HandleTaskCompleted;
        }
        */
    }

    private void OnDisable()
    {
        /*
        if (ARMultiplayerManager.Instance != null)
        {
            ARMultiplayerManager.Instance.OnPlayerJoined -= HandlePlayerJoined;
        }

        if (ARCollaborativeTaskManager.Instance != null)
        {
            ARCollaborativeTaskManager.Instance.OnTaskCompleted -= HandleTaskCompleted;
        }
        */
    }

    // --- Olay Yöneticileri (Event Handlers) ---

    private void HandlePlayerJoined(string playerName)
    {
        Debug.Log($"[Ses] Yeni oyuncu katıldı: {playerName}. Giriş sesi çalınıyor.");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerJoined(Camera.main.transform.position); 
        }
    }

    private void HandleTaskCompleted(object task)
    {
        Debug.Log("[Ses] Görev başarıyla tamamlandı! Tebrik sesi çalınıyor.");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySuccess();
        }
    }
}
