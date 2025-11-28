using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultiImageAnchorSpawner : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Image → Prefab Mapping")]
    public List<ImagePrefabMapping> imagePrefabMappings;

    private Dictionary<string, GameObject> spawnedModels = new Dictionary<string, GameObject>();

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
        // Handle newly detected images
        foreach (var added in args.added)
            HandleImageAdded(added);

        // Handle updates to existing tracked images
        foreach (var updated in args.updated)
            HandleImageUpdated(updated);

        // Handle removed images
        foreach (var removed in args.removed)
            HandleImageLost(removed.referenceImage.name);
    }

    private void HandleImageAdded(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        Debug.Log($"IMAGE ADDED: {imageName}");

        // Only create if tracking
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            CreateModel(trackedImage);
        }
    }

    private void HandleImageUpdated(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Create model if it doesn't exist (handles re-detection)
            if (!spawnedModels.ContainsKey(imageName))
            {
                CreateModel(trackedImage);
            }
            else
            {
                // Update existing model position
                GameObject model = spawnedModels[imageName];
                model.transform.SetPositionAndRotation(
                    trackedImage.transform.position,
                    trackedImage.transform.rotation
                );
            }
        }
        else
        {
            // Tracking lost - destroy the model
            HandleImageLost(imageName);
        }
    }

    private void CreateModel(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (spawnedModels.ContainsKey(imageName))
            return;

        GameObject prefab = GetPrefabForImage(imageName);
        if (prefab == null)
        {
            Debug.LogWarning($"NO PREFAB for {imageName}");
            return;
        }

        GameObject model = Instantiate(prefab, trackedImage.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        //model.transform.localScale = Vector3.one * 0.2f;

        spawnedModels[imageName] = model;
        Debug.Log($"MODEL SPAWNED for {imageName}");
    }

    private void HandleImageLost(string imageName)
    {
        if (spawnedModels.ContainsKey(imageName))
        {
            Debug.Log($"DESTROYING MODEL for {imageName}");
            Destroy(spawnedModels[imageName]);
            spawnedModels.Remove(imageName);
        }
    }

    private GameObject GetPrefabForImage(string imageName)
    {
        foreach (var pair in imagePrefabMappings)
            if (pair.imageName == imageName)
                return pair.prefab;

        return null;
    }
}

[System.Serializable]
public class ImagePrefabMapping
{
    public string imageName;
    public GameObject prefab;
}