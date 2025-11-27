using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Important for searching
using DG.Tweening;

public class SearchManager : MonoBehaviour
{
    [Header("Data Source")]
    public GalleryDatabase database;

    [Header("UI Components")]
    public TMP_InputField searchInputField;
    public Transform resultsContainer; // The Content object of your ScrollView
    public GameObject resultItemPrefab; // The Prefab created in Step 1
    public GameObject searchPanel; // The entire white panel holding results (to hide/show)
    public GameObject overlayshadow;
    // A helper class to flatten the data structure
    private class SearchableEntry
    {
        public ImageItem Item;
        public string CategoryName;
    }

    private List<SearchableEntry> _allItems = new List<SearchableEntry>();
    private List<GameObject> _currentUIItems = new List<GameObject>();

    private void Start()
    {
        InitializeDatabase();

        // Listen for typing
        searchInputField.onValueChanged.AddListener(OnSearchInputValueChanged);

        // Start hidden
        if (searchPanel != null) searchPanel.SetActive(false);
        if(overlayshadow != null) overlayshadow.SetActive(false);
    }

    // 1. Flatten the database for easy searching
    private void InitializeDatabase()
    {
        _allItems.Clear();

        foreach (var cat in database.categories)
        {
            foreach (var img in cat.images)
            {                                                      
                _allItems.Add(new SearchableEntry 
                {
                    Item = img,
                    CategoryName = cat.categoryName
                });
            }
        }
    }

    // 2. Called every time the user types
    public void OnSearchInputValueChanged(string query)
    {
        // Should we show the panel?
        bool isTyping = !string.IsNullOrEmpty(query);
        if (searchPanel != null) searchPanel.SetActive(isTyping);
        if (overlayshadow != null) overlayshadow.SetActive(isTyping);

        if (!isTyping)
        {
            ClearResults();
            return;
        }

        FilterResults(query);
    }

    // 3. The Logic: Filter by Name OR Category
    private void FilterResults(string query)
    {
        query = query.ToLower();

        // LINQ Query: Find items where Name contains query OR Category contains query
        var filteredList = _allItems.Where(x =>
            x.Item.imageName.ToLower().Contains(query) ||
            x.CategoryName.ToLower().Contains(query)
        ).ToList();

        UpdateUI(filteredList);
    }

    // 4. Update the Visuals
    private void UpdateUI(List<SearchableEntry> results)
    {
        // Clear old items
        ClearResults();

        // Create new items
        foreach (var entry in results)
        {
            GameObject newObj = Instantiate(resultItemPrefab, resultsContainer);
            SearchResultItem uiScript = newObj.GetComponent<SearchResultItem>();

            if (uiScript != null)
            {
                uiScript.Setup(entry.Item, entry.CategoryName);
            }

            _currentUIItems.Add(newObj);
        }
    }

    private void ClearResults()
    {
        foreach (var obj in _currentUIItems)
        {
            Destroy(obj);
        }
        _currentUIItems.Clear();
    }

    // BONUS: Method to call from those "Filter Chips" (e.g., clicking "Nouveau")
    public void FilterBySpecificCategory(string categoryName)
    {
        searchInputField.text = ""; // Clear text
        if (searchPanel != null) searchPanel.SetActive(true);
        if (overlayshadow != null) overlayshadow.SetActive(true);

        var filteredList = _allItems.Where(x => x.CategoryName == categoryName).ToList();
        UpdateUI(filteredList);
    }
}