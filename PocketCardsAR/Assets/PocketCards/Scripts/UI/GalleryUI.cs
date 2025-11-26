using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryUI : MonoBehaviour
{
    [Header("Database")]
    public GalleryDatabase database;

    [Header("UI References")]
    public GameObject categoryScreen;
    public TMP_Text categoryTitle;
    public Transform gridParent;
    public GameObject itemPrefab;

    // Called by category buttons
    public void OpenCategory(int index)
    {
        // Open screen
        categoryScreen.SetActive(true);

        // Set title
        categoryTitle.text = database.categories[index].categoryName;

        // Clear old grid content
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        // Spawn new items
        foreach (var img in database.categories[index].images)
        {
            GameObject go = Instantiate(itemPrefab, gridParent);
            var ui = go.GetComponent<CategoryItemUI>();
            if (!img.isLocked)
            {
                ui.lockImage.gameObject.SetActive(false);
            }
            ui.rawImage.texture = img.texture;
            ui.title.text = img.imageName;
        }
    }
}