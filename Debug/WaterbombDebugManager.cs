using UnityEngine;
using TMPro;

public class WaterbombDebugManager : MonoBehaviour
{
    // Reference to the GameState object for getting waterbomb count
    private GameState gameState;

    // Reference to the TMP Text component that will display the waterbomb count
    public TMP_Text waterbombCountText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the number of waterbombs based on the current waterbomb count
        UpdateDebugWaterbombCount();
    }

    private void Update()
    {
        // Continuously check if the waterbomb count has changed, and update the images accordingly
        UpdateDebugWaterbombCount();
    }

    private void UpdateDebugWaterbombCount()
    {
        int waterbombCount = gameState.WaterbombCount;

        waterbombCountText.text = waterbombCount.ToString();
    }
}
