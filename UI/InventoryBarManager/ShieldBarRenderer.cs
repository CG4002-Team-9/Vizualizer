using UnityEngine;
using UnityEngine.UI;

public class ShieldBarRenderer : MonoBehaviour
{
    // Array to hold references to shield UI images
    public Image[] shieldImages;

    // Reference to the GameState object for getting shield count
    private GameState gameState;

    // Alpha values for transparency (1 = fully opaque, 0 = fully transparent)
    private float opaqueAlpha = 1f;
    private float transparentAlpha = 0.2f; // You can adjust this to your desired transparency level

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the shield images based on the current shield count
        UpdateShieldImages();
    }

    private void Update()
    {
        // Continuously check if the shield count has changed, and update the images accordingly
        UpdateShieldImages();
    }

    // Function to update the shield images based on the current shield count
    private void UpdateShieldImages()
    {
        int shieldCount = gameState.ShieldCount;

        for (int i = 0; i < shieldImages.Length; i++)
        {
            if (i < shieldCount)
            {
                // Shields that are "active" (less than shield count) are fully visible
                SetImageAlpha(shieldImages[i], opaqueAlpha);
            }
            else
            {
                // Shields beyond the shield count are semi-transparent
                SetImageAlpha(shieldImages[i], transparentAlpha);
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