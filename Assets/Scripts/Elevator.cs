using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour
{
    [SerializeField] public float Distance;

    // audio 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip plingClip;
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip doorCloseClip;
    [SerializeField] private AudioClip elevatorMoveClip;
    [SerializeField] private AudioClip elevatorMusicClip;

    private bool PlayerIsInside;
    private Coroutine levelSequenceCoroutine;

    private bool isActive = false;

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
        // When elevator gets activated by LevelManager after LevelCompleted()
        isActive = true;

        StartCoroutine(ElevatorActivationSequence());
    }

    private IEnumerator ElevatorActivationSequence()
    {
        if (!isActive) yield break;
        PlayPling();
        yield return new WaitForSeconds(plingClip.length);
        PlayDoorOpen();
        yield return new WaitForSeconds(doorOpenClip.length);
        PlayElevatorMusic();
    }

    void Update()
    {
        if (!isActive)
        {
            return; // ensure elevator is active before updating
        }

        var player = Camera.main.transform; // får transform til hovedkameraet

        // Sjekker om spilleren er innenfor en viss avstand fra heisen
        if (Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(transform.position.x, transform.position.z)) < Distance)
        {
            if (!PlayerIsInside) levelSequenceCoroutine = StartCoroutine(LoadNextLevelSequence());
            PlayerIsInside = true;
        }
        else
        {
            PlayerIsInside = false;
            if (levelSequenceCoroutine != null)
            {
                StopCoroutine(levelSequenceCoroutine);
                levelSequenceCoroutine = null;
            }
        }
    }

    private IEnumerator LoadNextLevelSequence()
    {
        PlayDoorClose();
        yield return new WaitForSeconds(1.5f);

        PlayElevatorMove();
        yield return new WaitForSeconds(elevatorMoveClip.length);

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
    }

    private void PlayPling()
    {
        if (plingClip) audioSource.PlayOneShot(plingClip);
    }

    private void PlayDoorOpen()
    {
        if (doorOpenClip) audioSource.PlayOneShot(doorOpenClip);
    }

    private void PlayDoorClose()
    {
        if (doorCloseClip) audioSource.PlayOneShot(doorCloseClip);
    }

    private void PlayElevatorMove()
    {
        if (elevatorMoveClip) audioSource.PlayOneShot(elevatorMoveClip);
    }

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
