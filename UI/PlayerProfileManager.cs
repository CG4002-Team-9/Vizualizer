using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileManager : MonoBehaviour
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
            // Initialize currentIndex from GameState.ProfilePic
            currentIndex = GameState.Instance.ProfilePic % images.Length;

            // Set the image based on currentIndex
            rawImage.texture = images[currentIndex];

            // Subscribe to ProfilePicChanged event
            GameState.Instance.ProfilePicChanged += OnProfilePicChanged;
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
            GameState.Instance.ProfilePicChanged -= OnProfilePicChanged;
        }
    }

    void OnProfilePicChanged()
    {
        // Update currentIndex and rawImage.texture
        currentIndex = GameState.Instance.ProfilePic % images.Length;
        rawImage.texture = images[currentIndex];
    }

    // Call this method to go to the next image
    public void NextImage()
    {
        if (images.Length > 0)
        {
            // Increment the index and wrap it around if it exceeds the number of images
            currentIndex = (currentIndex + 1) % images.Length;

            // Set the new texture
            rawImage.texture = images[currentIndex];

            // Update GameState's ProfilePic
            GameState.Instance.ProfilePic = currentIndex;
        }
    }
}
