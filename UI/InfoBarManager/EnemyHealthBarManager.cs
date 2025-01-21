using UnityEngine;

public class EnemyHealthBarManager : MonoBehaviour
{
    //reference to gamestate to get the health
    private GameState gameState;

    //reference to the health bar slider
    public GameObject healthBarSlider;
    
    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the health bar
        UpdateEnemyHealthBar();

        // print the health value
        Debug.Log("Enemy Health Value: " + gameState.HealthValue);
    }

    // Update is called once per frame
    void Update()
    {
        // Continuously check if the health has changed, and update the health bar accordingly
        UpdateEnemyHealthBar();
        
    }

    // Function to update the health bar based on the current health
    private void UpdateEnemyHealthBar()
    {
        int health = gameState.EnemyHealthValue;

        healthBarSlider.GetComponent<UnityEngine.UI.Slider>().value = health;
    }
}