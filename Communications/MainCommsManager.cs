using UnityEngine;
using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

public class MainCommsManager : MonoBehaviour
{
    public static MainCommsManager Instance { get; private set; }
    // RabbitMQ Fields
    private IConnection rabbitConnection;
    private IModel rabbitChannel;
    public string rabbitMQHost = "178.128.213.67";
    public int rabbitMQPort = 5672;
    public string rabbitMQUsername = "admin";
    public string rabbitMQPassword = "Team9Team";
    public string updateGEQueue = "update_ge_queue";
    public string updateEveryoneExchange = "update_everyone_exchange";
    public string updatePredictionsExchange = "update_predictions_exchange";

    private EventingBasicConsumer rabbitUpdateConsumer;
    private EventingBasicConsumer rabbitPredictionsConsumer;
    private string updateQueueName;
    private string predictionsQueueName;

    private GameState gameState;

    [Serializable]
    public class PlayerState
    {
        public int hp;
        public int bullets;
        public int bombs;
        public int shield_hp;
        public int deaths;
        public int shields;
        public bool opponent_hit;
        public bool opponent_shield_hit;
        public bool opponent_visible;
        public int opponent_in_rain_bomb;
        public bool glove_connected;
        public bool vest_connected;
        public bool leg_connected;
        public bool disconnected;
        public bool login;
        public int profile_pic;
    }

    [Serializable]
    public class GameStateUpdate
    {
        public GameStateData game_state;
        public string action;
        public int player_id;
    }

    [Serializable]
    public class GameStateData
    {
        public PlayerState p1;
        public PlayerState p2;
    }

    [Serializable]
    public class PredictionMessage
    {
        public int player_id;
        public string action_type;
        public float confidence;
    }

    // Flag to prevent multiple concurrent Refresh calls
    private bool isRefreshing = false;

    async void Start()
    {
        await StartScriptAsync();
        StartCoroutine(PeriodicConnectionCheck());

        //Singleton MainCommsManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    
    // Initializes the communication manager by setting up event subscriptions and connecting to RabbitMQ.
    private async Task StartScriptAsync()
    {
        Debug.Log("MainCommsManager Start initiated.");
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("GameState.Instance is null. Ensure that GameState is initialized before MainCommsManager.");
            return;
        }

        gameState.EnemyActiveChanged += SendGameUpdateMessage;
        gameState.EnemyInWaterBombCountChanged += SendGameUpdateMessage;
        gameState.ProfilePicChanged += SendProfilePicUpdateMessage;

        // Initialize MainThreadDispatcher
        var dispatcher = MainThreadDispatcher.Instance;
        if (dispatcher == null)
        {
            Debug.LogError("MainThreadDispatcher.Instance is null. Ensure it is initialized before MainCommsManager.");
            return;
        }
        Debug.Log("MainThreadDispatcher accessed from MainCommsManager Start.");

        // Connect to RabbitMQ
        await ConnectToRabbitMQAsync();

        // Start consuming messages from RabbitMQ
        StartConsumingRabbitMessages();

        // Refresh data
        RequestForUpdate();
    }

    #region RabbitMQ Methods

    
    // Asynchronously connects to RabbitMQ and sets up exchanges and queues.
    private async Task ConnectToRabbitMQAsync()
    {
        Debug.Log("Connecting to RabbitMQ...");
        var factory = new ConnectionFactory()
        {
            HostName = rabbitMQHost,
            Port = rabbitMQPort,
            UserName = rabbitMQUsername,
            Password = rabbitMQPassword,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5) // Increased recovery interval for stability
        };

