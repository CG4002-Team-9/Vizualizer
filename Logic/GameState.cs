using UnityEngine;
using UnityEngine.Events;
using System;

public class GameState : MonoBehaviour
{
    // Singleton instance
    public static GameState Instance { get; private set; }

    private int playerId, healthValue, shieldValue, ownScore, enemyScore, shieldCount, ammoCount, waterbombCount, enemyHealthValue, enemyShieldValue, enemyInWaterBombCount, profilePic, enemyProfilePic, playerInWaterBombCount;
    private bool enemyActive, playerHit, playerShieldHit, enemyHit, enemyShieldHit, playerVisibleToEnemy;
    private bool rabbitMQConnected, actionQueueConnected, statusUpdateConnected, predictionConnected, vestActive, gloveActive, legActive;

    // Constants to define min and max values
    private const int MAX_SHIELD_COUNT = 3;
    private const int MIN_SHIELD_COUNT = 0;

    private const int MAX_AMMO_COUNT = 6;
    private const int MIN_AMMO_COUNT = 0;

    private const int MAX_WATERBOMB_COUNT = 2;
    private const int MIN_WATERBOMB_COUNT = 0;

    private const int MAX_HEALTH_VALUE = 100;
    private const int MIN_HEALTH_VALUE = 0;

    private const int MAX_SHIELD_VALUE = 30;
    private const int MIN_SHIELD_VALUE = 0;

    // Events for property changes
    public event Action EnemyActiveChanged;
    public event Action EnemyInWaterBombCountChanged;
    public event Action PlayerInWaterBombCountChanged;
    public event Action ProfilePicChanged;
    public event Action EnemyProfilePicChanged;

    // Events for playing hit animations
    public event Action PlayerHitEvent;
    public event Action PlayerShieldHitEvent;
    public event Action EnemyHitEvent;
    public event Action EnemyShieldHitEvent;

    // UnityEvent for game actions
    public UnityActionEvent gameActionOccurred;

    // UnityEvent for enemy game actions
    public UnityActionEvent enemyGameActionOccurred;

    // UnityEvent for received predictions
    public UnityPredictionEvent predictionOccurred;

    public bool PlayerVisibleToEnemy
    {
        get { return playerVisibleToEnemy; }
        set { playerVisibleToEnemy = value; }
    }

    public bool PlayerHit
    {
        get { return playerHit; }
        set
        {
            playerHit = value;
            if (playerHit)
            {
                PlayerHitEvent?.Invoke();
            }
        }
    }

    public bool PlayerShieldHit
    {
        get { return playerShieldHit; }
        set
        {
            playerShieldHit = value;
            if (playerShieldHit)
            {
                PlayerShieldHitEvent?.Invoke();
            }
        }
    }

    public bool EnemyHit{
        get { return enemyHit; }
        set
        {
            enemyHit = value;
            if (enemyHit)
            {
                EnemyHitEvent?.Invoke();
            }
        }
    }

    public bool EnemyShieldHit
    {
        get { return enemyShieldHit; }
        set
        {
            enemyShieldHit = value;
            if (enemyShieldHit)
            {
                EnemyShieldHitEvent?.Invoke();
            }
        }
    }

    public int ProfilePic
    {  get { return profilePic; } 
       set { if (profilePic != value)
            {
                profilePic = value;
                ProfilePicChanged?.Invoke();
            }
        }
    }

    public int EnemyProfilePic
    {
        get { return enemyProfilePic; }
        set
        {
            if (enemyProfilePic != value)
            {
                enemyProfilePic = value;
                EnemyProfilePicChanged?.Invoke();
            }
        }
    }

    public bool EnemyActive
    {
        get { return enemyActive; }
        set
        {
            if (enemyActive != value)
            {
                enemyActive = value;
                EnemyActiveChanged?.Invoke();
            }
        }
    }

