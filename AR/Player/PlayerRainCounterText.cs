using UnityEngine;
using TMPro;

public class PlayerWaterbombColliderTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text rainbombValueText;

    void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        gameState.PlayerInWaterBombCountChanged += UpdatePlayerRainbombColliderText;
    }

    void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.PlayerInWaterBombCountChanged -= UpdatePlayerRainbombColliderText;
        }
    }

    void UpdatePlayerRainbombColliderText()
    {
        int rainbombValue = gameState.PlayerInWaterBombCount;

        rainbombValueText.text =  $"in {rainbombValue.ToString()} rainbomb(s)";
        if (rainbombValue > 0)
        {
            rainbombValueText.color = new Color(152, 0, 0, 255);
        }
        else
        {
            rainbombValueText.color = new Color(192, 192, 192, 255);
        }
    }
}