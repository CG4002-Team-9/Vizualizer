using UnityEngine;
using TMPro;


public class shieldValueTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text shieldValueText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the shield value text based on the current shield value
        UpdateShieldValueText();
    }

    private void Update()
    {
        // Continuously check if the health value has changed, and update the text accordingly
        UpdateShieldValueText();

    }

     private void UpdateShieldValueText()
    {
        shieldValueText.text = gameState.ShieldValue.ToString() + " / 30";
    }
}