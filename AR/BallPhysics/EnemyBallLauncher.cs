using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyBallPrefab
{
    public string ballType;
    public GameObject prefab;

    [Header("Launch Settings")]
    public Transform launchPoint;          // Custom Launch Point for this ball
    public Transform playerTarget;         // Custom Player Target for this ball
    public float launchForce = 4f;         // Forward force to propel the ball
    public float upwardForce = 4f;         // Upward force to create the arc
    public float homingForce = 7f;         // Force applied each frame for homing
    public float homingDuration = 2f;      // Duration for the homing effect

    [Header("Roll Settings")]
    public float rollFactor = 10f;         // Torque applied to make the ball roll

    [Header("Target Settings")]
    public string targetTag = "PlayerPosition"; // Default target tag ("PlayerPosition" for player or "PlayerTarget" for other)
}

public class EnemyBallLauncher : MonoBehaviour
{
    [Header("Ball Settings")]
    public List<EnemyBallPrefab> ballPrefabsList = new List<EnemyBallPrefab>(); // Assign the Ball Prefabs in the Inspector
    private Dictionary<string, EnemyBallPrefab> ballPrefabs = new Dictionary<string, EnemyBallPrefab>();

    [Header("Launch Cooldown")]
    public float launchCooldown = 0.5f;      // Time between launches
    private bool canLaunch = true;

    private GameObject currentBall;
    private Coroutine homingCoroutine;
    private GameState gameState;

    void Awake()
    {
        // Convert the list to a dictionary
        foreach (var ballPrefab in ballPrefabsList)
        {
            if (!ballPrefabs.ContainsKey(ballPrefab.ballType))
            {
                ballPrefabs.Add(ballPrefab.ballType, ballPrefab);
            }
        }
    }

    void Start()
    {
        gameState = GameState.Instance;
    }

