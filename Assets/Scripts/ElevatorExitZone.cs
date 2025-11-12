// ElevatorExitZone.cs
using UnityEngine;

/// <summary>
/// ElevatorExitZone is a MonoBehaviour that detects when the player exits the elevator.
/// It uses a trigger collider to identify when an object enters the exit zone and fires 
/// a corresponding game event to trigger the elevator exit voice line.
/// </summary>
public class ElevatorExitZone : MonoBehaviour
{
    /// <summary>
    /// Called when a collider enters the trigger zone.
    /// Fires the ExitElevator game event
    /// to trigger voice lines and other exit-related gameplay mechanics.
    /// </summary>
    /// <param name="other">The collider that entered the trigger zone.</param>
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.ExitElevator);
    }
}
