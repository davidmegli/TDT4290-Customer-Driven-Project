using System.Linq;
using UnityEngine;

public class VoiceLineManager : MonoBehaviour
{
    [SerializeField] private VoiceLineAction[] actionSequence;
    [SerializeField] private VoiceLineAction[] nonSequenceActions;

    private int index = 0;
    public AudioSource elevatorAudioSource;
    private AudioSource buttonAudioSource;

    private AudioClip[] elevatorLevelClips;
    private AudioClip[] buttonLevelClips;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameEvents.PlayVoiceLine += Play;
    }

    void Play(VoiceLineAction action)
    {
        if (nonSequenceActions.Contains(action))
        {
            PlaySound(action);
            return;
        }

        if (action != actionSequence[index]) return;

        PlaySound(action);
        index++;
    }

    void PlaySound(VoiceLineAction action)
    {
        switch (action)
        {
            case VoiceLineAction.EnteredElevator:
                elevatorAudioSource.clip = elevatorLevelClips[LevelManager.currentLevelIndex];
                break;
            case VoiceLineAction.PushedButton:
                buttonAudioSource.clip = buttonLevelClips[LevelManager.currentLevelIndex];
                break;
        }
    }
}