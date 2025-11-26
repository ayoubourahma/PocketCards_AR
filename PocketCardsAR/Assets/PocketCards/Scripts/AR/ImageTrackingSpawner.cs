using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingSpawner : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Prefab to Spawn")]
    public GameObject prefab;

    [Header("Smoothing Settings")]
    [Range(0f, 1f)] public float positionSmooth = 0.15f;
    [Range(0f, 1f)] public float rotationSmooth = 0.15f;

    private GameObject spawnedObject;

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // When AR Foundation detects the target image for the first time
        foreach (var added in args.added)
        {
            SpawnOrEnable(added);
        }

        // When tracking quality changes (tracking / limited / none)
        foreach (var updated in args.updated)
        {
            if (updated.trackingState == TrackingState.Tracking)
            {
                SpawnOrEnable(updated);
            }
            else
            {
                DestroySpawned();
            }
        }

        // When AR Foundation fully removes the tracked image
        foreach (var removed in args.removed)
        {
            DestroySpawned();
        }
    }

    private void SpawnOrEnable(ARTrackedImage trackedImage)
    {
        if (spawnedObject == null)
        {
            spawnedObject = Instantiate(prefab, trackedImage.transform.position, trackedImage.transform.rotation);
        }
        else
        {
            spawnedObject.SetActive(true);
        }

        // Smoothly follow image (jitter fix)
        StartCoroutine(SmoothFollow(trackedImage));
    }

    private void DestroySpawned()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }

    private System.Collections.IEnumerator SmoothFollow(ARTrackedImage trackedImage)
    {
        while (spawnedObject != null && trackedImage != null &&
               trackedImage.trackingState == TrackingState.Tracking)
        {
            // Smooth Position
            spawnedObject.transform.position =
                Vector3.Lerp(
                    spawnedObject.transform.position,
                    trackedImage.transform.position,
                    1f - Mathf.Pow(1 - positionSmooth, Time.deltaTime * 60)
                );

            // Smooth Rotation
            spawnedObject.transform.rotation =
                Quaternion.Slerp(
                    spawnedObject.transform.rotation,
                    trackedImage.transform.rotation,
                    1f - Mathf.Pow(1 - rotationSmooth, Time.deltaTime * 60)
                );

            yield return null;
        }
    }
}