        try
        {
            await Task.Run(() =>
            {
                rabbitConnection = factory.CreateConnection();
                rabbitChannel = rabbitConnection.CreateModel();

                // Declare the update everyone exchange
                rabbitChannel.ExchangeDeclare(exchange: updateEveryoneExchange, type: ExchangeType.Fanout, durable: true);

                // Declare the update predictions exchange
                rabbitChannel.ExchangeDeclare(exchange: updatePredictionsExchange, type: ExchangeType.Fanout, durable: true);

                // Declare the update_ge_queue
                rabbitChannel.QueueDeclare(queue: updateGEQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // Declare an exclusive, auto-deleted queue with a generated name for update exchange
                var queueDeclareUpdate = rabbitChannel.QueueDeclare(queue: "", durable: true, exclusive: true, autoDelete: true, arguments: null);
                updateQueueName = queueDeclareUpdate.QueueName;
                rabbitChannel.QueueBind(queue: updateQueueName, exchange: updateEveryoneExchange, routingKey: "");

                // Declare an exclusive, auto-deleted queue with a generated name for update exchange
                var queueDeclarePredictions = rabbitChannel.QueueDeclare(queue: "", durable: true, exclusive: true, autoDelete: true, arguments: null);
                predictionsQueueName = queueDeclarePredictions.QueueName;
                rabbitChannel.QueueBind(queue: predictionsQueueName, exchange: updatePredictionsExchange, routingKey: "");
            });

            Debug.Log($"Connected to RabbitMQ");
            UpdateConnectionStatus(true, "RabbitMQ");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error connecting to RabbitMQ: {ex.Message}");
            UpdateConnectionStatus(false, "RabbitMQ");
        }
    }

    
    // Starts consuming messages from the RabbitMQ queue.
    private void StartConsumingRabbitMessages()
    {
        if (rabbitChannel == null || !rabbitChannel.IsOpen)
        {
            Debug.LogError("RabbitMQ channel is not open. Cannot start consuming messages.");
            return;
        }

        // Start consuming messages from the update queue
        rabbitUpdateConsumer = new EventingBasicConsumer(rabbitChannel);
        rabbitUpdateConsumer.Received += HandleReceivedRabbitMessage;
        rabbitChannel.BasicConsume(queue: updateQueueName, consumer: rabbitUpdateConsumer);

        // Start consuming messages from the predictions queue
        rabbitPredictionsConsumer = new EventingBasicConsumer(rabbitChannel);
        rabbitPredictionsConsumer.Received += HandleReceivedPredictionsMessage;
        rabbitChannel.BasicConsume(queue: predictionsQueueName, consumer: rabbitPredictionsConsumer);

        Debug.Log("Started consuming RabbitMQ messages.");
    }

    
    // Handles received Update messages from RabbitMQ.
    private void HandleReceivedRabbitMessage(object sender, BasicDeliverEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Body);
        Debug.Log($"Received RabbitMQ message: {message}");

        try
        {
            // Parse the JSON data using Json.NET
            GameStateUpdate gameStateUpdate = JsonConvert.DeserializeObject<GameStateUpdate>(message);

            if (gameStateUpdate != null && gameStateUpdate.game_state != null)
            {
                Debug.Log("Parsed GameStateUpdate successfully.");
                // Dispatch the UpdateGameState to the main thread
                MainThreadDispatcher.Instance.Enqueue(() => UpdateGameState(gameStateUpdate));
            }
            else
            {
                Debug.LogWarning("Received GameStateUpdate is null or incomplete.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing RabbitMQ message: {ex.Message}");
        }
    }

    // Handles received Predictions messages from RabbitMQ.
    private void HandleReceivedPredictionsMessage(object sender, BasicDeliverEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Body);
        Debug.Log($"Received Predictions message: {message}");

        try
        {
            // Parse the JSON data using Json.NET
            PredictionMessage predictionMessage = JsonConvert.DeserializeObject<PredictionMessage>(message);

            if (predictionMessage != null)
            {
                Debug.Log("Parsed PredictionMessage successfully.");
                // Dispatch the UpdateGameState to the main thread
                MainThreadDispatcher.Instance.Enqueue(() => HandlePredictionMessage(predictionMessage));
            }
            else
            {
                Debug.LogWarning("Received PredictionMessage is null or incomplete.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing Predictions message: {ex.Message}");
        }
    }

    // Asynchronously sends a message to RabbitMQ.
    public async Task SendRabbitMessage(string message)
    {
        if (rabbitChannel != null && rabbitChannel.IsOpen)
        {
            var body = Encoding.UTF8.GetBytes(message);

            var properties = rabbitChannel.CreateBasicProperties();
            properties.Persistent = true;

            try
            {
                await Task.Run(() =>
                {
                    rabbitChannel.BasicPublish(exchange: "",
                                             routingKey: updateGEQueue,
                                             basicProperties: properties,
                                             body: body);
                });

                Debug.Log($"Message sent to RabbitMQ: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending message to RabbitMQ: {ex.Message}");
                // Attempt to reconnect with RabbitMQ
                await ConnectToRabbitMQAsync();
            }
        }
        else
        {
            Debug.LogError("RabbitMQ channel is not open.");
        }
    }
    
    // Sends a game update message based on changes in enemy activity or bomb count.
    private readonly object lockObj = new object();
    private Timer debounceTimer;
    const int DebounceTimeMs = 1000; // Only debounce if the visibility is false

