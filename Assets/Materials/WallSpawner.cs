using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    [Header("Level 1 Settings")]
    public GameObject wallPrefab;
    public float wallSpeed = 1f;

    private GameObject currentWall;
    private bool wallSpawned = false;

    private void Start()
    {
        SpawnSingleWall();
    }

    private void SpawnSingleWall()
    {
        if (wallSpawned) return;

        // Fixed spawn position
        Vector3 spawnPosition = new Vector3(-1.5f, 0f, 1.5f);

        // No rotation
        Quaternion spawnRotation = Quaternion.identity;

        // Create the wall
        currentWall = Instantiate(wallPrefab, spawnPosition, spawnRotation);

        // Configure simple movement
        ConfigureWallMovement(currentWall);

        wallSpawned = true;

        Debug.Log($"Level 1 wall spawned at {spawnPosition}");
    }

    private void ConfigureWallMovement(GameObject wallObject)
    {
        wall wallScript = wallObject.GetComponent<wall>();
        if (wallScript == null) return;

        // Fixed points: from (-1.5, 0, 1.5) to (1.5, 0, 1.5)
        wallScript.pointA = new Vector3(-1.5f, 0f, 1.5f);
        wallScript.pointB = new Vector3(1.5f, 0f, 1.5f);
        wallScript.speed = wallSpeed;

        Debug.Log($"Wall configured: A={wallScript.pointA}, B={wallScript.pointB}, Speed={wallScript.speed}");
    }

    // Method to respawn the wall if it is destroyed
    public void RespawnWall()
    {
        if (currentWall == null)
        {
            wallSpawned = false;
            SpawnSingleWall();
        }
    }

    // Method to destroy the current wall
    public void DestroyCurrentWall()
    {
        if (currentWall != null)
        {
            Destroy(currentWall);
            currentWall = null;
            wallSpawned = false;
            Debug.Log("Wall destroyed");
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Display the wall's path
        Gizmos.color = Color.green;
        Vector3 pointA = new Vector3(-1.5f, 0f, 1.5f);
        Vector3 pointB = new Vector3(1.5f, 0f, 1.5f);

        Gizmos.DrawSphere(pointA, 0.2f);
        Gizmos.DrawSphere(pointB, 0.2f);
        Gizmos.DrawLine(pointA, pointB);
    }
}