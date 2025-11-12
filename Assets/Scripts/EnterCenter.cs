using UnityEngine;

/// <summary>
/// EnterCenter is a MonoBehaviour that detects when the player enters the center area of a room.
/// When triggered, it activates hitboxes for left, right, and center wall zones, fires a voice line event,
/// and destroys the trigger object.
/// </summary>
public class EnterCenter : MonoBehaviour
{
    /// <summary>
    /// Reference to the left wall's near-to-wall hitbox zone.
    /// </summary>
    [SerializeField] private NearToTheWall left;
    
    /// <summary>
    /// Reference to the right wall's near-to-wall hitbox zone.
    /// </summary>
    [SerializeField] private NearToTheWall right;
    
    /// <summary>
    /// Reference to the center area's hitbox zone.
    /// </summary>
    [SerializeField] private NearToTheWall center;


    /// <summary>
    /// Called when a collider enters the trigger zone.
    /// Activates the left, right, and center hitbox zones, fires a room center entry voice line event,
    /// and destroys this trigger object to ensure it only activates once.
    /// </summary>
    /// <param name="other">The collider that entered the trigger zone.</param>
    private void OnTriggerEnter(Collider other)
    {
        left.activateHitbox();
        right.activateHitbox();
        center.activateHitbox();
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        Destroy(gameObject);
    }
}
