using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;


public class AnimationClickTrigger : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    public int maxSteps = 4;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip introVoice;
    public AudioClip[] stepVoices;

    [Header("UI")]
    public TMP_Text stepText;
    public GameObject dotPrefab;
    public RectTransform progressParent;

    [Header("Visuals")]
    public float dotSpacing = 30f;
    public float bigScale = 1.5f;
    public float smallScale = 1f;
    public float shiftDuration = 0.5f;
    public Color currentColor = Color.green;
    public Color defaultColor = Color.gray;

    [Header("Button Assignment")]
    public string targetButtonName = "AnimButton";
    public Button animButton;

    [Header("Finish UI")]
    public GameObject finishImage;   // FULLSCREEN IMAGE HERE

    private int visibleCount = 4;
    private int currentIndex = 0;
    private int totalSteps;
    private int startIndex = 0;
    private Image[] progressDots;
    private RectTransform[] dotSlotsRT;
    private bool canClick = false;

    void Start()
    {
        totalSteps = maxSteps;

        // Hide finish image at start
        if (finishImage != null)
            finishImage.SetActive(false);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (dotPrefab != null && progressParent != null)
        {
            progressDots = new Image[visibleCount];
            dotSlotsRT = new RectTransform[visibleCount];
            float halfSpacing = ((visibleCount - 1) * dotSpacing) / 2f;

            for (int i = 0; i < visibleCount; i++)
            {
                GameObject dotInstance = Instantiate(dotPrefab, progressParent);
                dotSlotsRT[i] = dotInstance.GetComponent<RectTransform>();
                dotSlotsRT[i].anchoredPosition = new Vector2(-halfSpacing + i * dotSpacing, 0);
                progressDots[i] = dotInstance.GetComponent<Image>();
                progressDots[i].color = defaultColor;
                dotInstance.transform.localScale = Vector3.one * smallScale;
            }
        }

        UpdateUI();
        AssignToButton();
        PlayIntroVoice();
    }

    private void AssignToButton()
    {
        if (animButton != null)
        {
            animButton.onClick.RemoveAllListeners();
            animButton.onClick.AddListener(AdvanceAnimation);
            animButton.interactable = false;
        }
    }

    private void SetStatus(string text)
    {
        if (stepText != null)
            stepText.text = text;
    }

    private void PlayIntroVoice()
    {
        if (introVoice != null)
        {
            SetStatus("Intro");
            audioSource.PlayOneShot(introVoice);
            StartCoroutine(WaitForAudioToFinish(introVoice.length));
        }
        else
        {
            canClick = true;
            if (animButton != null)
                animButton.interactable = true;

            SetStatus("Continue");
        }
    }

    private IEnumerator WaitForAudioToFinish(float duration)
    {
        yield return new WaitForSeconds(duration);

        canClick = true;
        if (animButton != null)
            animButton.interactable = true;

        SetStatus("Continue");
    }

    // ======================================================
    // MAIN LOGIC
    // ======================================================
    public void AdvanceAnimation()
    {
        if (!canClick) return;

        // LAST STEP 👉 show fullscreen UI
        if (currentIndex >= totalSteps - 1)
        {
            SetStatus("Finished");

            if (finishImage != null)
                finishImage.SetActive(true);

            return;
        }

        canClick = false;
        if (animButton != null)
            animButton.interactable = false;

        SetStatus("Playing...");

        if (animator != null)
            animator.SetTrigger("Next");

        AudioClip currentStepVoice = null;
        if (stepVoices != null && currentIndex < stepVoices.Length)
            currentStepVoice = stepVoices[currentIndex];

        currentIndex++;
        UpdateUI();

        StartCoroutine(WaitForAnimationAndVoice(currentStepVoice));
    }

    private IEnumerator WaitForAnimationAndVoice(AudioClip voiceClip)
    {
        float animationLength = GetCurrentAnimationLength();
        float voiceLength = 0f;

        if (voiceClip != null)
        {
            audioSource.PlayOneShot(voiceClip);
            voiceLength = voiceClip.length;
        }

        float waitTime = Mathf.Max(animationLength, voiceLength);
        yield return new WaitForSeconds(waitTime);

        canClick = true;
        if (animButton != null)
            animButton.interactable = true;

        if (currentIndex >= totalSteps)
            SetStatus("Finished");
        else
            SetStatus("Continue");
    }

    private float GetCurrentAnimationLength()
    {
        if (animator == null) return 0f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }

    // ======================================================
    // UI DOT LOGIC
    // ======================================================
    void UpdateUI()
    {
        ApplyDotStates();

        int position = currentIndex - startIndex;
        int shiftTrigger = visibleCount - 1;

        if (position >= shiftTrigger && startIndex + 1 < totalSteps)
        {
            StartCoroutine(ShiftDots(1));
        }
    }

    private IEnumerator ShiftDots(int amount)
    {
        Sequence seq = DOTween.Sequence();
        seq.SetEase(Ease.InOutQuad);

        for (int i = 0; i < visibleCount; i++)
        {
            if (progressDots[i].gameObject.activeSelf)
            {
                float targetX = dotSlotsRT[i].anchoredPosition.x - (dotSpacing * amount);
                seq.Join(dotSlotsRT[i].DOAnchorPosX(targetX, shiftDuration));
            }
        }

        yield return seq.WaitForCompletion();

        Image tempImage = progressDots[0];
        RectTransform tempRT = dotSlotsRT[0];

        for (int i = 0; i < visibleCount - 1; i++)
        {
            progressDots[i] = progressDots[i + 1];
            dotSlotsRT[i] = dotSlotsRT[i + 1];
        }

        progressDots[visibleCount - 1] = tempImage;
        dotSlotsRT[visibleCount - 1] = tempRT;

        startIndex++;
        RecenterDots();
        ApplyDotStates();
    }

public void ReloadScene()
{
    Scene current = SceneManager.GetActiveScene();
    SceneManager.LoadScene(current.name);
}


    private void RecenterDots()
    {
        int activeCount = Mathf.Min(visibleCount, totalSteps - startIndex);
        if (activeCount == 0) return;

        float halfSpacing = ((activeCount - 1) * dotSpacing) / 2f;
        int activeIdx = 0;

        for (int i = 0; i < visibleCount; i++)
        {
            int step = startIndex + i;
            if (step < totalSteps)
            {
                dotSlotsRT[i].anchoredPosition = new Vector2(-halfSpacing + activeIdx * dotSpacing, 0);
                activeIdx++;
            }
        }
    }

    private void ApplyDotStates()
    {
        for (int i = 0; i < visibleCount; i++)
        {
            int step = startIndex + i;

            if (step >= totalSteps)
            {
                if (progressDots[i].gameObject.activeSelf)
                    progressDots[i].gameObject.SetActive(false);
            }
            else
            {
                progressDots[i].gameObject.SetActive(true);
                bool isCurrent = (step == currentIndex);
                progressDots[i].color = isCurrent ? currentColor : defaultColor;

                Vector3 targetScale = isCurrent ? Vector3.one * bigScale : Vector3.one * smallScale;
                dotSlotsRT[i].DOScale(targetScale, 0.2f).SetEase(Ease.OutBack);
            }
        }
    }
}
