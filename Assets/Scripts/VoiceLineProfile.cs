using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voice Lines/Voice Line Profile", fileName = "VoiceLineProfile")]
public class VoiceLineProfile : ScriptableObject
{
    [Serializable]
    public class SequenceStep
    {
        public VoiceLineAction action;                    // Hvilken handling må skje
        public AudioClip clip;                            // Hvilket klipp skal spilles
        public AudioChannel channels = AudioChannel.A;    // Hvilke av A–D skal spille (kan kombineres)
    }

    [Header("Action Sequence")]
    public List<SequenceStep> sequence = new();

    [Header("Non Sequence Actions (e.g., Game Over)")]
    public List<VoiceLineAction> nonSequenceActions = new(); // typisk bare TouchedWall
    public AudioClip nonSequenceClip;                         // game over-klipp
    public AudioChannel nonSequenceChannels = AudioChannel.All;
}
