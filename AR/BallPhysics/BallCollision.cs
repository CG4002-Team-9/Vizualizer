using UnityEngine;

public class BallCollision : MonoBehaviour
{
    [Header("Target Settings")]
    public string targetTag = "PlayerTarget";  // Set the tag to determine the target (e.g., "playerPosition" or "PlayerTarget")

    private bool isBeingDestroyed = false;

    void Start()
    {
        // Schedule the destruction of the ball after 4 seconds
        Invoke(nameof(DestroyBall), 4f);
        Debug.Log("Ball will be destroyed in 4 seconds if no collision occurs.");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBeingDestroyed) return;

        Debug.Log("Collision detected with: " + collision.gameObject.name);  // Log the name of the collided object

        if (collision.gameObject.CompareTag(targetTag))  // Check if the collided object matches the target tag
        {
            Debug.Log($"Collided with {targetTag}. Destroying ball.");
            DestroyBall();
        }
    }

    void DestroyBall()
    {
        if (isBeingDestroyed) return;

        isBeingDestroyed = true;
        CancelInvoke();  // Cancel any pending invokes
        Destroy(gameObject);  // Destroy the ball
        Debug.Log("Ball destroyed.");
    }

    void OnDestroy()
    {
        // Ensure no further operations are performed on the destroyed object
        isBeingDestroyed = true;
        CancelInvoke();
        Debug.Log("Ball OnDestroy called.");
    }

    void OnDisable()
    {
        // Ensure no further operations are performed on the disabled object
        isBeingDestroyed = true;
        CancelInvoke();
        Debug.Log("Ball OnDisable called.");
    }
}
