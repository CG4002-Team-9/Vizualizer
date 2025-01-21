using UnityEngine;
using TMPro;

public class HealthValueTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text healthValueText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the health value text based on the current health value
        UpdateHealthValueText();
    }

    private void Update()
    {
        // Continuously check if the health value has changed, and update the text accordingly + Vignette
        UpdateHealthValueText();
    }

     private void UpdateHealthValueText()
    {
        healthValueText.text = gameState.HealthValue.ToString() + " / 100";
    }
}