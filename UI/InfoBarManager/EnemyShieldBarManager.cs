using UnityEngine;

public class EnemyShieldBarManager : MonoBehaviour
{
    //reference to gamestate to get the shield
    private GameState gameState;

    //reference to the shield bar
    public GameObject shieldBarSlider;
    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the shield bar
        UpdateEnemyShieldBar();

        // print the shield value
        Debug.Log("Enemy Shield Value: " + gameState.ShieldValue);
    }

    // Update is called once per frame
    void Update()
    {
        // Continuously check if the shield has changed, and update the shield bar accordingly
        UpdateEnemyShieldBar();
    }

    // Function to update the shield bar based on the current shield
    private void UpdateEnemyShieldBar()
    {
        int shield = gameState.EnemyShieldValue;
        
        shieldBarSlider.GetComponent<UnityEngine.UI.Slider>().value = shield;
    }
}
