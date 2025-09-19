using UnityEngine;
public class SurfaceAttachAudio : MonoBehaviour
{
    [Tooltip("BoxCollider til kuben lyden skal 'komme fra'")]
    public BoxCollider sourceCollider;
    [Tooltip("Transform med AudioListener (ofte Main Camera). Tomt = Camera.main")]
    public Transform listener;
    void Reset()
    {
        if (!listener && Camera.main) listener = Camera.main.transform;
        if (!sourceCollider) sourceCollider = GetComponentInParent<BoxCollider>();
    }
    void Start()
    {
        // Prøv igjen i Start i tilfelle hierarkiet ble satt opp etterpå
        if (!listener && Camera.main) listener = Camera.main.transform;
        if (!sourceCollider)
        {
            var inParent = GetComponentInParent<BoxCollider>();
            if (inParent) sourceCollider = inParent;
        }
        if (!sourceCollider)
            Debug.LogWarning($"{name}: SurfaceAttachAudio mangler SourceCollider. Dra inn kubens BoxCollider i Inspector.");
        if (!listener)
            Debug.LogWarning($"{name}: SurfaceAttachAudio mangler Listener. Dra inn kameraet eller sett Camera.main-tag.");
    }
    void LateUpdate()
    {
        if (!sourceCollider || (!listener && !Camera.main)) return;
        Vector3 listenerPos = listener ? listener.position : Camera.main.transform.position;
        Vector3 closest = sourceCollider.ClosestPoint(listenerPos);
        transform.position = closest;
    }
}