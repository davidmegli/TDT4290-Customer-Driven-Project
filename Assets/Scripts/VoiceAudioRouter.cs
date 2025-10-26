using UnityEngine;
using System;

public class VoiceAudioRouter : MonoBehaviour
{
    [Header("Scene-wide Audio Sources (MainScene)")]
    [SerializeField] public AudioSource sourceA;
    [SerializeField] public AudioSource sourceB;
    [SerializeField] public AudioSource sourceC;
    [SerializeField] public AudioSource sourceD;

    // Kall dette for sample-presis sync på tvers av flere kilder
    public void Play(AudioClip clip, AudioChannel channels, bool sync = true, double leadSeconds = 0.05)
    {
        if (clip == null || channels == AudioChannel.None) return;

        if (sync)
        {
            // Planlegg lik starttid på DSP-klokka (mest stabilt for flerkilde)
            double t = AudioSettings.dspTime + leadSeconds;
            if (channels.HasFlag(AudioChannel.A)) { PrepareAndSchedule(sourceA, clip, t); }
            if (channels.HasFlag(AudioChannel.B)) { PrepareAndSchedule(sourceB, clip, t); }
            if (channels.HasFlag(AudioChannel.C)) { PrepareAndSchedule(sourceC, clip, t); }
            if (channels.HasFlag(AudioChannel.D)) { PrepareAndSchedule(sourceD, clip, t); }
        }
        else
        {
            // Greit for enkel avspilling (ikke sample-presist)
            if (channels.HasFlag(AudioChannel.A)) sourceA.PlayOneShot(clip);
            if (channels.HasFlag(AudioChannel.B)) sourceB.PlayOneShot(clip);
            if (channels.HasFlag(AudioChannel.C)) sourceC.PlayOneShot(clip);
            if (channels.HasFlag(AudioChannel.D)) sourceD.PlayOneShot(clip);
        }
    }

    private void PrepareAndSchedule(AudioSource src, AudioClip clip, double dspTime)
    {
        if (src == null) return;
        src.Stop();
        src.clip = clip;
        src.PlayScheduled(dspTime);
    }
}
