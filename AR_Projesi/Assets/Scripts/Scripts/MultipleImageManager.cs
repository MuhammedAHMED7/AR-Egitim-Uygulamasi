using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class MultipleImageManager : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    // Her marker için prefab
    public GameObject dersPrefab;
    public GameObject kameraPrefab;
    public GameObject profilPrefab;

    // Marker ile eþleþen instantiation objeleri
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()

    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Yeni markerlar
        foreach (var trackedImage in eventArgs.added)
        {
            AssignPrefab(trackedImage);
        }

        // Marker güncellenince
        foreach (var trackedImage in eventArgs.updated)
        {
            AssignPrefab(trackedImage);
        }

        // Marker kaybolunca
        foreach (var trackedImage in eventArgs.removed)
        {
            if (spawnedObjects.ContainsKey(trackedImage.referenceImage.name))
            {
                Destroy(spawnedObjects[trackedImage.referenceImage.name]);
                spawnedObjects.Remove(trackedImage.referenceImage.name);
            }
        }
    }

    void AssignPrefab(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (!spawnedObjects.ContainsKey(name))
        {
            GameObject prefabToSpawn = null;

            if (name == "marker_ders") prefabToSpawn = dersPrefab;
            else if (name == "marker_kamera") prefabToSpawn = kameraPrefab;
            else if (name == "marker_profil") prefabToSpawn = profilPrefab;

            if (prefabToSpawn != null)
            {
                GameObject spawned = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
                spawnedObjects[name] = spawned;
            }
        }
        else
        {
            // Marker var, pozisyon ve rotasyonu güncelle
            spawnedObjects[name].transform.position = trackedImage.transform.position;
            spawnedObjects[name].transform.rotation = trackedImage.transform.rotation;
        }
    }
}