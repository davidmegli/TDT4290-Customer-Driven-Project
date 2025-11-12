using UnityEngine;

/// <summary>
/// WallCollisionsNew handles collision detection for walls in the game.
/// It plays audio feedback and manages game-over conditions based on proximity to the wall.
/// </summary>
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
    [Header("Load grace")]
    [Tooltip("Ignore collision/game-over for this many seconds after a level is loaded.")]
    public float ignoreAfterLoadSeconds = 0.5f;

    void Awake()
    {
        wallCol = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialize = true;     
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.reverbZoneMix = 0f;   
    }

    /// <summary>
    /// Triggered when another collider stays within the wall's trigger zone.
    /// Checks proximity to the wall and triggers game-over conditions if necessary.
    /// </summary>
    /// <param name="other">The collider that is staying within the trigger zone.</param>
    private void OnTriggerStay(Collider other)
    {
        // Ignore collisions for a short grace period right after a level is instantiated.
        if (Time.time - LevelManager.lastLoadTime < ignoreAfterLoadSeconds) return;

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
            latched = true; // dont spam in OnCollisionStay
        }
    }

    /// <summary>
    /// Triggered when another collider exits the wall's trigger zone.
    /// Resets the latch state to allow future triggers.
    /// </summary>
    /// <param name="other">The collider that exited the trigger zone.</param>
    private void OnTriggerExit(Collider other)
    {
        // when we are outside, release latch 
        latched = false;
    }

    /// <summary>
    /// Checks if the given collider has a tag that matches the triggering tags.
    /// </summary>
    /// <param name="other">The collider to check.</param>
    /// <returns>True if the collider's tag matches; otherwise, false.</returns>
    private bool IsPlayerTag(Collider other)
    {
        foreach (var t in triggeringTags)
            if (other.CompareTag(t)) return true;
        return false;
    }

    /// <summary>
    /// Plays the collision sound if an audio clip is assigned.
    /// </summary>
    private void PlayCollisionSound()
    {
        if (collisionSound != null)
            audioSource.PlayOneShot(collisionSound);
    }
}
