using UnityEngine;
using UnityEngine.UI;

public class AmmoBarRenderer : MonoBehaviour
{
    // Array to hold references to ammo UI images
    public Image[] ammoImages;

    // Reference to the GameState object for getting ammo count
    private GameState gameState;

    // Alpha values for transparency (1 = fully opaque, 0 = fully transparent)
    private float opaqueAlpha = 1f;
    private float transparentAlpha = 0.2f; // You can adjust this to your desired transparency level

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the ammo images based on the current ammo count
        UpdateAmmoImages();
    }

    private void Update()
    {
        // Continuously check if the ammo count has changed, and update the images accordingly
        UpdateAmmoImages();
    }

    // Function to update the ammo images based on the current ammo count
    private void UpdateAmmoImages()
    {
        int ammoCount = gameState.AmmoCount;

        for (int i = 0; i < ammoImages.Length; i++)
        {
            if (i < ammoCount)
            {
                // Ammos that are "active" (less than ammo count) are fully visible
                SetImageAlpha(ammoImages[i], opaqueAlpha);
            }
            else
            {
                // Shields beyond the ammo count are semi-transparent
                SetImageAlpha(ammoImages[i], transparentAlpha);
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