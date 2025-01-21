using UnityEngine;
using Vuforia;

public class RainBombCollision : MonoBehaviour
{
    public GameObject rainCloudPrefab; // Assign in the Inspector

    private bool isBeingDestroyed = false;

    private GameObject ownPlayer;

    public float rainbombRadius = 0.5f;

    void Start()
    {
        // Schedule the destruction of the ball after 4 seconds
        Invoke(nameof(DestroyBall), 4f);
        Debug.Log("Ball will be destroyed in 4 seconds if no collision occurs.");

        ownPlayer = GameObject.FindGameObjectWithTag("PlayerPosition");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBeingDestroyed) return;
        if (!collision.gameObject.CompareTag("PlayerTarget")) return;

        Debug.Log("Collision detected with: " + collision.gameObject.name);

        // Get the contact point of the collision
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionPoint = contact.point;
        Vector3 playerPosition = ownPlayer.transform.position;

        Debug.Log("Player point: " + playerPosition);
        Debug.Log("Collision point: " + collisionPoint);

        // Calculate the direction vector on the x-z plane from the player to the collision point
        Vector3 direction = collisionPoint - playerPosition;
        direction[1] = 0; // Ignore the y-axis to only move in the horizontal plane
        direction.Normalize();

        // Calculate the position behind the collision point
        Vector3 behindPosition = collisionPoint + (direction * rainbombRadius);

        // Use the collisionPoint.y to keep the original y value
        behindPosition[1] = collisionPoint[1] + 0.5f;

        // Instantiate and anchor the rain cloud at the calculated position
        InstantiateRainCloud(behindPosition);

        // Destroy the water bomb
        DestroyBall();
    }

    void InstantiateRainCloud(Vector3 position)
    {
        if (rainCloudPrefab == null)
        {
            Debug.LogError("Rain Cloud Prefab is not assigned.");
            return;
        }

        // Instantiate the rain cloud at the collision point
        GameObject rainCloud = Instantiate(rainCloudPrefab, position, Quaternion.identity);

        // Anchor the rain cloud using Vuforia's Ground Plane
        AnchorRainCloud(rainCloud, position);
    }

    void AnchorRainCloud(GameObject rainCloud, Vector3 position)
    {
        // Add the Content Positioning Behaviour
        ContentPositioningBehaviour contentPositioning = FindObjectOfType<ContentPositioningBehaviour>();
        if (contentPositioning == null)
        {
            Debug.LogError("ContentPositioningBehaviour not found in the scene.");
            return;
        }
        
        contentPositioning.PositionContentAtMidAirAnchor(rainCloud.transform);
    }

    void DestroyBall()
    {
        if (isBeingDestroyed) return;

        isBeingDestroyed = true;
        CancelInvoke();
        Destroy(gameObject);
        Debug.Log("Ball destroyed.");
    }

    void OnDestroy()
    {
        isBeingDestroyed = true;
        CancelInvoke();
        Debug.Log("Ball OnDestroy called.");
    }

    void OnDisable()
    {
        isBeingDestroyed = true;
        CancelInvoke();
        Debug.Log("Ball OnDisable called.");
    }
}