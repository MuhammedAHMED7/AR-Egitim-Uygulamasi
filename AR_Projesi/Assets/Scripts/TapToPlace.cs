using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class TapToPlace : MonoBehaviour
{
    public GameObject objectToSpawn; // This will hold your Cell Prefab
    private GameObject spawnedObject;
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // Check if the user is touching the screen
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Only run the code exactly when the finger first touches the screen
            if (touch.phase == TouchPhase.Began)
            {
                // Shoot a raycast from the touch position to any detected AR planes
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;

                    // If we haven't spawned the cell yet, spawn it
                    if (spawnedObject == null)
                    {
                        spawnedObject = Instantiate(objectToSpawn, hitPose.position, hitPose.rotation);
                    }
                    // If it already exists, just move it to the new tap location
                    else
                    {
                        spawnedObject.transform.position = hitPose.position;
                    }
                }
            }
        }
    }
}
