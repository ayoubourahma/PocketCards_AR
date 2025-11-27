using UnityEngine;
using UnityEngine.UI; // Required for RawImage
using TMPro;

public class SearchResultItem : MonoBehaviour
{
    [Header("UI References")]
    // Changed from Image to RawImage
    public RawImage thumbnailImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI categoryBadgeText;

    private ImageItem _data;

    public void Setup(ImageItem item, string categoryName)
    {
        _data = item;

        // 1. Directly assign the texture (No Sprite.Create needed)
        if (thumbnailImage != null)
        {
            thumbnailImage.texture = item.texture;
        }

        // 2. Set the text
        if (titleText != null)
        {
            titleText.text = item.imageName;
        }

        // 3. Set the Category Badge
        if (categoryBadgeText != null)
        {
            categoryBadgeText.text = categoryName;
        }
    }

    public void OnClick()
    {
        Debug.Log($"Clicked on {_data.imageName}");
        // Add logic here to open the details view
    }
}