using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallProximityWarning : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip warningClip;   // Legg inn voice her
    private AudioSource audioSource;

    [Header("Behaviour")]
    public float warningCooldown = 0.7f;
    public string[] triggeringTags = { "PlayerHand", "Player" };

    private float _nextBeepTime = 0f;
    private int _insideCount = 0;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("[WallProximityWarning] Collider må være IsTrigger = true. Retter det.");
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
        Debug.Log($"[Proximity] ENTER: {other.name} (count={_insideCount})");
        TryWarn();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerTag(other)) return;
        TryWarn();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerTag(other)) return;
        _insideCount = Mathf.Max(0, _insideCount - 1);
        Debug.Log($"[Proximity] EXIT: {other.name} (count={_insideCount})");
        if (_insideCount == 0) _nextBeepTime = 0f;
    }

    private bool IsPlayerTag(Collider other)
    {
        for (int i = 0; i < triggeringTags.Length; i++)
            if (other.CompareTag(triggeringTags[i])) return true;
        return false;
    }

    private void TryWarn()
    {
        if (Time.time < _nextBeepTime) return;

        if (warningClip != null)
        {
            audioSource.PlayOneShot(warningClip);
            _nextBeepTime = Time.time + warningCooldown;
            Debug.Log("[Proximity] WARNING voice played");
        }
        else
        {
            Debug.LogWarning("[Proximity] Ingen warningClip satt!");
        }
    }

#if UNITY_EDITOR
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
