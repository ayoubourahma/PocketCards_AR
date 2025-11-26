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

    private Dictionary<string, ARAnchor> anchors = new Dictionary<string, ARAnchor>();
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
        foreach (var added in args.added)
            HandleImageFound(added);

        foreach (var updated in args.updated)
        {
            if (updated.trackingState == TrackingState.Tracking)
                HandleImageUpdated(updated);
            else
                HandleImageLost(updated.referenceImage.name);
        }

        foreach (var removed in args.removed)
            HandleImageLost(removed.referenceImage.name);
    }


    // ----------------------------------------------------------
    // Create anchor + model once
    // ----------------------------------------------------------
    private void HandleImageFound(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        Debug.Log("TRACKING FOUND: " + imageName);

        if (anchors.ContainsKey(imageName))
            return;

        // Create anchor
        GameObject anchorObj = new GameObject($"Anchor_{imageName}");
        anchorObj.transform.position = trackedImage.transform.position;
        anchorObj.transform.rotation = trackedImage.transform.rotation;

        ARAnchor anchor = anchorObj.AddComponent<ARAnchor>();

        if (anchor == null)
        {
            Debug.LogError("FAILED TO CREATE ANCHOR for " + imageName);
            return;
        }
        Debug.Log("ANCHOR CREATED for: " + imageName);

        anchors[imageName] = anchor;

        // Spawn model
        GameObject prefab = GetPrefabForImage(imageName);
        if (prefab == null)
        {
            Debug.LogWarning("NO PREFAB for " + imageName);
            return;
        }

        GameObject model = Instantiate(prefab, anchor.transform);
        model.transform.localScale = Vector3.one * 0.2f;  // force visible scale

        spawnedModels[imageName] = model;

        Debug.Log("MODEL SPAWNED for " + imageName);
    }



    // ----------------------------------------------------------
    // Update model to follow tracked image (NOT anchor!)
    // ----------------------------------------------------------
    private void HandleImageUpdated(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (!spawnedModels.ContainsKey(imageName))
            return;

        GameObject model = spawnedModels[imageName];

        // Update ONLY the model, NOT the anchor
        model.transform.SetPositionAndRotation(
            trackedImage.transform.position,
            trackedImage.transform.rotation
        );
    }


    // ----------------------------------------------------------
    // Destroy anchor + model
    // ----------------------------------------------------------
    private void HandleImageLost(string imageName)
    {
        if (spawnedModels.ContainsKey(imageName))
        {
            Destroy(spawnedModels[imageName]);
            spawnedModels.Remove(imageName);
        }

        if (anchors.ContainsKey(imageName))
        {
            Destroy(anchors[imageName].gameObject);
            anchors.Remove(imageName);
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
