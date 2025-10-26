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

    private int index = 0;
    private bool locked = false;

    private void Awake()
    {
        // Fallback i tilfelle du glemmer å dra inn referansen
        if (!router) router = FindObjectOfType<VoiceAudioRouter>();
    }

    private void OnEnable()
    {
        GameEvents.PlayVoiceLine += OnVoiceLine;
    }

    private void OnDisable()
    {
        GameEvents.PlayVoiceLine -= OnVoiceLine;
    }

    private void OnVoiceLine(VoiceLineAction action)
    {
        if (locked || profile == null || router == null) return;

        // 1) Non-sequence (f.eks. game over)
        if (profile.nonSequenceActions != null &&
            profile.nonSequenceActions.Contains(action))
        {
            if (profile.nonSequenceClip)
                router.Play(profile.nonSequenceClip, profile.nonSequenceChannels, sync: true);
            else
                Debug.LogWarning("[LevelVoiceController] Non-sequence triggered, but clip is missing.", this);

            locked = true;
            VoiceFlowEvents.RaiseNonSequenceTriggered();
            return;
        }

        // 2) Sekvenslogikk
        if (profile.sequence == null || index >= profile.sequence.Count) return;

        var step = profile.sequence[index];

        if (step.action != action) return; // feil rekkefølge -> ignorer

        if (step.clip)
            router.Play(step.clip, step.channels, sync: true);
        else
            Debug.LogWarning($"[LevelVoiceController] Sequence step {index} has no clip.", this);

        index++;

        if (index >= profile.sequence.Count)
        {
            locked = true;
            VoiceFlowEvents.RaiseSequenceCompleted();
        }
    }
}
