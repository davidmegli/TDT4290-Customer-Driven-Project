using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallProximityWarning : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip warningClip;         // stemme-klippet
    private AudioSource audioSource;

    [Header("Tags")]
    public string[] triggeringTags = { "PlayerHand", "Player" };

    [Header("Timing")]
    [Tooltip("Minimum tid mellom varsel (sekunder).")]
    public float warningCooldown = 1.2f;

    [Tooltip("Spill kun én gang per innpassering (til man forlater sonen).")]
    public bool playOncePerEntry = true;

    [Header("Hysterese (valgfritt)")]
    [Tooltip("Start varsel når avstand til veggflaten er <= denne verdien (meter). Bruk 0.35f som start).")]
    public float warnEnter = 0.35f;

    [Tooltip("Slutt varsel når avstand > denne verdien (meter). Sett litt større enn warnEnter, f.eks. 0.45f.")]
    public float warnExit = 0.45f;

    [Header("Vegg-geo (for mer presis avstand)")]
    [Tooltip("Transform til selve veggen (root). Brukes til å finne normal og posisjon.")]
    public Transform wallRoot;

    [Tooltip("Veggtykkelse i VERDENSMÅL (meter). Hos dere ~0.10f).")]
    public float wallThicknessWorld = 0.10f;

    private float _nextBeepTime = 0f;
    private bool _activeNear = false; // inne i "nær"-tilstand (mellom warnEnter og warnExit)
    private HashSet<int> _insideIds = new HashSet<int>(); // unike collidere inne i sonen

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;   // 3D
        audioSource.dopplerLevel = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerTag(other)) return;
        _insideIds.Add(other.GetInstanceID());
        UpdateNearState(other, true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerTag(other)) return;
        UpdateNearState(other, false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerTag(other)) return;

        _insideIds.Remove(other.GetInstanceID());
        if (_insideIds.Count == 0)
        {
            // Ute av sonen: reset
            _activeNear = false;
            _nextBeepTime = 0f;
            Debug.Log("[Proximity] EXIT all -> reset state");
        }
    }

    private void UpdateNearState(Collider referenceCollider, bool isEnter)
    {
        // Finn gap til veggflaten hvis vi har wallRoot; ellers fallback til "i sonen = nær"
        float gap = GetGapToWall(referenceCollider);
        bool shouldBeActive = gap >= 0f ? (gap <= warnEnter) : true; // om vi klarer å beregne gap

        // Hysterese
        if (_activeNear)
        {
            if (gap >= 0f && gap > warnExit) _activeNear = false;
        }
        else
        {
            if (shouldBeActive) _activeNear = true;
        }

        // Spill lyd?
        if (_activeNear)
        {
            if (playOncePerEntry)
            {
                // Spill kun ved første enter i "nær"-tilstand
                if (isEnter && Time.time >= _nextBeepTime)
                    PlayWarn();
            }
            else
            {
                // Periodisk, men ikke for ofte
                if (Time.time >= _nextBeepTime)
                    PlayWarn();
            }
        }
    }

    private float GetGapToWall(Collider other)
    {
        if (wallRoot == null) return -1f;

        // Anta at veggens "foran" er wallRoot.forward
        Vector3 normal = wallRoot.forward.normalized;

        // Veggflaten (foran) ligger halv veggtykkelse fra senter
        Vector3 wallSurfacePoint = wallRoot.position + normal * (wallThicknessWorld * 0.5f);

        // Signert avstand fra colliders senter til planet
        Plane wallPlane = new Plane(normal, wallSurfacePoint);
        float signedDist = wallPlane.GetDistanceToPoint(other.bounds.center); // >0 foran flaten

        // Vi bryr oss om avstanden foran (>=0). Negativ betyr "bak flaten".
        return Mathf.Max(0f, signedDist);
    }

    private void PlayWarn()
    {
        if (warningClip == null)
        {
            Debug.LogWarning("[Proximity] Ingen warningClip satt!");
            return;
        }

        audioSource.PlayOneShot(warningClip);
        _nextBeepTime = Time.time + warningCooldown;
        Debug.Log("[Proximity] WARNING voice played");
    }

    private bool IsPlayerTag(Collider other)
    {
        for (int i = 0; i < triggeringTags.Length; i++)
            if (other.CompareTag(triggeringTags[i])) return true;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (wallRoot == null) return;
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.35f);
        Vector3 normal = wallRoot.forward.normalized;
        Vector3 surf = wallRoot.position + normal * (wallThicknessWorld * 0.5f);
        Gizmos.DrawRay(surf, normal * 0.35f);
    }
#endif
}
