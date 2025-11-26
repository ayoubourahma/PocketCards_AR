using UnityEngine;

public class CardTilt : MonoBehaviour
{
    public RectTransform viewport;
    public float maxTilt = 25f;
    public float tiltDistance = 400f;

    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        Vector3 worldPos = rect.position;
        Vector3 viewPos = viewport.InverseTransformPoint(worldPos);

        float centerX = viewport.rect.width / 2f;
        float dist = viewPos.x - centerX;

        float t = Mathf.Clamp(dist / tiltDistance, -1f, 1f);

        float angle = -t * maxTilt;

        rect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}