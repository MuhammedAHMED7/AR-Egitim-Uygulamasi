using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TapToPlace : MonoBehaviour
{
    public GameObject cellPrefab; // This is where we will put your 3D cell
    private ARRaycastManager raycastManager;
    private GameObject spawnedObject;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // Check if the user touched the screen
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            // Shoot a raycast to see if they tapped on an AR plane
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                // If the cell hasn't been spawned yet, spawn it!
                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(cellPrefab, hitPose.position, hitPose.rotation);
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
