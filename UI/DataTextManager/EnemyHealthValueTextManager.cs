using UnityEngine;
using TMPro;

public class EnemyHealthValueTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text enemyHealthValueText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the health value text based on the current health value
        UpdateEnemyHealthValueText();
    }

    private void Update()
    {
        // Continuously check if the health value has changed, and update the text accordingly
        UpdateEnemyHealthValueText();
    }

    private void UpdateEnemyHealthValueText()
    {
        int healthValue = gameState.EnemyHealthValue;

        enemyHealthValueText.text = healthValue.ToString() + " / 100";
    }
}