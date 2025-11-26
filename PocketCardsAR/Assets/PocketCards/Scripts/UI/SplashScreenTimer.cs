using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SplashScreenTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float delay = 3f;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Target Object")]
    public GameObject targetObject;
    private Image splashImage;

    private void Start()
    {
        splashImage = GetComponent<Image>();
        if (splashImage == null)
        {
            Debug.LogError("No Image component found on this GameObject!");
            return;
        }
        // Start the timer
        Invoke(nameof(StartFade), delay);
    }

    private void StartFade()
    {
        if (targetObject != null)
            targetObject.SetActive(true);

        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color startColor = splashImage.color;

        // Fade the splash image out
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            splashImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Fully transparent
        splashImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        // Disable the splash screen GameObject after the animation
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
