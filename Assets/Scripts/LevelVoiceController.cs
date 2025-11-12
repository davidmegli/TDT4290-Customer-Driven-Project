using UnityEngine;
using System;

/// <summary>
/// Provides static events for signaling completion of voice line sequences and non-sequence triggers.
/// Used for communication between voice line systems and other game logic.
/// </summary>
public static class VoiceFlowEvents
{
    public static event Action OnSequenceCompleted;
    public static event Action OnNonSequenceTriggered;

    /// <summary>
    /// Raises the event indicating the voice line sequence has completed.
    /// </summary>
    public static void RaiseSequenceCompleted() => OnSequenceCompleted?.Invoke();
    /// <summary>
    /// Raises the event indicating a non-sequence voice line has been triggered.
    /// </summary>
    public static void RaiseNonSequenceTriggered() => OnNonSequenceTriggered?.Invoke();
}

/// <summary>
/// Controls voice line playback for a level, handling both sequential and non-sequential actions.
/// Manages playback policy, event firing, and progression through the voice line sequence.
/// </summary>
public class LevelVoiceController : MonoBehaviour
{
    [Header("Config (per level)")]
    [SerializeField] private VoiceLineProfile profile;

    [Header("Scene router (from MainScene)")]
    [SerializeField] private VoiceAudioRouter router;

    [Header("Playback policy")]
    [Tooltip("If on: always stops all active voice sources before new clip starts.")]
    [SerializeField] private bool stopPreviousOnNext = true;

    private int index = 0;
    private bool locked = false;

    /// <summary>
    /// Ensures the voice audio router reference is set, searching the scene if necessary.
    /// </summary>
    private void Awake()
    {
        if (!router) router = FindObjectOfType<VoiceAudioRouter>();
    }

    /// <summary>
    /// Subscribes and unsubscribes the OnVoiceLine handler to the PlayVoiceLine event.
    /// </summary>
    private void OnEnable()  => GameEvents.PlayVoiceLine += OnVoiceLine;
    private void OnDisable() => GameEvents.PlayVoiceLine -= OnVoiceLine;

    /// <summary>
    /// Handles incoming voice line actions, triggering playback for non-sequence and sequence steps,
    /// and raising completion events as appropriate.
    /// </summary>
    private void OnVoiceLine(VoiceLineAction action)
    {
        if (locked || profile == null || router == null) return;

        // 1) Non-sequence (can be triggered at any time)
        if (profile.nonSequenceActions != null &&
            profile.nonSequenceActions.Contains(action))
        {
            if (stopPreviousOnNext) router.StopAllVoices();

            if (profile.nonSequenceClip)
                router.Play(profile.nonSequenceClip, profile.nonSequenceChannels, sync: true);

            locked = true;
            VoiceFlowEvents.RaiseNonSequenceTriggered();
            return;
        }

        // 2) Sequential logic
        if (profile.sequence == null || index >= profile.sequence.Count) return;

        var step = profile.sequence[index];
        if (step.action != action) return; // wrong order -> ignore

        if (stopPreviousOnNext) router.StopAllVoices();

        if (step.clip)
            router.Play(step.clip, step.channels, sync: true);

        index++;

        if (index >= profile.sequence.Count)
        {
            locked = true;
            VoiceFlowEvents.RaiseSequenceCompleted();
        }
    }
}
