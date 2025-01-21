using UnityEngine;
using TMPro;

public class ShieldCountTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text shieldCountText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the shield count text based on the current shield count
        UpdateShieldCountText();
    }

    private void Update()
    {
        // Continuously check if the shield count has changed, and update the text accordingly
        UpdateShieldCountText();
    }

    private void UpdateShieldCountText()
    {
        int shieldCount = gameState.ShieldCount;

        shieldCountText.text = shieldCount.ToString() + " / 3";
    }
}