    // Method to launch the ball
    public void LaunchBall(string ballType)
    {
        if (!canLaunch)
        {
            Debug.Log("Launch is on cooldown.");
            return;
        }
        if (gameState.EnemyActive == false)
        {
            Debug.Log("Enemy is not active.");
            return;
        }

        canLaunch = false;
        Invoke(nameof(ResetLaunch), launchCooldown);

        Debug.Log("Attempting to launch ball...");

        // Validate ballType
        if (!ballPrefabs.ContainsKey(ballType))
        {
            Debug.Log($"'{ballType}' is not found in the dictionary or is not a ball action.");
            return;
        }

        EnemyBallPrefab selectedBallPrefab = ballPrefabs[ballType];

        // Validate ballPrefab
        if (selectedBallPrefab.prefab == null)
        {
            Debug.LogError($"Ball Prefab for type '{ballType}' is not assigned.");
            return;
        }

        // Validate launchPoint
        if (selectedBallPrefab.launchPoint == null)
        {
            Debug.LogError("Launch Point is not assigned for this ball.");
            return;
        }

        // Destroy existing ball if any
        if (currentBall != null)
        {
            if (!currentBall.Equals(null))
            {
                Debug.Log("Destroying existing ball...");
                Destroy(currentBall);
            }
            else
            {
                Debug.LogWarning("currentBall reference exists but the object is already destroyed.");
                currentBall = null;
            }
        }

        // Instantiate the ball at the LaunchPoint's position
        GameObject newBall = Instantiate(selectedBallPrefab.prefab, selectedBallPrefab.launchPoint.position, Quaternion.identity);
        Debug.Log("Ball instantiated successfully.");

        // Set the layer to "Enemy Balls"
        newBall.layer = LayerMask.NameToLayer("Enemy Balls");

        currentBall = newBall;

        // Change the color to red to indicate it was shot by the enemy
        Renderer ballRenderer = currentBall.GetComponent<Renderer>();
        if (ballRenderer != null)
        {
            ballRenderer.material.color = Color.red;
        }
        else
        {
            Debug.LogWarning("Ball prefab does not have a Renderer. Skipping color change.");
        }

        // Ensure the ball has a Rigidbody component
        Rigidbody ballRigidbody = currentBall.GetComponent<Rigidbody>();
        if (ballRigidbody == null)
        {
            Debug.LogWarning("Ball prefab missing Rigidbody. Adding one dynamically.");
            ballRigidbody = currentBall.AddComponent<Rigidbody>();
        }

        // Set the target tag for the ball collision based on the purpose of the ball
        BallCollision ballCollision = currentBall.GetComponent<BallCollision>();
        if (ballCollision != null)
        {
            ballCollision.targetTag = selectedBallPrefab.targetTag;
            Debug.Log($"BallCollision target tag set to: {selectedBallPrefab.targetTag}");
        }

        // Determine the target for the ball
        Transform targetTransform = null;
        Vector3 targetPosition;

        if (selectedBallPrefab.playerTarget != null)
        {
            // Use the assigned playerTarget Transform if available
            targetTransform = selectedBallPrefab.playerTarget;
            targetPosition = targetTransform.position;
        }
        else
        {
            // Otherwise, find the player using the tag specified by "targetTag" (default is "PlayerPosition")
            GameObject player = GameObject.FindGameObjectWithTag(selectedBallPrefab.targetTag);
            if (player == null)
            {
                Debug.LogError($"Target with tag '{selectedBallPrefab.targetTag}' not found.");
                return;
            }
            targetTransform = player.transform;
            targetPosition = targetTransform.position;
        }

        // Calculate direction towards the target (player)
        Vector3 direction = (targetPosition - selectedBallPrefab.launchPoint.position).normalized;
        Debug.Log($"Launch Direction: {direction}");

        // Apply initial velocities to create an arc
        ballRigidbody.velocity = direction * selectedBallPrefab.launchForce + Vector3.up * selectedBallPrefab.upwardForce;
        Debug.Log($"Initial Velocity Applied: {ballRigidbody.velocity}");

        // Apply rolling torque to the ball based on rollFactor
        Vector3 rollDirection = Vector3.Cross(Vector3.up, direction).normalized;
        ballRigidbody.AddTorque(rollDirection * selectedBallPrefab.rollFactor, ForceMode.Impulse);
        Debug.Log($"Rolling Torque Applied: {rollDirection * selectedBallPrefab.rollFactor}");

        // Start the homing coroutine
        if (homingCoroutine != null)
        {
            StopCoroutine(homingCoroutine);
        }
        homingCoroutine = StartCoroutine(HomingCoroutine(ballRigidbody, selectedBallPrefab, targetPosition, targetTransform));
    }

    // Coroutine to handle homing behavior
    IEnumerator HomingCoroutine(Rigidbody ballRigidbody, EnemyBallPrefab selectedBallPrefab, Vector3 initialTargetPosition, Transform targetTransform)
    {
        float elapsedTime = 0f;

        while (elapsedTime < selectedBallPrefab.homingDuration)
        {
            // Check if the ballRigidbody has been destroyed
            if (ballRigidbody == null)
            {
                Debug.LogWarning("Ball Rigidbody has been destroyed. Stopping homing coroutine.");
                yield break;
            }

            // Update current target position (in case the player moves)
            Vector3 currentTargetPosition = targetTransform.position;

            // Calculate direction towards the target
            Vector3 directionToTarget = (currentTargetPosition - ballRigidbody.position).normalized;

            // Apply homing force
            ballRigidbody.velocity += directionToTarget * selectedBallPrefab.homingForce * Time.deltaTime;

            // Clamp the velocity to prevent it from becoming too fast
            ballRigidbody.velocity = Vector3.ClampMagnitude(ballRigidbody.velocity, selectedBallPrefab.launchForce * 2);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    void ResetLaunch()
    {
        canLaunch = true;
    }
}
