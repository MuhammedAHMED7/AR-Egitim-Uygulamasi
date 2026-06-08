using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Core.Management
{
    /// <summary>
    /// Sahnelerin Single ve Additive olarak yüklenmesinden ve hafýza yönetiminden sorumlu alt sistem.
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        private string activeEducationalScene = "";

        /// <summary>
        /// Proje ilk açýldýðýnda Ana Menüyü güvenli bir þekilde yükler.
        /// </summary>
        public void LoadInitialMenu(string menuSceneName)
        {
            SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Peri veya Þuca'nýn hazýrladýðý bir eðitim sahnesini (Örn: Mars veya Kalp) Additive olarak yükler.
        /// </summary>
        public void LoadEducationalScene(string sceneName)
        {
            // Eðer sahne zaten yüklüyse veya geçiþ aþamasýndaysa iþlemi durdur (Güvenlik Önlemi)
            if (SceneManager.GetSceneByName(sceneName).isLoaded) return;

            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            // Sahneyi arka planda mevcut sistemleri bozmadan Additive (Katmanlý) olarak yükle
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            // Sahne tamamen yüklenene kadar oyunun donmasýný engelle, arka planda yükle
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Yeni yüklenen sahneyi Unity'ye "Aktif Sahne" olarak bildir.
            // Bu sayede Peri veya Ahmet bir obje instantiate ettiðinde doðrudan bu sahneye doðar.
            Scene newlyLoadedScene = SceneManager.GetSceneByName(sceneName);
            if (newlyLoadedScene.IsValid())
            {
                SceneManager.SetActiveScene(newlyLoadedScene);
                activeEducationalScene = sceneName;
            }
        }

        /// <summary>
        /// Mevcut eðitim sahnesini kapatýr ve hafýzayý temizleyerek ana menüyü yeniden aktif eder.
        /// </summary>
        public void UnloadCurrentScene()
        {
            if (string.IsNullOrEmpty(activeEducationalScene)) return;

            // Sahneyi ve içindeki objeleri hafýzadan tamamen silerek optimizasyon saðlar
            SceneManager.UnloadSceneAsync(activeEducationalScene);
            activeEducationalScene = "";
        }
    }
}