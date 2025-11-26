using UnityEngine;

[CreateAssetMenu(fileName = "GalleryDatabase", menuName = "Gallery/Database")]
public class GalleryDatabase : ScriptableObject
{
    public Category[] categories;
}

[System.Serializable]
public class Category
{
    public string categoryName;
    public ImageItem[] images;
}

[System.Serializable]
public class ImageItem
{
    public bool isLocked = true;
    public string imageName;
    public Texture2D texture;
}