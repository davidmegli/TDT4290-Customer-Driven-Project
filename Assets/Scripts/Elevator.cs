using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour
{
    [SerializeField] public float Distance; // ikke brukt lenger, beholdt for minimal diff

    // audio
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip plingClip;
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip doorCloseClip;
    [SerializeField] private AudioClip elevatorMoveClip;
    [SerializeField] private AudioClip elevatorMusicClip;

    private bool isActive = false;

    // Trigger/dwell
    [SerializeField] private float requiredStaySeconds = 2f;
    private bool playerIsInside = false;
    private Coroutine dwellCoroutine;
    private Coroutine levelSequenceCoroutine;

    // For å hindre auto-start hvis noe allerede overlapper ved aktivering
    [SerializeField] private bool requireExitBeforeEnter = true;
    private bool armed = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f;
        audioSource.reverbZoneMix = 0f;
    }

    private void OnEnable()
    {
        Debug.Log("Elevator enabled.");
        isActive = true;

        // reset trigger state
        playerIsInside = false;
        if (dwellCoroutine != null) { StopCoroutine(dwellCoroutine); dwellCoroutine = null; }
        armed = !requireExitBeforeEnter; // hvis vi ikke krever exit→enter, er den armert ved aktivering

        StartCoroutine(ElevatorActivationSequence());
    }

    private IEnumerator ElevatorActivationSequence()
    {
        if (!isActive) yield break;

        PlayPling();
        if (plingClip) yield return new WaitForSeconds(plingClip.length);

        PlayDoorOpen();
        if (doorOpenClip) yield return new WaitForSeconds(doorOpenClip.length);

        PlayElevatorMusic();
    }

    // --- TRIGGER-LOGIKK ---

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Ikke start dwell før vi er "armet" (har sett en exit etter aktivering, hvis krevd)
        if (!armed) return;

        if (!playerIsInside)
        {
            playerIsInside = true;
            if (dwellCoroutine == null)
                dwellCoroutine = StartCoroutine(DwellThenStart());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isActive) return;

        // Første gyldige exit etter aktivering "armer" heisen
        armed = true;

        playerIsInside = false;

        if (dwellCoroutine != null)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = null;
        }
    }

    private IEnumerator DwellThenStart()
    {
        float t = 0f;
        while (t < requiredStaySeconds)
        {
            if (!isActive || !playerIsInside) yield break; // avbrutt
            t += Time.deltaTime;
            yield return null;
        }

        if (levelSequenceCoroutine == null)
            levelSequenceCoroutine = StartCoroutine(LoadNextLevelSequence());
    }

    private IEnumerator LoadNextLevelSequence()
    {
        PlayDoorClose();
        yield return new WaitForSeconds(1.5f);

        PlayElevatorMove();
        if (elevatorMoveClip) yield return new WaitForSeconds(elevatorMoveClip.length);

        StopElevatorMove();

        LevelManager.StartNextLevel();

        PlayPling();
        yield return new WaitForSeconds(1f);

        PlayDoorOpen();
        yield return new WaitForSeconds(1.5f);

        PlayDoorClose();
        StopElevatorMusic();

        gameObject.SetActive(false);
        isActive = false; // deactivate elevator after use
        levelSequenceCoroutine = null;
    }

    // --- AUDIO HELPERS ---

    private void PlayPling()        { if (plingClip)        audioSource.PlayOneShot(plingClip); }
    private void PlayDoorOpen()     { if (doorOpenClip)     audioSource.PlayOneShot(doorOpenClip); }
    private void PlayDoorClose()    { if (doorCloseClip)    audioSource.PlayOneShot(doorCloseClip); }
    private void PlayElevatorMove() { if (elevatorMoveClip) audioSource.PlayOneShot(elevatorMoveClip); }

    private void StopElevatorMove()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    private void PlayElevatorMusic()
    {
        if (elevatorMusicClip)
        {
            audioSource.clip = elevatorMusicClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopElevatorMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
}
