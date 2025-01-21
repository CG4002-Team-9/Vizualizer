using UnityEngine;
using TMPro;

public class AmmoDebugManager : MonoBehaviour
{
    // Reference to the GameState object for getting ammo count
    private GameState gameState;

    // Reference to the TMP Text component that will display the ammo count
    public TMP_Text ammoCountText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the number of ammo based on the current ammo count
        UpdateDebugAmmoCount();
    }

    private void Update()
    {
        // Continuously check if the ammo count has changed, and update the images accordingly
        UpdateDebugAmmoCount();
    }

    private void UpdateDebugAmmoCount()
    {
        int ammoCount = gameState.AmmoCount;

        ammoCountText.text = ammoCount.ToString();
    }
}