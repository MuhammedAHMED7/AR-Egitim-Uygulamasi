using UnityEngine;

namespace Core.Management
{
    /// <summary>
    /// Uygulamanýn yaþam döngüsünü baþlatan, kalýcýlýðýný saðlayan ana yönetici sýnýf.
    /// </summary>
    [RequireComponent(typeof(SceneController))] // AppCore olan yerde SceneController olmak zorundadýr.
    public class AppCore : MonoBehaviour
    {
        public static AppCore Instance { get; private set; }

        [Header("Scene Settings")]
        [SerializeField] private string mainMenuSceneName = "Scene_MainMenu";

        private SceneController sceneController;

        private void Awake()
        {
            InitializeSingleton();
            sceneController = GetComponent<SceneController>();
        }

        private void Start()
        {
            // Uygulama açýlýr açýlmaz patron, kontrolöre ilk menüyü yükleme emri veriyor
            sceneController.LoadInitialMenu(mainMenuSceneName);
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // --- Dýþarýdan (Menü butonlarýndan vs.) Çaðrýlacak Ana Fonksiyonlar ---

        public void ChangeToEducationalScene(string sceneName)
        {
            sceneController.LoadEducationalScene(sceneName);
        }

        public void ReturnToMainMenu()
        {
            sceneController.UnloadCurrentScene();
        }
    }
}