    public void SendGameUpdateMessage()
    {
        lock (lockObj)
        {
            var message = new JObject
            {
                ["game_state"] = new JObject()
            };
    
            string playerKey = GameState.Instance.PlayerID == 1 ? "p1" : "p2";
            message["update"] = true;
            message["game_state"][playerKey] = new JObject
            {
                ["opponent_visible"] = GameState.Instance.EnemyActive,
                ["opponent_in_rain_bomb"] = GameState.Instance.EnemyInWaterBombCount
            };
    
            string jsonMessage = message.ToString(Formatting.None);
            Debug.Log($"Prepared RabbitMQ message: {jsonMessage}");
    
            // Dispose existing timer
            debounceTimer?.Dispose();
    
            if (GameState.Instance.EnemyActive)
            {
                // Send the message immediately
                _ = SendRabbitMessage(jsonMessage);
            }
            else
            {
                // Start new debounce timer
                debounceTimer = new Timer(async _ =>
                {
                    // Send the message
                    await SendRabbitMessage(jsonMessage);
    
                    // Dispose timer after execution
                    debounceTimer?.Dispose();
                    debounceTimer = null;
                }, null, DebounceTimeMs, Timeout.Infinite);
            }
        }
    }

    public void SendProfilePicUpdateMessage()
    {
        Debug.Log("SendProfilePicUpdateMessage triggered by event.");

        var message = new JObject
        {
            ["game_state"] = new JObject()
        };

        string playerKey = gameState.PlayerID == 1 ? "p1" : "p2";
        message["update"] = true;
        message["game_state"][playerKey] = new JObject
        {
            ["profile_pic"] = gameState.ProfilePic
        };

        string jsonMessage = message.ToString(Formatting.None);
        Debug.Log($"Prepared RabbitMQ message: {jsonMessage}");

        // Enqueue the SendRabbitMessage to be executed on a background thread
        Task.Run(async () =>
        {
            await SendRabbitMessage(jsonMessage);
        });
    }

    public void RequestForUpdate()
    {
        Debug.Log("RequestForUpdate triggered");

        var message = new JObject
        {
            ["game_state"] = new JObject()
        };

        string playerKey = gameState.PlayerID == 1 ? "p1" : "p2";
        message["update"] = true;
        message["f"] = true;

        string jsonMessage = message.ToString(Formatting.None);
        Debug.Log($"Prepared RabbitMQ message: {jsonMessage}");

        // Enqueue the SendRabbitMessage to be executed on a background thread
        Task.Run(async () =>
        {
            await SendRabbitMessage(jsonMessage);
        });
    }


    // Sends the full game state information to RabbitMQ.
    public void SendFullGameState()
    {
        Debug.Log("SendFullGameState triggered.");

        var message = new JObject
        {
            ["update"] = true,
            ["game_state"] = new JObject()
        };

        // get the player id
        string playerKey = gameState.PlayerID == 1 ? "p1" : "p2";

        // Own game state
        var ownState = new JObject
        {
            ["hp"] = gameState.HealthValue,
            ["bullets"] = gameState.AmmoCount,
            ["bombs"] = gameState.WaterbombCount,
            ["shield_hp"] = gameState.ShieldValue,
            ["shields"] = gameState.ShieldCount,
        };

        message["game_state"][playerKey] = ownState;

        string jsonMessage = message.ToString(Formatting.None);
        Debug.Log($"Prepared full game state RabbitMQ message: {jsonMessage}");

        // Enqueue the SendRabbitMessage to be executed on a background thread
        Task.Run(async () =>
        {
            await SendRabbitMessage(jsonMessage);
        });

        Debug.Log("Full game state message enqueued to RabbitMQ.");
    }

    public void SendPlayerLoginMessage()
    {
      Debug.Log($"Login for player {gameState.PlayerID} triggered.");
      var message = new JObject
      {
          ["update"] = true,
          ["f"] = true,
          ["game_state"] = new JObject()
      };
      
      string playerKey = gameState.PlayerID == 1 ? "p1" : "p2";
      message["game_state"][playerKey] = new JObject
      {
          ["login"] = true
      };
      
      string jsonMessage = message.ToString(Formatting.None);

      Debug.Log($"Prepared game action RabbitMQ message: {jsonMessage}");

      Task.Run(async () =>
      {
          await SendRabbitMessage(jsonMessage);
      });

      Debug.Log("Game action message enqueued to RabbitMQ.");
    }

