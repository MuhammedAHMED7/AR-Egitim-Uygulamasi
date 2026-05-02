using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Zemin üzerindeki hedefleyici halkayı yönetir.
/// Reticle prefabına ekle.
/// Prefab: düz bir halka Mesh + Mesh Renderer
/// </summary>
public class ReticleController : MonoBehaviour
{
    [Header("AR")]
    public ARRaycastManager raycastManager;

    [Header("Renk")]
    public Color colorFound    = new Color(0f, 0.82f, 0.47f, 0.8f);
    public Color colorNotFound = new Color(1f, 0.3f, 0.3f, 0.5f);

    [Header("Döndürme Hızı")]
    public float rotationSpeed = 45f;

    private List<ARRaycastHit> _hits     = new List<ARRaycastHit>();
    private Renderer           _renderer;

    public bool IsVisible { get; private set; }

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        DetectPlane();
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    void DetectPlane()
    {
        if (!raycastManager) return;

        var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        if (raycastManager.Raycast(center, _hits, TrackableType.PlaneWithinPolygon))
        {
            IsVisible = true;
            gameObject.SetActive(true);

            var pose = _hits[0].pose;
            transform.position = pose.position;
            // Sadece Y eksenini al, halkayı düz tut
            transform.rotation = Quaternion.Euler(
                0f,
                transform.rotation.eulerAngles.y,
                0f);

            if (_renderer) _renderer.material.color = colorFound;
        }
        else
        {
            IsVisible = false;
            gameObject.SetActive(false);
        }
    }
}
