using UnityEngine;
using Vuforia;

public class ImageTargetSwitcher : MonoBehaviour
{
    public GameState gameState; // Reference to your GameState script
    public ImageTargetBehaviour player1Target; // Reference to the ImageTarget for Player 1
    public ImageTargetBehaviour player2Target; // Reference to the ImageTarget for Player 2

    void Start()
    {
        // Call the method to activate the correct target based on PlayerID
        SetImageTargetBasedOnPlayerID();
    }

    public void SetImageTargetBasedOnPlayerID()
    {
        // Fetch the current PlayerID from your GameState script
        int playerID = gameState.PlayerID;

        // Enable the appropriate image target and disable the other
        if (playerID == 1)
        {
            player1Target.ImageTargetType = ImageTargetType.PREDEFINED; // Set the ImageTargetType to PREDEFINED
            // set the ImageTargetType to PREDEFINED for Player 1
            player1Target.gameObject.SetActive(true); // Activate Player 1's image target
            
        }
        else
        {
            player1Target.gameObject.SetActive(false); // Deactivate Player 1's image target
            player2Target.gameObject.SetActive(true);  // Activate Player 2's image target
        }
    }
}