using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class OnBoardingHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [System.Serializable]
    public class OnboardingPage
    {
        public string title;
        [TextArea(2, 5)] public string description;
        public Sprite image;
    }

    private const string OnboardingDoneKey = "OnboardingDone"; // 🔥 PlayerPrefs KEY

    [Header("UI References")]
    public TMP_Text titleTMP;
    public TMP_Text descriptionTMP;
    public Image displayImage;
    public List<Image> scrollDots;

    [Header("Buttons")]
    public Button skipButton;
    public GameObject signInScreen;
    public GameObject onBordingScreen;

    [Header("Pages Data")]
    public List<OnboardingPage> pages = new List<OnboardingPage>();

    [Header("Settings")]
    public float slideDistance = 500f;
    public float animationDuration = 0.5f;
    public Color activeDotColor = Color.black;
    public Color inactiveDotColor = Color.gray;

    [Header("Swipe Settings")]
    public float swipeThreshold = 50f;
    public float swipeSensitivity = 0.5f;

    private int currentIndex = 0;
    private bool isAnimating = false;
    private Vector2 dragStartPosition;
    private Vector2 currentDragPosition;
    private bool isDragging = false;

    private void Start()
    {
        // 🔥 Skip onboarding if already done
        if (PlayerPrefs.GetInt(OnboardingDoneKey, 0) == 1)
        {
            onBordingScreen.SetActive(false);
            signInScreen.SetActive(true);
            return;
        }

        if (pages.Count == 0)
        {
            GoToSignIn();
            return;
        }

        UpdateUIInstant();
        UpdateDots();

        if (skipButton != null) skipButton.onClick.AddListener(GoToSignIn);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        dragStartPosition = eventData.position;
        currentDragPosition = dragStartPosition;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isAnimating) return;

        currentDragPosition = eventData.position;
        Vector2 dragDelta = currentDragPosition - dragStartPosition;

        UpdateDragVisuals(dragDelta.x);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isAnimating) return;

        Vector2 dragDelta = currentDragPosition - dragStartPosition;

        ResetDragVisuals();

        if (Mathf.Abs(dragDelta.x) > swipeThreshold)
        {
            if (dragDelta.x < 0)
            {
                NextPage();
            }
            else
            {
                PreviousPage();
            }
        }

        isDragging = false;
    }

    private void UpdateDragVisuals(float dragDeltaX)
    {
        float normalizedDrag = Mathf.Clamp(dragDeltaX / (Screen.width * swipeSensitivity), -1f, 1f);
    }

    private void ResetDragVisuals()
    {
        titleTMP.rectTransform.anchoredPosition = Vector2.zero;
        descriptionTMP.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void NextPage()
    {
        if (isAnimating) return;

        if (currentIndex >= pages.Count - 1)
        {
            GoToSignIn();
            return;
        }

        AnimateTransition(currentIndex + 1, Vector3.left);
    }

    public void PreviousPage()
    {
        if (isAnimating || currentIndex <= 0) return;
        AnimateTransition(currentIndex - 1, Vector3.right);
    }

    private void AnimateTransition(int nextIndex, Vector3 direction)
    {
        isAnimating = true;

        Sequence seq = DOTween.Sequence();

        seq.Append(titleTMP.rectTransform.DOAnchorPos(direction * slideDistance, animationDuration).SetEase(Ease.OutQuad));
        seq.Join(descriptionTMP.rectTransform.DOAnchorPos(direction * slideDistance, animationDuration).SetEase(Ease.OutQuad));
        seq.Join(displayImage.rectTransform.DOAnchorPos(direction * slideDistance, animationDuration).SetEase(Ease.OutQuad));
        seq.Join(displayImage.DOFade(0f, animationDuration).SetEase(Ease.OutQuad));

        seq.AppendCallback(() =>
        {
            titleTMP.rectTransform.anchoredPosition = -direction * slideDistance;
            descriptionTMP.rectTransform.anchoredPosition = -direction * slideDistance;
            displayImage.rectTransform.anchoredPosition = -direction * slideDistance;

            currentIndex = nextIndex;
            titleTMP.text = pages[currentIndex].title;
            descriptionTMP.text = pages[currentIndex].description;

            if (displayImage != null)
            {
                displayImage.sprite = pages[currentIndex].image;
                Color color = displayImage.color;
                color.a = 0f;
                displayImage.color = color;
            }

            UpdateDots();
        });

        seq.Append(titleTMP.rectTransform.DOAnchorPos(Vector2.zero, animationDuration).SetEase(Ease.OutQuad));
        seq.Join(descriptionTMP.rectTransform.DOAnchorPos(Vector2.zero, animationDuration).SetEase(Ease.OutQuad));
        seq.Join(displayImage.rectTransform.DOAnchorPos(Vector2.zero, animationDuration).SetEase(Ease.OutQuad));
        seq.Join(displayImage.DOFade(1f, animationDuration).SetEase(Ease.OutQuad));

        seq.OnComplete(() => isAnimating = false);
    }

    private void UpdateUIInstant()
    {
        titleTMP.text = pages[currentIndex].title;
        descriptionTMP.text = pages[currentIndex].description;
        if (displayImage != null)
        {
            displayImage.sprite = pages[currentIndex].image;
            Color color = displayImage.color;
            color.a = 1f;
            displayImage.color = color;
        }
    }

    private void UpdateDots()
    {
        for (int i = 0; i < scrollDots.Count; i++)
        {
            if (i < pages.Count)
            {
                scrollDots[i].color = (i == currentIndex) ? activeDotColor : inactiveDotColor;
                scrollDots[i].gameObject.SetActive(true);
            }
            else
            {
                scrollDots[i].gameObject.SetActive(false);
            }
        }
    }

    public void GoToSignIn()
    {
        // 🔥 Save onboarding complete
        PlayerPrefs.SetInt(OnboardingDoneKey, 1);
        PlayerPrefs.Save();

        onBordingScreen.SetActive(false);

        if (signInScreen != null)
            signInScreen.SetActive(true);
    }

    private void Update()
    {
        if (isAnimating) return;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPage();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPage();
        }
#endif
    }
}