    public int EnemyInWaterBombCount
    {
        get { return enemyInWaterBombCount; }
        set
        {
            int newValue = Mathf.Max(0, value);
            if (enemyInWaterBombCount != newValue)
            {
                enemyInWaterBombCount = newValue;
                EnemyInWaterBombCountChanged?.Invoke();
            }
        }
    }

    public int PlayerInWaterBombCount
    {
        get { return playerInWaterBombCount; }
        set
        {
            int newValue = Mathf.Max(0, value);
            if (playerInWaterBombCount != newValue)
            {
                playerInWaterBombCount = newValue;
                PlayerInWaterBombCountChanged?.Invoke();
            }
        }
    }

    public int PlayerID
    {
        get { return playerId; }
        set { playerId = value; }
    } 

    public bool RabbitMQConnected
    {
        get { return rabbitMQConnected; }
        set { rabbitMQConnected = value; }
    }

    public bool ActionQueueConnected
    {
        get { return actionQueueConnected; }
        set { actionQueueConnected = value; }
    }

    public bool StatusUpdateConnected
    {
        get { return statusUpdateConnected; }
        set { statusUpdateConnected = value; }
    }

    public bool PredictionConnected
    {
        get { return predictionConnected; }
        set { predictionConnected = value; }
    }

    public bool VestActive
    { 
        get { return vestActive; }
        set { vestActive = value; }
    }

    public bool GloveActive
    {
        get { return gloveActive; }
        set { gloveActive = value; }
    }

    public bool LegActive
    {
        get { return legActive; }
        set { legActive = value; }
    }

    public int HealthValue
    {
        get { return healthValue; }
        set { healthValue = Mathf.Clamp(value, MIN_HEALTH_VALUE, MAX_HEALTH_VALUE); } // Clamp healthValue
    }

    public int ShieldValue
    {
        get { return shieldValue; }
        set { shieldValue = Mathf.Clamp(value, MIN_SHIELD_VALUE, MAX_SHIELD_VALUE); } // Clamp shieldValue
    }

    public int EnemyHealthValue
    {
        get { return enemyHealthValue; }
        set { enemyHealthValue = Mathf.Clamp(value, MIN_HEALTH_VALUE, MAX_HEALTH_VALUE); } // Clamp enemyHealthValue
    }

    public int EnemyShieldValue
    {
        get { return enemyShieldValue; }
        set { enemyShieldValue = Mathf.Clamp(value, MIN_SHIELD_VALUE, MAX_SHIELD_VALUE); } // Clamp enemyShieldValue
    }

    public int OwnScore
    {
        get { return ownScore; }
        set { ownScore = value; }
    }

    public int EnemyScore
    {
        get { return enemyScore; }
        set { enemyScore = value; }
    }

    public int ShieldCount
    {
        get { return shieldCount; }
        set { shieldCount = Mathf.Clamp(value, MIN_SHIELD_COUNT, MAX_SHIELD_COUNT); } // Clamp shieldCount
    }

    public int AmmoCount
    {
        get { return ammoCount; }
        set { ammoCount = Mathf.Clamp(value, MIN_AMMO_COUNT, MAX_AMMO_COUNT); } // Clamp ammoCount
    }

    public int WaterbombCount
    {
        get { return waterbombCount; }
        set { waterbombCount = Mathf.Clamp(value, MIN_WATERBOMB_COUNT, MAX_WATERBOMB_COUNT); } // Clamp waterbombCount
    }

    // Increment and decrement methods for shieldCount
    public void IncrementShieldCount()
    {
        ShieldCount = Mathf.Min(ShieldCount + 1, MAX_SHIELD_COUNT);
    }

    public void DecrementShieldCount()
    {
        ShieldCount = Mathf.Max(ShieldCount - 1, MIN_SHIELD_COUNT);
    }

    // Increment and decrement methods for ammoCount
    public void IncrementAmmoCount()
    {
        AmmoCount = Mathf.Min(AmmoCount + 1, MAX_AMMO_COUNT);
    }

