using UnityEngine;

public class CardTilt : MonoBehaviour
{
    public RectTransform viewport;

    [Header("Tilt Settings")]
    public float maxTilt = 25f;
    public float tiltDistance = 400f;

    [Header("Scale Settings")]
    public float centerScale = 1.2f; // Size when in the middle (Pop up)
    public float sideScale = 0.8f;   // Size when far away

    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (viewport == null) return;

        // 1. Calculate Position relative to Viewport Center
        Vector3 worldPos = rect.position;
        Vector3 viewPos = viewport.InverseTransformPoint(worldPos);

        float centerX = viewport.rect.width / 2f;
        float dist = viewPos.x - centerX;

        // 2. Apply Tilt (Your original logic)
        float tTilt = Mathf.Clamp(dist / tiltDistance, -1f, 1f);
        float angle = -tTilt * maxTilt;
        rect.localRotation = Quaternion.Euler(0, 0, angle);

        // 3. Apply Scaling (New Logic)
        // We use absolute distance because scaling is the same left or right
        float absDist = Mathf.Abs(dist);

        // Normalize: 0 = Exact Center, 1 = Far Edge
        float tScale = Mathf.Clamp01(absDist / tiltDistance);

        // Smoothly blend between Center Scale and Side Scale
        float finalScale = Mathf.Lerp(centerScale, sideScale, tScale);

        rect.localScale = Vector3.one * finalScale;
    }
}