using UnityEngine;
using UnityEngine.UI;

public class waterbombBarRenderer : MonoBehaviour
{
    // Array to hold references to waterbomb UI images
    public Image[] waterbombImages;

    // Reference to the GameState object for getting waterbomb count
    private GameState gameState;

    // Alpha values for transparency (1 = fully opaque, 0 = fully transparent)
    private float opaqueAlpha = 1f;
    private float transparentAlpha = 0.2f; // You can adjust this to your desired transparency level

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the waterbomb images based on the current waterbomb count
        UpdateWaterbombImages();
    }

    private void Update()
    {
        // Continuously check if the waterbomb count has changed, and update the images accordingly
        UpdateWaterbombImages();
    }

    // Function to update the waterbomb images based on the current waterbomb count
    private void UpdateWaterbombImages()
    {
        int waterbombCount = gameState.WaterbombCount;

        for (int i = 0; i < waterbombImages.Length; i++)
        {
            if (i < waterbombCount)
            {
                // Waterbombs that are "active" (less than waterbomb count) are fully visible
                SetImageAlpha(waterbombImages[i], opaqueAlpha);
            }
            else
            {
                // Waterbombs beyond the waterbomb count are semi-transparent
                SetImageAlpha(waterbombImages[i], transparentAlpha);
            }
        }
    }

    // Helper function to set the alpha of an image
    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}