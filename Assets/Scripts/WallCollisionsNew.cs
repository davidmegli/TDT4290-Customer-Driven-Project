using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallCollisionsNew : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip collisionSound; 
    private AudioSource audioSource;

    [Header("Filter")]
    public string[] triggeringTags = { "PlayerHand", "Player", "PlayerHead"};

    [Header("Contact Settings")]
    [Tooltip("Game over triggers when hand is near (meter). 0.02 = 2 cm.")]
    public float gameOverDistance = 0.02f;

    [Tooltip("Has to go out as much as meter to trigger.")]
    public float releaseDistance = 0.05f;   

    [Tooltip("Minimum time between two different triggers")]
    public float minRepeatInterval = 1.0f;

    private Collider wallCol;
    private float lastPlayTime = -999f;
    private bool latched = false;

    void Awake()
    {
        wallCol = GetComponent<Collider>();
        if (!wallCol.isTrigger)
        {
            Debug.LogWarning("[WallCollisions] sets veg-collider to Is-Trigger = true, for VR-touching without physical pressing it.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialize = true;     
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.reverbZoneMix = 0f;   
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerTag(other)) return;

        Vector3 p = other.bounds.center;
        float dist = Vector3.Distance(wallCol.ClosestPoint(p), p);

        // Latchet: wait to we are outside of the barrier
        if (latched)
        {
            if (dist > releaseDistance) latched = false;
            return;
        }

        // Not latchet: triggers when we are tight enough on the OG cooldown is over. 
        if (dist <= gameOverDistance && Time.time - lastPlayTime >= minRepeatInterval)
        {
            PlayCollisionSound();
            lastPlayTime = Time.time;
            latched = true; // dont spam in OnTriggerStay
            Debug.Log($"[WallCollisions] GAME OVER (dist={dist:F3} m)");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // when we are outside, release latch 
        latched = false;
    }

    private bool IsPlayerTag(Collider other)
    {
        foreach (var t in triggeringTags)
            if (other.CompareTag(t)) return true;
        return false;
    }

    private void PlayCollisionSound()
    {
        if (collisionSound != null)
            audioSource.PlayOneShot(collisionSound);
        else
            Debug.LogWarning("[WallCollisions] Ingen collisionSound satt i Inspector!");
    }
}
