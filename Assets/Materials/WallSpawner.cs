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

        // Position de spawn fixe
        Vector3 spawnPosition = new Vector3(-1.5f, 0f, 1.5f);

        // Pas de rotation
        Quaternion spawnRotation = Quaternion.identity;

        // Créer le mur
        currentWall = Instantiate(wallPrefab, spawnPosition, spawnRotation);

        // Configurer le mouvement simple
        ConfigureWallMovement(currentWall);

        wallSpawned = true;

        Debug.Log($"Mur Level 1 spawné à {spawnPosition}");
    }

    private void ConfigureWallMovement(GameObject wallObject)
    {
        wall wallScript = wallObject.GetComponent<wall>();
        if (wallScript == null) return;

        // Points fixes : de (-1.5, 0, 1.5) vers (1.5, 0, 1.5)
        wallScript.pointA = new Vector3(-1.5f, 0f, 1.5f);
        wallScript.pointB = new Vector3(1.5f, 0f, 1.5f);
        wallScript.speed = wallSpeed;

        Debug.Log($"Mur configuré: A={wallScript.pointA}, B={wallScript.pointB}, Speed={wallScript.speed}");
    }

    // Méthode pour respawn le mur si il est détruit
    public void RespawnWall()
    {
        if (currentWall == null)
        {
            wallSpawned = false;
            SpawnSingleWall();
        }
    }

    // Méthode pour détruire le mur actuel
    public void DestroyCurrentWall()
    {
        if (currentWall != null)
        {
            Destroy(currentWall);
            currentWall = null;
            wallSpawned = false;
            Debug.Log("Mur détruit");
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        // Afficher le trajet du mur
        Gizmos.color = Color.green;
        Vector3 pointA = new Vector3(-1.5f, 0f, 1.5f);
        Vector3 pointB = new Vector3(1.5f, 0f, 1.5f);

        Gizmos.DrawSphere(pointA, 0.2f);
        Gizmos.DrawSphere(pointB, 0.2f);
        Gizmos.DrawLine(pointA, pointB);
    }
}