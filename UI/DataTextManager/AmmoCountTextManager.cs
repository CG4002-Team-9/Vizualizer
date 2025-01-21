using UnityEngine;
using TMPro;

public class AmmoCountTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text ammoText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the ammo text based on the current ammo count
        UpdateAmmoText();
    }

    private void Update()
    {
        // Continuously check if the ammo count has changed, and update the text accordingly
        UpdateAmmoText();
    }

    private void UpdateAmmoText()
    {
        int ammoCount = gameState.AmmoCount;

        ammoText.text = ammoCount.ToString() + " / 6";
    }
}