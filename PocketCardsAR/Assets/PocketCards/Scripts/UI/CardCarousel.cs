using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class CardCarousel : MonoBehaviour
{
    [Header("UI")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport;

    [Header("Dots")]
    public Transform dotsContainer;
    public GameObject dotPrefab;
    public float dotActiveScale = 1.4f;

    [Header("Animation")]
    public float snapDuration = 0.35f;
    public Ease snapEase = Ease.OutCubic;

    private int pageCount;
    private int currentPage = 0;
    private float pageWidth;

    private List<RectTransform> dots = new List<RectTransform>();
    private bool isDragging = false;

    void Start()
    {
        InitPages();
        CreateDots();
        UpdateDots();
    }

    void InitPages()
    {
        pageCount = content.childCount;

        // Each card width = viewport width
        pageWidth = viewport.rect.width;
    }

    void CreateDots()
    {
        foreach (Transform child in dotsContainer)
            Destroy(child.gameObject);

        dots.Clear();

        for (int i = 0; i < pageCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsContainer);
            dot.SetActive(true);
            dots.Add(dot.GetComponent<RectTransform>());
        }
    }

    void Update()
    {
        if (isDragging) return;

        // If swipe released, snap
        if (Input.GetMouseButtonUp(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            SnapToNearestPage();
        }
    }

    public void OnBeginDrag()
    {
        isDragging = true;
    }

    public void OnEndDrag()
    {
        isDragging = false;
        SnapToNearestPage();
    }

    void SnapToNearestPage()
    {
        float pos = content.anchoredPosition.x;

        int targetPage = Mathf.RoundToInt(-pos / pageWidth);
        targetPage = Mathf.Clamp(targetPage, 0, pageCount - 1);

        GoToPage(targetPage);
    }

    public void GoToPage(int page)
    {
        currentPage = page;

        float targetX = -page * pageWidth;

        content.DOAnchorPosX(targetX, snapDuration)
               .SetEase(snapEase);

        UpdateDots();
    }

    void UpdateDots()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            if (i == currentPage)
                dots[i].DOScale(dotActiveScale, 0.2f);
            else
                dots[i].DOScale(1f, 0.2f);
        }
    }
}
