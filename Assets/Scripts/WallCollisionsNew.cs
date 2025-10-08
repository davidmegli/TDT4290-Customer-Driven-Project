using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallCollisionsNew : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip collisionSound; // Game Over-lyd
    private AudioSource audioSource;

    [Header("Filter")]
    public string[] triggeringTags = { "PlayerHand", "Player" };

    [Header("Contact Settings")]
    [Tooltip("Game over utløses først når hånda er dette nære (meter). 0.02 = 2 cm.")]
    public float gameOverDistance = 0.02f;

    private Collider wallCol;

    void Awake()
    {
        wallCol = GetComponent<Collider>();
        if (!wallCol.isTrigger)
        {
            Debug.LogWarning("[WallCollisions] Sett vegg-collider til IsTrigger = true for VR-berøring uten fysisk dytt.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerTag(other)) return;

        // Mål reell avstand mellom vegg og colliderns senter
        Vector3 otherPoint = other.bounds.center;
        Vector3 closestOnWall = wallCol.ClosestPoint(otherPoint);
        float dist = Vector3.Distance(closestOnWall, otherPoint);

        // Debug: se tall i Console
        Debug.Log($"[WallCollisions] {other.name} dist={dist:F3} m");

        // Bare når vi er "helt inntil" (<= 2 cm) utløser vi game over
        if (dist <= gameOverDistance)
        {
            PlayCollisionSound();
            Debug.Log($"[WallCollisions] GAME OVER på {other.name} (dist={dist:F3} m)");
            // TODO: game over-logikk her (disable, restart, scene load osv.)
        }
    }

    private bool IsPlayerTag(Collider other)
    {
        for (int i = 0; i < triggeringTags.Length; i++)
            if (other.CompareTag(triggeringTags[i])) return true;
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
