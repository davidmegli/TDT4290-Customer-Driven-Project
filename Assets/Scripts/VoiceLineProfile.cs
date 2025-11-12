using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines a profile for voice lines, including action sequences and non-sequence actions.
/// Used to configure which audio clips and channels are triggered for specific game events.
/// </summary>
[CreateAssetMenu(menuName = "Voice Lines/Voice Line Profile", fileName = "VoiceLineProfile")]
public class VoiceLineProfile : ScriptableObject
{
    /// <summary>
    /// Represents a single step in a voice line sequence, specifying the action, audio clip, and channels to play.
    /// </summary>
    [Serializable]
    public class SequenceStep
    {
        public VoiceLineAction action;                    // What action must be taken
        public AudioClip clip;                            // Which clip should be played
        public AudioChannel channels = AudioChannel.A;    // Which of A–D should play (can be combined)
    }

    [Header("Action Sequence")]
    public List<SequenceStep> sequence = new();

    [Header("Non Sequence Actions (e.g., Game Over)")]
    public List<VoiceLineAction> nonSequenceActions = new(); // typically just TouchedWall
    public AudioClip nonSequenceClip;                         // game over clip
    public AudioChannel nonSequenceChannels = AudioChannel.All;
}
