using UnityEngine;
using TMPro;

public class ScoreboardManager : MonoBehaviour
{
    private GameState gameState;
    public TMP_Text scoreboardText;

    private void Start()
    {
        gameState = GameState.Instance;

        UpdateScoreboard();
    }

    private void Update()
    {
        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        int ownScore = gameState.OwnScore;
        int enemyScore = gameState.EnemyScore;

        scoreboardText.text = ownScore + " : " + enemyScore;
    }
}