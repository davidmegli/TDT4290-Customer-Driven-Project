
using UnityEngine;
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
    [Tooltip("How smooth the volume adjusts: 0 = no smoothing, 0 < = more smoothing")]
    public float volumeLerpSpeed = 10f;

    public float CurrentDistance { get; private set; }

    private AudioSource _audio;
    void Reset()
    {
        // if (!handCollider)
        if (!sourceCollider) sourceCollider = GetComponentInParent<BoxCollider>();
        if (!_audio) TryGetComponent(out _audio);
        if (!handCollider && !string.IsNullOrEmpty(targetTag))
        {
            GameObject taggedObj = GameObject.FindWithTag(targetTag);
            if (taggedObj) handCollider = taggedObj.GetComponent<BoxCollider>();
        }
    }
    void Start()
    {
        // Try again in Start in case the hierarchy was set up afterwards
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
        if (!sourceCollider)
            Debug.LogWarning($"{name}: SurfaceAttachAudio missing SourceCollider. Drag the cube's BoxCollider in Inspector.");
        if (!_audio && useDistanceVolume)
            Debug.LogWarning($"{name}: SurfaceAttachAudio found no AudioSource on the same GameObject");
    }
    void LateUpdate()
    {
        if (!sourceCollider || !handCollider) return;
        Vector3 handCenter = handCollider.bounds.center;
        Vector3 closest = sourceCollider.ClosestPoint(handCenter);
        transform.position = closest;
        CurrentDistance = Vector3.Distance(handCenter, closest);

        if (useDistanceVolume && _audio)
        {
            float targetVolume;
            if (maxDistance <= minDistance + 1e-4f)
            {
                targetVolume = volumeAtMinDistance;
            }
            else
            {
                float t = Mathf.InverseLerp(minDistance, maxDistance, CurrentDistance);
                targetVolume = Mathf.Lerp(volumeAtMinDistance, volumeAtMaxDistance, t);
            }
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