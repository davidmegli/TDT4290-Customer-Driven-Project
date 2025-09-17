using UnityEngine;

public class wall : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 pointA;
    public Vector3 pointB;
    public float speed = 0.1f;

    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public AudioClip explosionSound;
    public float explosionVolume = 1f;

    [Header("Collision Settings")]
    [Tooltip("Distance à la surface du mur pour déclencher l'explosion")]
    public float surfaceDetectionDistance = 0.1f;

    [HideInInspector] public Vector3 target;

    private bool hasExploded = false;
    private AudioSource audioSource;
    private Camera playerCamera;
    private Collider wallCollider;

    void Start()
    {
        // Initialisation du mouvement
        transform.position = pointA;
        target = pointB;

        // Trouver la caméra
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();

        // Setup des composants
        SetupAudioSource();
        SetupCollider();
    }

    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = explosionVolume;
        audioSource.spatialBlend = 1f;
        audioSource.spatialize = false;
    }

    void SetupCollider()
    {
        wallCollider = GetComponent<Collider>();
        if (wallCollider == null)
        {
            // Ajouter un BoxCollider par défaut si aucun collider n'existe
            wallCollider = gameObject.AddComponent<BoxCollider>();
        }

        // Le collider reste normal (pas trigger) pour une détection de surface précise
        wallCollider.isTrigger = false;
    }

    void Update()
    {
        if (!hasExploded)
        {
            MoveWall();
            CheckSurfaceCollision();
        }
    }

    void MoveWall()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = target == pointA ? pointB : pointA;
        }
    }

    void CheckSurfaceCollision()
    {
        if (hasExploded || playerCamera == null || wallCollider == null) return;

        // Calcul de la distance RÉELLE à la surface du mur
        Vector3 cameraPosition = playerCamera.transform.position;
        Vector3 closestPointOnWall = wallCollider.ClosestPoint(cameraPosition);
        float distanceToSurface = Vector3.Distance(cameraPosition, closestPointOnWall);

        // Debug visuel pour voir la détection
        Debug.DrawLine(cameraPosition, closestPointOnWall, Color.red, 0.1f);

        // Si trop proche de la surface
        if (distanceToSurface <= surfaceDetectionDistance)
        {
            Debug.Log($"Collision détectée ! Distance à la surface: {distanceToSurface:F3}m");
            TriggerExplosion();
        }
    }

    void TriggerExplosion()
    {
        if (hasExploded) return;

        hasExploded = true;
        Debug.Log("💥 Explosion du mur !");

        // Jouer le son
        if (explosionSound != null && audioSource != null)
        {
            audioSource.clip = explosionSound;
            audioSource.Play();
        }

        // Créer l'effet d'explosion
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5f);
        }
        else
        {
            CreateSimpleExplosionEffect();
        }

        // Détruire le mur après le son
        float destroyDelay = explosionSound != null ? explosionSound.length : 1f;
        Destroy(gameObject, destroyDelay);
    }

    void CreateSimpleExplosionEffect()
    {
        GameObject explosion = new GameObject("WallExplosion");
        explosion.transform.position = transform.position;

        ParticleSystem particles = explosion.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 2f;
        main.startSpeed = 5f;
        main.startSize = 0.5f;
        main.startColor = Color.red;
        main.maxParticles = 50;

        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;

        Destroy(explosion, 3f);
    }

    // Méthodes publiques utiles
    public void ForceExplosion()
    {
        TriggerExplosion();
    }

    public void SetMovementPoints(Vector3 newPointA, Vector3 newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
        target = pointB;
    }

    // Visualisation dans l'éditeur
    void OnDrawGizmosSelected()
    {
        // Ligne de trajectoire
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pointA, pointB);

        // Points A et B
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pointA, 0.2f);
        Gizmos.DrawWireSphere(pointB, 0.2f);

        // Zone de détection autour de la caméra
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCamera.transform.position, surfaceDetectionDistance);
        }
    }
}
