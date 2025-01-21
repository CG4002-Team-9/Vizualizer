using UnityEngine;
using TMPro;

public class ShieldDebugManager : MonoBehaviour
{
    // Reference to the GameState object for getting ammo count
    private GameState gameState;

    // Reference to the TMP Text component that will display the shield count
    public TMP_Text shieldCountText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the number of shields based on the current shield count
        UpdateDebugShieldCount();
    }

    private void Update()
    {
        // Continuously check if the ammo count has changed, and update the images accordingly
        UpdateDebugShieldCount();
    }

    private void UpdateDebugShieldCount()
    {
        int shieldCount = gameState.ShieldCount;

        shieldCountText.text = shieldCount.ToString();
    }
}
