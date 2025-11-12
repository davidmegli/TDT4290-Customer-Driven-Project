using UnityEngine;

/// <summary>
/// HandTriggeredAudio is responsible for managing an audio source that reacts
/// to the proximity of a specified hand object (using colliders).
/// 
/// It dynamically positions the sound source at the closest point between
/// the "hand" and a target collider, and optionally adjusts the volume
/// based on the distance between them.
/// </summary>
public class HandTriggeredAudio : MonoBehaviour
{
    [Tooltip("BoxCollider of the cube the sound should 'come from'")]
    public BoxCollider sourceCollider;

    [Tooltip("Left or Right hand Cube object")]
    public BoxCollider handCollider;

    [Tooltip("The tag of the hand Cube")]
    public string targetTag;

    [Tooltip("On/off dynamic volume based on distance")]
    public bool useDistanceVolume = true;

    [Min(0f)]
    [Tooltip("Closest distance where volume is at max")]
    public float minDistance = 0f;

    [Min(0f)]
    [Tooltip("Furthest distance where volume is the lowest")]
    public float maxDistance = 0f;

    [Range(0f, 1f)]
    [Tooltip("Volume at minDistance")]
    public float volumeAtMinDistance = 1f;

    [Range(0f, 1f)]
    [Tooltip("Volume at maxDistance")]
    public float volumeAtMaxDistance = 0f;

    [Min(0f)]
    [Tooltip("How smooth the volume adjusts: 0 = no smoothing, higher = smoother")]
    public float volumeLerpSpeed = 10f;

    /// <summary>
    /// The current distance between the hand and the closest point on the source collider.
    /// </summary>
    public float CurrentDistance { get; private set; }

    private AudioSource _audio;

    /// <summary>
    /// Reset() is called when the component is first added or reset in the Inspector.
    /// It attempts to automatically find and assign references for colliders and the AudioSource.
    /// </summary>
    void Reset()
    {
        // Try to find required components automatically
        if (!sourceCollider) sourceCollider = GetComponentInParent<BoxCollider>();
        if (!_audio) TryGetComponent(out _audio);
        if (!handCollider && !string.IsNullOrEmpty(targetTag))
        {
            GameObject taggedObj = GameObject.FindWithTag(targetTag);
            if (taggedObj) handCollider = taggedObj.GetComponent<BoxCollider>();
        }
    }

    /// <summary>
    /// Start() runs once before the first frame update.
    /// It ensures that all required references are set and logs warnings if not.
    /// Useful for initialization when scene hierarchy may not yet be ready in Reset().
    /// </summary>
    void Start()
    {
        // Ensure colliders and audio source are properly assigned
        if (!sourceCollider)
        {
            var inParent = GetComponentInParent<BoxCollider>();
            if (inParent) sourceCollider = inParent;
        }
        if (!handCollider && !string.IsNullOrEmpty(targetTag))
        {
            GameObject taggedObj = GameObject.FindWithTag(targetTag);
            if (taggedObj) handCollider = taggedObj.GetComponent<BoxCollider>();
        }
        if (!_audio) TryGetComponent(out _audio);

        // Log warnings for missing components
        if (!sourceCollider)
            Debug.LogWarning($"{name}: SurfaceAttachAudio missing SourceCollider. Drag the cube's BoxCollider in Inspector.");
        if (!_audio && useDistanceVolume)
            Debug.LogWarning($"{name}: SurfaceAttachAudio found no AudioSource on the same GameObject");
    }

    /// <summary>
    /// LateUpdate() is called once per frame, after all Update() calls.
    /// It calculates the closest point between the hand and the source collider,
    /// updates the object's position accordingly, and dynamically adjusts the
    /// audio volume based on the distance (if enabled).
    /// </summary>
    void LateUpdate()
    {
        if (!sourceCollider || !handCollider) return;

        // Find closest point between hand and source collider
        Vector3 handCenter = handCollider.bounds.center;
        Vector3 closest = sourceCollider.ClosestPoint(handCenter);
        transform.position = closest;

        // Measure current distance
        CurrentDistance = Vector3.Distance(handCenter, closest);

        // Adjust audio volume based on distance, if enabled
        if (useDistanceVolume && _audio)
        {
            float targetVolume;

            // Handle edge case where min and max distances are nearly identical
            if (maxDistance <= minDistance + 1e-4f)
            {
                targetVolume = volumeAtMinDistance;
            }
            else
            {
                // Map distance to a 0–1 range and interpolate volume accordingly
                float t = Mathf.InverseLerp(minDistance, maxDistance, CurrentDistance);
                targetVolume = Mathf.Lerp(volumeAtMinDistance, volumeAtMaxDistance, t);
            }

            // Apply optional volume smoothing
            if (volumeLerpSpeed <= 0f)
            {
                _audio.volume = targetVolume;
            }
            else
            {
                float k = 1f - Mathf.Exp(-volumeLerpSpeed * Time.deltaTime);
                _audio.volume = Mathf.Lerp(_audio.volume, targetVolume, k);
            }
        }
    }
}
