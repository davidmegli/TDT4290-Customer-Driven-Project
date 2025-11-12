// GameEvents.cs
using System;

/// <summary>
/// Enumeration representing different in-game actions or events 
/// that can trigger a voice line to play.
/// Each enum value corresponds to a specific situation or interaction 
/// the player can perform in the game.
/// </summary>
public enum VoiceLineAction
{
    EnteredElevator,
    PushedButton,
    TouchedWall,
    ExitElevator,
    RoomCenterEnter,
    DoorOpen,
    NearWall,
    NearSecondWall
}

/// <summary>
/// Static class that defines a centralized event system for broadcasting
/// game actions related to voice line playback.
/// Other scripts can subscribe to the <see cref="PlayVoiceLine"/> event 
/// to react when a specific <see cref="VoiceLineAction"/> occurs.
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// Event triggered when a specific <see cref="VoiceLineAction"/> occurs.
    /// Subscribers to this event can handle the action (e.g., play the appropriate voice line).
    /// </summary>
    public static event Action<VoiceLineAction> PlayVoiceLine;

    /// <summary>
    /// Invokes the <see cref="PlayVoiceLine"/> event with the specified action.
    /// This method safely checks for subscribers before invoking the event.
    /// </summary>
    /// <param name="action">The <see cref="VoiceLineAction"/> to broadcast.</param>
    public static void Fire(VoiceLineAction action) => PlayVoiceLine?.Invoke(action);
}
