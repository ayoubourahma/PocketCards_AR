using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CarouselSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler

{
   [Header("Setup")]
    [SerializeField] RectTransform content;
    [SerializeField] float snapSpeed = 15f;  // Higher = snappier
    [SerializeField] float threshold = 0.1f; // Drag distance to snap next

    [Header("Cards")]
    [SerializeField] int cardCount = 4; // Your 4 cards

    ScrollRect scrollRect;
    HorizontalLayoutGroup layoutGroup;
    float itemWidth;
    bool isDragging = false;
    float dragStartPos;
    Vector2 velocity;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        layoutGroup = content.GetComponent<HorizontalLayoutGroup>();
        if (cardCount == 0) cardCount = content.childCount;

        // Calc exact item width (card + spacing)
        RectTransform firstChild = content.GetChild(0) as RectTransform;
        itemWidth = firstChild.rect.width + layoutGroup.spacing;

        // Total width for N loops (prevents tiny content)
        content.sizeDelta = new Vector2(cardCount * 3f * itemWidth, content.sizeDelta.y); // 3x for smooth loop

        // Start centered
        scrollRect.horizontalNormalizedPosition = 0.5f;
    }

    // Capture drag start
    public void OnBeginDrag(PointerEventData eventData) => isDragging = true;

    // Capture velocity on end
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        velocity = scrollRect.velocity;
        StartCoroutine(SmoothSnap());
    }

    // Track drag for threshold
    public void OnDrag(PointerEventData eventData) { }

    IEnumerator SmoothSnap()
    {
        float startPos = scrollRect.horizontalNormalizedPosition;
        float targetIndex = Mathf.Round(startPos * (content.childCount - 1)); // Current snap index
        float dragDistance = Mathf.Abs(startPos - dragStartPos);

        // Threshold: small drag snaps nearest; big jumps next
        if (dragDistance > threshold || Mathf.Abs(velocity.x) > 500f)
            targetIndex += Mathf.Sign(velocity.x) * (itemWidth / content.rect.width);

        // Clamp to 0-3 (your 4 cards)
        int clampedIndex = Mathf.Clamp((int)targetIndex, 0, cardCount - 1);
        float targetPos = (float)clampedIndex / (cardCount - 1);

        // Smooth lerp with easing
        float elapsed = 0f;
        float duration = 0.4f; // Snap time
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPos, targetPos, t);
            yield return null;
        }
        scrollRect.horizontalNormalizedPosition = targetPos;

        // Infinite loop: Reposition content seamlessly
        LoopContent();
    }

    void LoopContent()
    {
        float currentX = content.anchoredPosition.x;

        // Shift left: move first cards to end
        if (currentX > itemWidth)
        {
            for (int i = 0; i < cardCount; i++)
            {
                RectTransform child = content.GetChild(i) as RectTransform;
                child.SetAsLastSibling();
                child.anchoredPosition += new Vector2(cardCount * itemWidth, 0);
            }
            content.anchoredPosition -= new Vector2(cardCount * itemWidth, 0);
        }
        // Shift right: move last to front
        else if (currentX < -itemWidth)
        {
            for (int i = 0; i < cardCount; i++)
            {
                RectTransform child = content.GetChild(content.childCount - 1 - i) as RectTransform;
                child.SetAsFirstSibling();
                child.anchoredPosition -= new Vector2(cardCount * itemWidth, 0);
            }
            content.anchoredPosition += new Vector2(cardCount * itemWidth, 0);
        }
    }

    void LateUpdate()
    {
        if (!isDragging) return;
        dragStartPos = scrollRect.horizontalNormalizedPosition;
    }
}