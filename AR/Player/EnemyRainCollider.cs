using UnityEngine;
using TMPro;

public class EnemyRainCollider : MonoBehaviour
{
    [Header("Collision Settings")]
    public string waterbombTag = "Waterbomb"; // Tag assigned to waterbombs

    [Header("UI Elements (Optional)")]
    public TextMeshProUGUI collisionCountText1;
    public TextMeshProUGUI collisionCountText2;

    public void ResetOverlapCount()
    {
        GameState.Instance.EnemyInWaterBombCount = 0;
        UpdateOverlapCountUI();
    }

    // Called when a waterbomb starts overlapping with the player
    void OnTriggerEnter(Collider other)
    {
        // Check if the player is active and the other collider has the waterbomb tag
        if (gameObject.activeInHierarchy && other.gameObject.CompareTag(waterbombTag))
        {
            // Increment the overlap count
            GameState.Instance.EnemyInWaterBombCount += 1;

            // Log the overlap
            Debug.Log($"Waterbomb entered overlap. Current overlaps: {GameState.Instance.EnemyInWaterBombCount}");

            // Update the UI if necessary
            UpdateOverlapCountUI();
        }
    }

    // Called when a waterbomb stops overlapping with the player
    void OnTriggerExit(Collider other)
    {
        // Check if the player is active and the other collider has the waterbomb tag
        if (gameObject.activeInHierarchy && other.gameObject.CompareTag(waterbombTag))
        {
            // Decrement the game state's enemyInWaterBombCount
            GameState.Instance.EnemyInWaterBombCount -= 1;

            // Log the overlap
            Debug.Log($"Waterbomb exited overlap. Current overlaps: {GameState.Instance.EnemyInWaterBombCount}");

            // Update the UI if necessary
            UpdateOverlapCountUI();
        }
    }

    // Method to update the overlap count on the UI
    void UpdateOverlapCountUI()
    {
        if (collisionCountText1 != null && collisionCountText2 != null)
        {
            collisionCountText1.text = $"Current Rainbomb Collisions: {GameState.Instance.EnemyInWaterBombCount}";
            collisionCountText2.text = $"in {GameState.Instance.EnemyInWaterBombCount} rainbomb(s)";

            if (GameState.Instance.EnemyInWaterBombCount > 0)
            {
                // white text
                collisionCountText1.color = Color.green;
                collisionCountText2.color = Color.green;
            }
            else
            {
                collisionCountText1.color = Color.white;
                collisionCountText2.color = Color.white;
            }
        }
    }
}