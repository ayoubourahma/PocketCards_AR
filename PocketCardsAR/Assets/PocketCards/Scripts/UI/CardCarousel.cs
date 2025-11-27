using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // Needed for DOTween
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class CardCarousel : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Animation Settings")]
    public float snapDuration = 0.3f;
    public Ease snapEase = Ease.OutBack;
    public float swipeVelocityThreshold = 800f;

    [Header("Dots Settings")]
    public Transform dotsContainer;
    public GameObject dotPrefab;
    public Color activeDotColor = new Color32(50, 50, 50, 255);
    public Color inactiveDotColor = new Color32(200, 200, 200, 255);

    [Header("References")]
    public HorizontalLayoutGroup layoutGroup;

    private ScrollRect _scrollRect;
    private RectTransform _content;

    private float _cardWidthOnly;
    private float _itemWidth;
    private int _originalCount;
    private int _totalCount;
    private List<Image> _dots = new List<Image>();
    private bool _isDragging;

    private void Start()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _content = _scrollRect.content;

        _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        _scrollRect.inertia = true;
        _scrollRect.decelerationRate = 0.135f;

        StartCoroutine(InitializeCarousel());
    }

    private IEnumerator InitializeCarousel()
    {
        yield return new WaitForEndOfFrame();

        _originalCount = _content.childCount;
        if (_originalCount == 0) yield break;

        SetupDots();

        // 1. Calculate Sizes
        RectTransform firstChild = _content.GetChild(0) as RectTransform;
        _cardWidthOnly = firstChild.rect.width;
        // Important: Include spacing so snapping is accurate
        _itemWidth = _cardWidthOnly + layoutGroup.spacing;

        // 2. Create Clones
        for (int i = _originalCount - 1; i >= 0; i--)
            Instantiate(_content.GetChild(i), _content).transform.SetAsFirstSibling();

        for (int i = 0; i < _originalCount; i++)
            Instantiate(_content.GetChild(_originalCount + i), _content).transform.SetAsLastSibling();

        _totalCount = _content.childCount;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

        // 3. Jump to the first Real Card
        JumpToCard(_originalCount, false);
    }

    private void SetupDots()
    {
        if (dotsContainer == null || dotPrefab == null) return;
        foreach (Transform t in dotsContainer) Destroy(t.gameObject);
        _dots.Clear();
        for (int i = 0; i < _originalCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsContainer);
            _dots.Add(dot.GetComponent<Image>());
        }
    }

    private void Update()
    {
        if (_totalCount == 0) return;

        HandleInfiniteLoop();
        UpdateDots();
    }

    private void HandleInfiniteLoop()
    {
        float currentPos = _content.anchoredPosition.x;
        float offset = GetCenterOffset();

        float rawIndex = -(currentPos - offset) / _itemWidth;

        if (rawIndex < _originalCount - 0.5f)
        {
            float shift = _originalCount * _itemWidth;
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x - shift, _content.anchoredPosition.y);
        }
        else if (rawIndex > (_originalCount * 2) - 0.5f)
        {
            float shift = _originalCount * _itemWidth;
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x + shift, _content.anchoredPosition.y);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _content.DOKill();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        float velocity = _scrollRect.velocity.x;
        float currentPos = _content.anchoredPosition.x;
        float offset = GetCenterOffset();

        int nearestIndex = Mathf.RoundToInt(-(currentPos - offset) / _itemWidth);

        if (Mathf.Abs(velocity) > swipeVelocityThreshold)
        {
            if (velocity < 0) nearestIndex++;
            else nearestIndex--;
        }

        JumpToCard(nearestIndex, true);
    }

    private void JumpToCard(int index, bool animate)
    {
        float offset = GetCenterOffset();
        float targetX = -(index * _itemWidth) + offset;

        _scrollRect.velocity = Vector2.zero;

        if (animate)
            _content.DOAnchorPosX(targetX, snapDuration).SetEase(snapEase);
        else
            _content.anchoredPosition = new Vector2(targetX, _content.anchoredPosition.y);
    }

    private float GetCenterOffset()
    {
        float viewportWidth = _scrollRect.viewport.rect.width;
        // Center the card relative to viewport
        return (viewportWidth / 2f) - (_cardWidthOnly / 2f);
    }

    private void UpdateDots()
    {
        if (_dots.Count == 0) return;
        float currentPos = _content.anchoredPosition.x;
        float offset = GetCenterOffset();
        int rawIndex = Mathf.RoundToInt(-(currentPos - offset) / _itemWidth);
        int realIndex = (rawIndex % _originalCount + _originalCount) % _originalCount;

        for (int i = 0; i < _dots.Count; i++)
        {
            _dots[i].color = (i == realIndex) ? activeDotColor : inactiveDotColor;
            float scale = (i == realIndex) ? 1.2f : 1f;
            _dots[i].rectTransform.localScale = Vector3.one * scale;
        }
    }

}