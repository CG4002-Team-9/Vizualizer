using UnityEngine;
using TMPro;

public class EnemyShieldValueTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text enemyShieldValueText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the shield value text based on the current shield value
        UpdateEnemyShieldValueText();
    }

    private void Update()
    {
        // Continuously check if the shield value has changed, and update the text accordingly
        UpdateEnemyShieldValueText();
    }

    private void UpdateEnemyShieldValueText()
    {
        int shieldValue = gameState.EnemyShieldValue;

        enemyShieldValueText.text = shieldValue.ToString() + " / 30";
    }
}