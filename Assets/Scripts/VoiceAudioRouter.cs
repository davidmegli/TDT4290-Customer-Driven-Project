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
    [Tooltip("List all audio sources to be used for voice lines")]
    [SerializeField] private AudioSource[] voiceChannels;

    /// <summary>
    /// Play a clip on one or more channels.
    /// </summary>
    public void Play(AudioClip clip, AudioChannel channels, bool sync = true, double leadSeconds = 0.05)
    {
        if (clip == null || channels == AudioChannel.None) return;

        if (sync)
        {
            // sample-precise sync of multiple sources
            double t = AudioSettings.dspTime + leadSeconds;
            if (channels.HasFlag(AudioChannel.A)) { PrepareAndSchedule(sourceA, clip, t); }
            if (channels.HasFlag(AudioChannel.B)) { PrepareAndSchedule(sourceB, clip, t); }
            if (channels.HasFlag(AudioChannel.C)) { PrepareAndSchedule(sourceC, clip, t); }
            if (channels.HasFlag(AudioChannel.D)) { PrepareAndSchedule(sourceD, clip, t); }
        }
        else
        {
            // fallback PlayOneShot (don't sync)
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
    /// Ensures that no voice clips can overlap.
    /// Stops EVERYTHING playing through the router.
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

    public bool IsAnyVoicePlaying()
    {
        if (voiceChannels == null) return false;
        foreach (var src in voiceChannels)
        {
            if (src && src.isPlaying)
            {
                return true;
            }
        }
        return false;
    }

}


