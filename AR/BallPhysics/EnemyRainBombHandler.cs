using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRainBombHandler : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the rain bomb prefab to spawn
    public GameObject rainCloudPrefab;

    // Reference to the player's position
    private GameObject playerObject;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Reference the player object in the scene
        playerObject = GameObject.FindGameObjectWithTag("PlayerPosition");
    }

    // This method will be called from the Inspector to handle enemy game actions
    public void HandleEnemyBomb(string actionType)
    {
        // Check if the action is "bomb" and if the player is visible to the enemy
        if (actionType == "bomb" && gameState.PlayerVisibleToEnemy)
        {
            // Spawn the rain bomb on the player
            SpawnRainBombOnPlayer();
        }
    }

    private void SpawnRainBombOnPlayer()
    {
        if (playerObject == null)
        {
            Debug.LogError("Player object not found.");
            return;
        }

        if (rainCloudPrefab == null)
        {
            Debug.LogError("Rain Cloud Prefab is not assigned.");
            return;
        }

        // Get the player's current position
        Vector3 playerPosition = playerObject.transform.position;


        Vector3 spawnPosition = playerPosition;

        // Instantiate the rain cloud at the calculated position
        InstantiateRainCloud(spawnPosition);
    }

    private void InstantiateRainCloud(Vector3 position)
    {
        // Instantiate the rain cloud at the specified position
        GameObject rainCloud = Instantiate(rainCloudPrefab, position, Quaternion.identity);
        Debug.Log("Rain cloud instantiated at position: " + position);
    }
}
