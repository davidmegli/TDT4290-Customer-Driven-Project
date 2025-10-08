using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallCollisions : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip collisionSound; // Game Over-lyd
    private AudioSource audioSource;

    [Header("Filter")]
    [Tooltip("Hvilke tags som får trigge kollisjon (f.eks. spillerhender/kropp).")]
    public string[] triggeringTags = { "PlayerHand", "Player" };

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("[WallCollisions] Collider bør være IsTrigger = true for å fange berøring uten fysikkpress.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerTag(other)) return;

        PlayCollisionSound();
        Debug.Log($"[WallCollisions] Berørt av: {other.gameObject.name}");
        // TODO: Her kan du fyre av game-over logikk (load scene, disable input, osv.)
    }

    private bool IsPlayerTag(Collider other)
    {
        for (int i = 0; i < triggeringTags.Length; i++)
        {
            if (other.CompareTag(triggeringTags[i])) return true;
        }
        return false;
    }

    private void PlayCollisionSound()
    {
        if (collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }
        else
        {
            Debug.LogWarning("[WallCollisions] Ingen collisionSound satt i Inspector!");
        }
    }
}
