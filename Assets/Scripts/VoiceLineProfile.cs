using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voice Lines/Voice Line Profile", fileName = "VoiceLineProfile")]
public class VoiceLineProfile : ScriptableObject
{
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
