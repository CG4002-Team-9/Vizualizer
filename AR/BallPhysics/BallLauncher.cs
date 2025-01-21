using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BallPrefab
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
    public string targetTag = "PlayerTarget"; // Default target tag ("PlayerPosition" for player or "PlayerTarget" for enemy)
}

public class BallLauncher : MonoBehaviour
{
    [Header("Ball Settings")]
    public List<BallPrefab> ballPrefabsList = new List<BallPrefab>(); // Assign the Ball Prefabs in the Inspector
    private Dictionary<string, BallPrefab> ballPrefabs = new Dictionary<string, BallPrefab>();

    [Header("Launch Cooldown")]
    public float launchCooldown = 0.5f;      // Time between launches
    private bool canLaunch = true;

    private GameObject currentBall;
    private Coroutine homingCoroutine;

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

    // Method to launch the ball
    public void LaunchBall(string ballType)
    {
        if (!canLaunch)
        {
            Debug.Log("Launch is on cooldown.");
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

        BallPrefab selectedBallPrefab = ballPrefabs[ballType];

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

        currentBall = newBall;

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

        // Calculate targetPosition and targetTransform
        Vector3 targetPosition;
        Transform targetTransform = null;

        if (!GameState.Instance.EnemyActive)
        {
            // When enemy is not active, set the target position to 4 units in front of the camera
            targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 4;
        }
        else
        {
            // When enemy is active, use the playerTarget's position
            if (selectedBallPrefab.playerTarget == null)
            {
                Debug.LogError("Player Target is not assigned for this ball.");
                return;
            }
            targetTransform = selectedBallPrefab.playerTarget;
            targetPosition = targetTransform.position;
        }

        // Calculate direction towards the target
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
    IEnumerator HomingCoroutine(Rigidbody ballRigidbody, BallPrefab selectedBallPrefab, Vector3 initialTargetPosition, Transform targetTransform)
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

            Vector3 currentTargetPosition = initialTargetPosition;

            if (GameState.Instance.EnemyActive && targetTransform != null)
            {
                currentTargetPosition = targetTransform.position;
            }

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