    public void DecrementAmmoCount()
    {
        AmmoCount = Mathf.Max(AmmoCount - 1, MIN_AMMO_COUNT);
    }

    // Increment and decrement methods for waterbombCount
    public void IncrementWaterbombCount()
    {
        WaterbombCount = Mathf.Min(WaterbombCount + 1, MAX_WATERBOMB_COUNT);
    }

    public void DecrementWaterbombCount()
    {
        WaterbombCount = Mathf.Max(WaterbombCount - 1, MIN_WATERBOMB_COUNT);
    }

    public void IncrementHealthValue()
    {
        HealthValue = Mathf.Min(HealthValue + 5, MAX_HEALTH_VALUE);
    }

    public void DecrementHealthValue()
    {
        HealthValue = Mathf.Max(HealthValue - 5, MIN_HEALTH_VALUE);
    }

    public void IncrementEnemyHealthValue()
    {
        EnemyHealthValue = Mathf.Min(EnemyHealthValue + 5, MAX_HEALTH_VALUE);
    }

    public void DecrementEnemyHealthValue()
    {
        EnemyHealthValue = Mathf.Max(EnemyHealthValue - 5, MIN_HEALTH_VALUE);
    }

    public void IncrementShieldValue()
    {
        ShieldValue = Mathf.Min(ShieldValue + 5, MAX_SHIELD_VALUE);
    }

    public void DecrementShieldValue()
    {
        ShieldValue = Mathf.Max(ShieldValue - 5, MIN_SHIELD_VALUE);
    }

    public void IncrementEnemyShieldValue()
    {
        EnemyShieldValue = Mathf.Min(EnemyShieldValue + 5, MAX_SHIELD_VALUE);
    }

    public void DecrementEnemyShieldValue()
    {
        EnemyShieldValue = Mathf.Max(EnemyShieldValue - 5, MIN_SHIELD_VALUE);
    }

    // Unity lifecycle methods
    private void Awake()
    {
        // Singleton pattern enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Set initial values
        playerId = 1; // player ID
        vestActive = false;
        gloveActive = false;
        legActive = false;

        rabbitMQConnected = false;
        actionQueueConnected = false;
        statusUpdateConnected = false;
        predictionConnected = false;
        
        enemyActive = false;
        healthValue = 100; // Example initial health value
        shieldValue = 0; // Example initial shield value
        ownScore = 0; // Example initial own score
        enemyScore = 0; // Example initial enemy score
        shieldCount = 3; // Example initial shield count
        ammoCount = 6; // Example initial ammo count
        waterbombCount = 2; // Example initial waterbomb count
        enemyHealthValue = 100; // Example initial enemy health value
        enemyShieldValue = 0; // Example initial enemy shield value
        profilePic = 0;
        enemyProfilePic = 0;
    }

    // Method to handle game actions
    public void HandleGameAction(string actionType)
    {
      Debug.Log($"HandleGameAction called with actionType: {actionType}");
      gameActionOccurred.Invoke(actionType);
      Debug.Log($"Game action occurred: {actionType}");
    }

    public void HandleEnemyGameAction(string actionType)
    {
      Debug.Log($"HandleEnemyGameAction called with actionType: {actionType}");
      enemyGameActionOccurred.Invoke(actionType);
      Debug.Log($"Enemy game action occurred: {actionType}");
    }

    // Method to handle game predictions sent
    public void HandlePredictionMessage(string predictionType, float predictionValue)
    {
      Debug.Log($"HandleGamePrediction called with predictionType: {predictionType} and predictionValue: {predictionValue}");
      predictionOccurred.Invoke(predictionType, predictionValue);
      Debug.Log($"Game prediction occurred: {predictionType} with value: {predictionValue}");
    }

  }

// UnityEvent for game actions
[System.Serializable]
public class UnityActionEvent : UnityEvent<string> { }

[System.Serializable]
public class UnityPredictionEvent : UnityEvent<string, float> { }