    // Sends game action messages to RabbitMQ.
    public void SendGameActionMessage(string action_type, bool hit)
    {
        Debug.Log($"SendGameActionMessage triggered with action_type={action_type}, hit={hit}");

        var message = new JObject
        {
            ["action"] = true,
            ["action_type"] = action_type,
            ["player_id"] = gameState.PlayerID,
            ["hit"] = hit,
            ["game_state"] = new JObject()
        };

        string playerKey = gameState.PlayerID == 1 ? "p1" : "p2";
        message["game_state"][playerKey] = new JObject
        {
            ["opponent_visible"] = gameState.EnemyActive
        };

        string jsonMessage = message.ToString(Formatting.None);
        Debug.Log($"Prepared game action RabbitMQ message: {jsonMessage}");

        // Enqueue the SendRabbitMessage to be executed on a background thread
        Task.Run(async () =>
        {
            await SendRabbitMessage(jsonMessage);
        });

        Debug.Log("Game action message enqueued to RabbitMQ.");
    }


    // Sends a game action message indicating a hit.
    public void SendGameActionMessageHit(string action_type)
    {
        Debug.Log($"SendGameActionMessageHit called with action_type={action_type}");
        SendGameActionMessage(action_type, true);
    }

    
    // Sends a game action message indicating a miss.
    public void SendGameActionMessageNotHit(string action_type)
    {
        Debug.Log($"SendGameActionMessageNotHit called with action_type={action_type}");
        SendGameActionMessage(action_type, false);
    }

    #endregion

    
    // Updates the game state based on the received RabbitMQ message.
    private void UpdateGameState(GameStateUpdate gameStateUpdate)
    {
        Debug.Log("Executing UpdateGameState on main thread.");

        PlayerState ownPlayerState = gameState.PlayerID == 1 ? gameStateUpdate.game_state.p1 : gameStateUpdate.game_state.p2;
        PlayerState enemyPlayerState = gameState.PlayerID == 1 ? gameStateUpdate.game_state.p2 : gameStateUpdate.game_state.p1;

        if (ownPlayerState != null)
        {
            Debug.Log($"Own Player State Update: HP={ownPlayerState.hp}, Bullets={ownPlayerState.bullets}, Bombs={ownPlayerState.bombs}, Shield HP={ownPlayerState.shield_hp}, Deaths={ownPlayerState.deaths}, Shields={ownPlayerState.shields}");

            // Update own player's game state
            gameState.HealthValue = ownPlayerState.hp;
            gameState.AmmoCount = ownPlayerState.bullets;
            gameState.WaterbombCount = ownPlayerState.bombs;
            gameState.ShieldValue = ownPlayerState.shield_hp;
            gameState.ShieldCount = ownPlayerState.shields;

            // Update enemy player's hit and shield hit status
            gameState.EnemyHit = ownPlayerState.opponent_hit;
            gameState.EnemyShieldHit = ownPlayerState.opponent_shield_hit;

            gameState.GloveActive = ownPlayerState.glove_connected;
            gameState.VestActive = ownPlayerState.vest_connected;
            gameState.LegActive = ownPlayerState.leg_connected;

            gameState.OwnScore = ownPlayerState.deaths;

            gameState.ProfilePic = ownPlayerState.profile_pic;
        }

        if (enemyPlayerState != null)
        {
            Debug.Log($"Enemy Player State Update: HP={enemyPlayerState.hp}, Bullets={enemyPlayerState.bullets}, Bombs={enemyPlayerState.bombs}, Shield HP={enemyPlayerState.shield_hp}, Deaths={enemyPlayerState.deaths}, Shields={enemyPlayerState.shields}");

            // Update enemy player's game state
            gameState.EnemyHealthValue = enemyPlayerState.hp;
            gameState.EnemyShieldValue = enemyPlayerState.shield_hp;
            gameState.EnemyScore = enemyPlayerState.deaths;

            gameState.EnemyProfilePic = enemyPlayerState.profile_pic;

            gameState.PlayerInWaterBombCount = enemyPlayerState.opponent_in_rain_bomb;

            gameState.PlayerHit = enemyPlayerState.opponent_hit;
            gameState.PlayerShieldHit = enemyPlayerState.opponent_shield_hit;

            gameState.PlayerVisibleToEnemy = enemyPlayerState.opponent_visible;

        }

        if (!string.IsNullOrEmpty(gameStateUpdate.action))
        {
            Debug.Log($"Game Action Update: {gameStateUpdate.action} from player {gameStateUpdate.player_id}");

            string actionType = gameStateUpdate.action;

            // Handle game actions if player is correct
            if (gameStateUpdate.player_id == gameState.PlayerID)
            {
                gameState.HandleGameAction(actionType);
            } else {
                gameState.HandleEnemyGameAction(actionType);
            }
        }
    }

