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

    [Tooltip("Må ut igjen minst så mye før vi kan trigge på nytt (meter).")]
    public float releaseDistance = 0.05f;   // litt større enn gameOverDistance

    [Tooltip("Minimum tid mellom to avspillinger (sekunder).")]
    public float minRepeatInterval = 1.0f;

    private Collider wallCol;
    private float lastPlayTime = -999f;
    private bool latched = false;

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
        audioSource.spatialize = true;     // se sjekkliste under hvis du vil teste uten spatializer
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.reverbZoneMix = 0f;    // viktig for å unngå “ekko” fra reverb zones
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerTag(other)) return;

        Vector3 p = other.bounds.center;
        float dist = Vector3.Distance(wallCol.ClosestPoint(p), p);

        // Latchet: venter til vi er godt ute igjen
        if (latched)
        {
            if (dist > releaseDistance) latched = false;
            return;
        }

        // Ikke latchet: trigger når vi er tett nok OG cooldown er over
        if (dist <= gameOverDistance && Time.time - lastPlayTime >= minRepeatInterval)
        {
            PlayCollisionSound();
            lastPlayTime = Time.time;
            latched = true; // ikke spam i OnTriggerStay
            Debug.Log($"[WallCollisions] GAME OVER (dist={dist:F3} m)");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // når alt er ute, slippe latch uansett
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
