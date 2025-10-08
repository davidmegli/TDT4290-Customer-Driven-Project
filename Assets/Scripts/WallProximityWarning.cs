using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallProximityWarning : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip warningBeep;
    private AudioSource audioSource;

    [Header("Behaviour")]
    [Tooltip("Sekunder mellom hvert varsel mens spilleren er i nærheten.")]
    public float warningCooldown = 0.7f;

    [Tooltip("Hvilke tags som utløser advarsel (f.eks. spillerhender/kropp).")]
    public string[] triggeringTags = { "PlayerHand", "Player" };

    private float _nextBeepTime = 0f;
    private int _insideCount = 0; // hvor mange spiller-collidere som er inne i sonen

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("[WallProximityWarning] Collider må ha IsTrigger = true.");
            col.isTrigger = true;
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

        _insideCount++;
        TryBeep(); // gi umiddelbar første pip når man går inn i sonen
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerTag(other)) return;

        TryBeep(); // repeter med cooldown så lenge man er i nærheten
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerTag(other)) return;

        _insideCount = Mathf.Max(0, _insideCount - 1);
        if (_insideCount == 0)
        {
            // Reset gjerne så det piper raskt neste gang man kommer tilbake
            _nextBeepTime = 0f;
        }
    }

    private bool IsPlayerTag(Collider other)
    {
        for (int i = 0; i < triggeringTags.Length; i++)
        {
            if (other.CompareTag(triggeringTags[i])) return true;
        }
        return false;
    }

    private void TryBeep()
    {
        if (Time.time < _nextBeepTime) return;

        if (warningBeep != null)
        {
            audioSource.PlayOneShot(warningBeep);
            _nextBeepTime = Time.time + warningCooldown;
        }
        else
        {
            Debug.LogWarning("[WallProximityWarning] Ingen warningBeep satt i Inspector!");
        }
    }

#if UNITY_EDITOR
    // Hjelper deg å “se” sonen i editor
    private void OnDrawGizmosSelected()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
        if (col is BoxCollider bc)
        {
            Gizmos.DrawCube(bc.center, bc.size);
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.8f);
            Gizmos.DrawWireCube(bc.center, bc.size);
        }
    }
#endif
}