    private void HandlePredictionMessage(PredictionMessage predictionMessage)
    {
        Debug.Log("Executing HandlePredictionMessage on main thread.");

        Debug.Log($"Prediction Message: Player ID={predictionMessage.player_id}, Action Type={predictionMessage.action_type}, Confidence={predictionMessage.confidence}");

        // Handle prediction message
        if (predictionMessage.player_id == gameState.PlayerID)
        {
            Debug.Log("Prediction message for own player. Handling prediction...");
            gameState.HandlePredictionMessage(predictionMessage.action_type, predictionMessage.confidence);
        }
    }

    // Updates the connection status of the specified server type.
    private void UpdateConnectionStatus(bool isConnected, string serverType)
    {
        if (serverType == "RabbitMQ")
        {
            gameState.RabbitMQConnected = isConnected;
        }

        Debug.Log($"{serverType} connection status updated: {(isConnected ? "Connected" : "Disconnected")}");
    }

    // Called when the application quits to ensure proper cleanup.
    void OnApplicationQuit()
    {
        EndScriptAsync().ConfigureAwait(false);
    }

    // Asynchronously disconnects from RabbitMQ and cleans up event subscriptions.
    private async Task EndScriptAsync()
    {
        Debug.Log("OnApplicationQuit triggered.");

        await Task.Run(() =>
        {
            // Disconnect RabbitMQ
            if (rabbitChannel != null && rabbitChannel.IsOpen)
            {
                try
                {
                    rabbitChannel.Close();
                    rabbitConnection.Close();

                    Debug.Log("Disconnected from RabbitMQ.");
                    UpdateConnectionStatus(false, "RabbitMQ");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error disconnecting from RabbitMQ: {ex.Message}");
                }
            }

            if (gameState != null)
            {
                gameState.EnemyActiveChanged -= SendGameUpdateMessage;
                gameState.EnemyInWaterBombCountChanged -= SendGameUpdateMessage;
                gameState.ProfilePicChanged -= SendProfilePicUpdateMessage;
                Debug.Log("Unsubscribed from GameState events.");
            }
        });
    }

    // Refreshes the RabbitMQ connection by disconnecting and reconnecting.
    public async Task RefreshAsync()
    {
        if (isRefreshing)
        {
            Debug.LogWarning("Refresh is already in progress.");
            return;
        }

        isRefreshing = true;
        Debug.Log("Starting Refresh...");

        await EndScriptAsync();
        await StartScriptAsync();

        isRefreshing = false;
        Debug.Log("Refresh completed.");
    }

    
    // Handles the refresh button click event.
    public async void OnRefreshButtonClick()
    {
        Debug.Log("Refresh button clicked.");
        await RefreshAsync();
    }

    
    // Coroutine that periodically checks the connection status.
    private IEnumerator PeriodicConnectionCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.5f);
            CheckConnectionStatusAsync().ConfigureAwait(false);
        }
    }

    
    // Asynchronously checks the connection status of the RabbitMQ exchange and queue.
    private async Task CheckConnectionStatusAsync()
    {
        if (isRefreshing)
        {
            Debug.Log("Currently refreshing. Skipping connection status check.");
            return;
        }

        bool exchangeUpdateConnected = false;
        bool exchangePredictionsConnected = false;
        bool queueConnected = false;

        try
        {
            await Task.Run(() =>
            {
                if (rabbitChannel != null && rabbitChannel.IsOpen)
                {
                    try
                    {
                        // Check if the update exchange is reachable
                        rabbitChannel.ExchangeDeclarePassive(updateEveryoneExchange);
                        exchangeUpdateConnected = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exchange connection check failed: {ex.Message}");
                    }
                    try
                    {
                        // Check if the predictions exchange is reachable
                        rabbitChannel.ExchangeDeclarePassive(updatePredictionsExchange);
                        exchangePredictionsConnected = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exchange connection check failed: {ex.Message}");
                    }
                    try
                    {
                        // Check if the action queue is reachable
                        rabbitChannel.QueueDeclarePassive(updateGEQueue);
                        queueConnected = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Queue connection check failed: {ex.Message}");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during connection status check: {ex.Message}");
        }

        gameState.StatusUpdateConnected = exchangeUpdateConnected;
        gameState.PredictionConnected = exchangePredictionsConnected;
        gameState.ActionQueueConnected = queueConnected;

        Debug.Log($"Periodic Connection Check: Status Update Connected: {exchangeUpdateConnected}, Predictions Connected: {exchangePredictionsConnected}, Actions Connected: {queueConnected}");

        if (!exchangeUpdateConnected || !queueConnected || !exchangePredictionsConnected)
        {
            await RefreshAsync();
        }
    }
}