using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultiImageTrackingManager : MonoBehaviour
{
    public GameObject dersPrefab;
    public GameObject kameraPrefab;
    public GameObject profilPrefab;

    private ARTrackedImageManager trackedImageManager;
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
        foreach (var trackedImage in eventArgs.added)
        {
            SpawnObject(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateObject(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            RemoveObject(trackedImage);
        }
    }

    void SpawnObject(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        GameObject prefabToSpawn = null;

        switch (imageName)
        {
            case "marker_ders":
                prefabToSpawn = dersPrefab;
                break;

            case "marker_kamera":
                prefabToSpawn = kameraPrefab;
                break;

            case "marker_profil":
                prefabToSpawn = profilPrefab;
                break;
        }

        if (prefabToSpawn != null && !spawnedObjects.ContainsKey(imageName))
        {
            GameObject obj = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
            obj.transform.parent = trackedImage.transform;
            spawnedObjects.Add(imageName, obj);
        }
    }

    void UpdateObject(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedObjects.ContainsKey(imageName))
        {
            spawnedObjects[imageName].SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }
    }

    void RemoveObject(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedObjects.ContainsKey(imageName))
        {
            Destroy(spawnedObjects[imageName]);
            spawnedObjects.Remove(imageName);
        }
    }
}