// GameEvents.cs
using System;

public enum VoiceLineAction
{
    EnteredElevator,
    PushedButton,
    TouchedWall,
    ExitElevator
}

public static class GameEvents
{
    public static event Action<VoiceLineAction> PlayVoiceLine;
    public static void Fire(VoiceLineAction action) => PlayVoiceLine?.Invoke(action);
}
