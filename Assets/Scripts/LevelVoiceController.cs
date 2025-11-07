using UnityEngine;
using System;

public static class VoiceFlowEvents
{
    public static event Action OnSequenceCompleted;
    public static event Action OnNonSequenceTriggered;

    public static void RaiseSequenceCompleted() => OnSequenceCompleted?.Invoke();
    public static void RaiseNonSequenceTriggered() => OnNonSequenceTriggered?.Invoke();
}

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

    private void Awake()
    {
        if (!router) router = FindObjectOfType<VoiceAudioRouter>();
    }

    private void OnEnable()  => GameEvents.PlayVoiceLine += OnVoiceLine;
    private void OnDisable() => GameEvents.PlayVoiceLine -= OnVoiceLine;

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
