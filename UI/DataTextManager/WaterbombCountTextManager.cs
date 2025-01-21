using UnityEngine;
using TMPro;

public class WaterbombCountTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text waterbombCountText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the waterbomb count text based on the current waterbomb count
        UpdateWaterbombCountText();
    }

    private void Update()
    {
        // Continuously check if the waterbomb count has changed, and update the text accordingly
        UpdateWaterbombCountText();
    }

    private void UpdateWaterbombCountText()
    {
        int waterbombCount = gameState.WaterbombCount;

        waterbombCountText.text = waterbombCount.ToString() + " / 2";
    }
}