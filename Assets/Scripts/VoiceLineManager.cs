using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the playback of voice lines and sound effects in response to game events.
/// Handles sequencing and non-sequenced actions for elevator and button interactions.
/// </summary>
public class VoiceLineManager : MonoBehaviour
{
    [SerializeField] private VoiceLineAction[] actionSequence;
    [SerializeField] private VoiceLineAction[] nonSequenceActions;

    private int index = 0;
    public AudioSource elevatorAudioSource;
    private AudioSource buttonAudioSource;

    private AudioClip[] elevatorLevelClips;
    private AudioClip[] buttonLevelClips;

    /// <summary>
    /// Registers the Play method to respond to PlayVoiceLine game events.
    /// </summary>
    void Start()
    {
        GameEvents.PlayVoiceLine += Play;
    }

    /// <summary>
    /// Handles incoming voice line actions, determines if they are part of the sequence or non-sequence,
    /// and triggers the appropriate sound playback.
    /// </summary>
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

    /// <summary>
    /// Plays the corresponding audio clip for the given action, selecting the correct clip based on the current level.
    /// </summary>
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