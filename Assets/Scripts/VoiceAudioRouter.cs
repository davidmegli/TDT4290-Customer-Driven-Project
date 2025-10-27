using UnityEngine;
using System;


public class VoiceAudioRouter : MonoBehaviour
{
    [Header("Scene-wide Audio Sources (MainScene)")]
    [SerializeField] public AudioSource sourceA;
    [SerializeField] public AudioSource sourceB;
    [SerializeField] public AudioSource sourceC;
    [SerializeField] public AudioSource sourceD;

    [Header("All voice channels (for StopAllVoices)")]
    [Tooltip("List opp ALLE audio sources som skal brukes til voice lines")]
    [SerializeField] private AudioSource[] voiceChannels;

    /// <summary>
    /// Spill av et klipp på en eller flere kanaler.
    /// </summary>
    public void Play(AudioClip clip, AudioChannel channels, bool sync = true, double leadSeconds = 0.05)
    {
        if (clip == null || channels == AudioChannel.None) return;

        if (sync)
        {
            // sample-presis synk av flere kilder
            double t = AudioSettings.dspTime + leadSeconds;
            if (channels.HasFlag(AudioChannel.A)) { PrepareAndSchedule(sourceA, clip, t); }
            if (channels.HasFlag(AudioChannel.B)) { PrepareAndSchedule(sourceB, clip, t); }
            if (channels.HasFlag(AudioChannel.C)) { PrepareAndSchedule(sourceC, clip, t); }
            if (channels.HasFlag(AudioChannel.D)) { PrepareAndSchedule(sourceD, clip, t); }
        }
        else
        {
            // fallback PlayOneShot (ikke synk)
            if (channels.HasFlag(AudioChannel.A) && sourceA) sourceA.PlayOneShot(clip);
            if (channels.HasFlag(AudioChannel.B) && sourceB) sourceB.PlayOneShot(clip);
            if (channels.HasFlag(AudioChannel.C) && sourceC) sourceC.PlayOneShot(clip);
            if (channels.HasFlag(AudioChannel.D) && sourceD) sourceD.PlayOneShot(clip);
        }
    }

    private void PrepareAndSchedule(AudioSource src, AudioClip clip, double dspTime)
    {
        if (!src) return;
        src.Stop();
        src.clip = clip;
        src.PlayScheduled(dspTime);
    }

    /// <summary>
    /// Sørger for at ingen voice-klipp kan overlappe.
    /// Stopper ALT som spiller via routeren.
    /// </summary>
    public void StopAllVoices()
    {
        if (voiceChannels == null) return;

        foreach (var src in voiceChannels)
        {
            if (src && src.isPlaying)
                src.Stop();
        }
    }
}
