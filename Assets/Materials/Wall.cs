using UnityEngine;

public class wall : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 pointA;
    public Vector3 pointB;
    public float speed = 0.1f;

    [Header("Pause Settings")]
    [Tooltip("How long the wall should pause at each endpoint before reversing (seconds).")]
    public float endStopDuration = 0f;

    [Header("Scraping Sound Settings")]
    [Tooltip("Assign both scraping AudioSources (e.g., floor and ceiling).")]
    public AudioSource[] scrapingAudioSources;

    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public AudioClip explosionSound;
    public float explosionVolume = 1f;

    [Header("Collision Settings")]
    [Tooltip("Distance to the wall surface that triggers the explosion")]
    public float surfaceDetectionDistance = 0.1f;

    [HideInInspector] public Vector3 target;

    private bool hasExploded = false;
    private AudioSource audioSource;
    private Camera playerCamera;
    private Collider wallCollider;

    // Pause state
    private bool isPausing = false;
    private float pauseRemaining = 0f;
    private const float targetReachThreshold = 0.1f;

    void Start()
    {
        // Initialize movement
        transform.position = pointA;
        target = pointB;

        // Find the camera
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();

        // Setup components
        SetupAudioSource();
        SetupCollider();

        // Ensure all scraping sounds start playing and loop
        foreach (var src in scrapingAudioSources)
        {
            if (src != null)
            {
                src.loop = true;
                if (!src.isPlaying) src.Play();
            }
        }
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
            // Add a default BoxCollider if none exists
            wallCollider = gameObject.AddComponent<BoxCollider>();
        }

        // Keep collider non-trigger for precise surface detection
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
        // If we are currently pausing at an endpoint, count down and do not move
        if (isPausing)
        {
            pauseRemaining -= Time.deltaTime;
            if (pauseRemaining <= 0f)
            {
                isPausing = false;
                // After the pause, flip the target and resume movement
                target = (target == pointA) ? pointB : pointA;
                SetScrapingSoundsActive(true);
            }
            return;
        }

        // Move toward the current target
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // If we reached the target, start a pause (if any) before switching direction
        if (Vector3.Distance(transform.position, target) < targetReachThreshold)
        {
            if (endStopDuration > 0f)
            {
                isPausing = true;
                pauseRemaining = endStopDuration;
                SetScrapingSoundsActive(false);
            }
            else
            {
                // No pause configured: immediately switch target (original behavior)
                target = (target == pointA) ? pointB : pointA;
            }
        }
    }

    // Utility to pause/unpause all assigned scraping sounds
    void SetScrapingSoundsActive(bool active)
    {
        foreach (var src in scrapingAudioSources)
        {
            if (src == null) continue;
            if (active)
            {
                if (!src.isPlaying) src.UnPause();
            }
            else
            {
                src.Pause();
            }
        }
    }

    void CheckSurfaceCollision()
    {
        if (hasExploded || playerCamera == null || wallCollider == null) return;

        // Calculate REAL distance to the wall surface
        Vector3 cameraPosition = playerCamera.transform.position;
        Vector3 closestPointOnWall = wallCollider.ClosestPoint(cameraPosition);
        float distanceToSurface = Vector3.Distance(cameraPosition, closestPointOnWall);

        // Visual debug for detection
        Debug.DrawLine(cameraPosition, closestPointOnWall, Color.red, 0.1f);

        // If too close to the surface
        if (distanceToSurface <= surfaceDetectionDistance)
        {
            Debug.Log($"Collision detected! Distance to surface: {distanceToSurface:F3}m");
            TriggerExplosion();
        }
    }

    void TriggerExplosion()
    {
        if (hasExploded) return;

        hasExploded = true;
        Debug.Log("💥 Wall explosion!");

        // Play sound
        if (explosionSound != null && audioSource != null)
        {
            audioSource.clip = explosionSound;
            audioSource.Play();
        }

        // Create explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5f);
        }
        else
        {
            CreateSimpleExplosionEffect();
        }

        // Destroy wall after the sound finishes (or default delay)
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

    // Useful public methods
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

    // Editor visualization
    void OnDrawGizmosSelected()
    {
        // Motion path line
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pointA, pointB);

        // Points A and B
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pointA, 0.2f);
        Gizmos.DrawWireSphere(pointB, 0.2f);

        // Detection zone around the camera
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCamera.transform.position, surfaceDetectionDistance);
        }
    }
}
