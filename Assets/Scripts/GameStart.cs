using UnityEngine;

/// <summary>
/// Handles the trigger event that starts or completes the game sequence.
/// This script should be attached to a GameObject with a trigger collider.
/// When another collider (such as the player) enters the trigger, 
/// it fires a voice line event and signals the LevelManager that the level is completed.
/// </summary>
public class GameStart : MonoBehaviour
{
    /// <summary>
    /// Called automatically by Unity when another collider enters this object's trigger collider.
    /// Triggers a voice line event for the room center entry and notifies the LevelManager that the level is complete.
    /// </summary>
    /// <param name="other">The collider of the object that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        LevelManager.LevelCompleted();
    }
}
