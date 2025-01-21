using UnityEngine;
using UnityEngine.UI;

public class EnemyProfileManager : MonoBehaviour
{
    public RawImage rawImage;   // Reference to the RawImage where the texture will be displayed
    public string folderName = "Player";  // Folder name in the Resources directory

    private Texture[] images;   // Array to hold all loaded textures
    private int currentIndex = 0;   // Keeps track of the current image index

    void Start()
    {
        // Load all textures from the folder
        images = Resources.LoadAll<Texture>(folderName);

        // Check if we have images
        if (images.Length > 0)
        {
            currentIndex = GameState.Instance.EnemyProfilePic % images.Length;

            // Set the image based on currentIndex
            rawImage.texture = images[currentIndex];

            // Subscribe to a custom event for enemy profile pic changes
            GameState.Instance.EnemyProfilePicChanged += OnEnemyProfilePicChanged;
        }
        else
        {
            Debug.LogError("No images found in the folder: " + folderName);
        }
    }

    void OnDestroy()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.EnemyProfilePicChanged -= OnEnemyProfilePicChanged;
        }
    }

    void OnEnemyProfilePicChanged()
    {
        // Update currentIndex and rawImage.texture
        currentIndex = GameState.Instance.EnemyProfilePic % images.Length;
        rawImage.texture = images[currentIndex];
    }
}
