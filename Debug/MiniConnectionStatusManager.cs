using UnityEngine;
using TMPro;

public class MiniConnectionStatusManager : MonoBehaviour
{
    // Reference to the GameState object for getting ammo count
    private GameState gameState;

    // Reference to the TMP Text components
    public TMP_Text playerIDText;
    public TMP_Text RabbitMQConnectionText;
    public TMP_Text ActionQueueConnectionText;
    public TMP_Text StatusUpdateConnectionText;
    public TMP_Text PredictionConnectionText;
    public TMP_Text VestConnectionText;
    public TMP_Text GloveConnectionText;
    public TMP_Text LegConnectionText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;
    }
    private void Update()
    {
        // Update all texts
        UpdatePlayerID();
        UpdateRabbitMQConnection();
        UpdateActionQueueConnection();
        UpdateStatusUpdateConnection();
        UpdatePredictionConnection();
        UpdateVestConnection();
        UpdateGloveConnection();
        UpdateLegConnection();
    }

    private void UpdatePlayerID()
    {
        int playerID = gameState.PlayerID;
        playerIDText.text = playerID.ToString();
    }

    private void UpdateRabbitMQConnection()
    {
        bool rabbitMQConnection = gameState.RabbitMQConnected;
        RabbitMQConnectionText.color = rabbitMQConnection ? Color.green : Color.red;
    }

    private void UpdateActionQueueConnection()
    {
        bool actionQueueConnection = gameState.ActionQueueConnected;
        ActionQueueConnectionText.color = actionQueueConnection ? Color.green : Color.red;
    }

    private void UpdateStatusUpdateConnection()
    {
        bool statusUpdateConnection = gameState.StatusUpdateConnected;
        StatusUpdateConnectionText.color = statusUpdateConnection ? Color.green : Color.red;
    }

    private void UpdatePredictionConnection()
    {
        bool predictionConnection = gameState.PredictionConnected;
        PredictionConnectionText.color = predictionConnection ? Color.green : Color.red;
    }

    private void UpdateVestConnection()
    {
        bool vestConnection = gameState.VestActive;
        VestConnectionText.color = vestConnection ? Color.green : Color.red;
    }

    private void UpdateGloveConnection()
    {
        bool gloveConnection = gameState.GloveActive;
        GloveConnectionText.color = gloveConnection ? Color.green : Color.red;
    }

    private void UpdateLegConnection()
    {
        bool legConnection = gameState.LegActive;
        LegConnectionText.color = legConnection ? Color.green : Color.red;
    }
}