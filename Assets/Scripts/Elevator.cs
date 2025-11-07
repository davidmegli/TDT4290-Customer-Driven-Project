using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour
{
    [SerializeField] private VoiceAudioRouter voiceRouter;

    [SerializeField] public float Distance; // no longer used, kept for minimal diff

    // --- AUDIO SOURCES (new) ---
    [Header("Audio Sources")]
    [SerializeField] private AudioSource insideSource;  // inside the elevator
    [SerializeField] private AudioSource outsideSource; // outside the elevator
    [SerializeField] private AudioSource doorSource;    // on door GameObject

    // --- CLIPS ---
    [Header("Clips")]
    [SerializeField] private AudioClip plingClip;          // pling sound (plays inside + outside)
    [SerializeField] private AudioClip doorOpenClip;       // door sound
    [SerializeField] private AudioClip doorCloseClip;      // door sound
    [SerializeField] private AudioClip elevatorMoveClip;   // movement sound (loops during ride)
    [SerializeField] private AudioClip elevatorMusicClip;  // elevator music (loops inside only)

    [Header("Volumes (0..1)")]
    [Range(0f, 1f)][SerializeField] private float plingVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float doorOpenVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float doorCloseVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float elevatorMoveVolume = 1f;   // loop (inside)
    [Range(0f, 1f)][SerializeField] private float elevatorMusicVolume = 1f;  // loop (inside)

    // (optional) master per source:
    [Header("Master per source (0..1)")]
    [Range(0f, 1f)][SerializeField] private float insideMaster = 1f;
    [Range(0f, 1f)][SerializeField] private float outsideMaster = 1f;
    [Range(0f, 1f)][SerializeField] private float doorMaster = 1f;

    [Header("Door")]
    [SerializeField] private GameObject door;
    [SerializeField] private float doorSpeed = 3f;
    private bool isDoorOpening = false;

    // Activation / trigger
    private bool isActive = false;
    [Header("Trigger/Dwell")]
    [SerializeField] private float requiredStaySeconds = 2f;
    private bool playerIsInside = false;
    private Coroutine dwellCoroutine;
    private Coroutine levelSequenceCoroutine;

    [Header("Ride duration")]
    [Tooltip("Standard duration of elevator ride if not specified externally")]
    [SerializeField] private float defaultRideDuration = 5f;
    private float currentRideDuration = 0f;

    [Header("Arming")]
    [SerializeField] private bool requireExitBeforeEnter = true;
    private bool armed = true;

    // --- NEW: wait-for-exit state ---
    private bool waitingForExitToClose = false;
    private bool gotExitSignal = false;

    private void Awake()
    {
        // Ensure sources exist; don't create new ones if you want to place them manually in prefab
        if (insideSource == null)
        {
            // fallback: try to find one on the same object
            insideSource = GetComponent<AudioSource>();
            if (insideSource == null) insideSource = gameObject.AddComponent<AudioSource>();
        }

        // Basic setup for 3D audio on all sources (adjust in Inspector as needed)
        Setup3DAudio(insideSource);
        if (outsideSource != null) Setup3DAudio(outsideSource);
        if (doorSource != null) Setup3DAudio(doorSource);

        // Starting value for ride duration
        currentRideDuration = defaultRideDuration;
    }

    private void Setup3DAudio(AudioSource src)
    {
        if (!src) return;
        src.playOnAwake = false;
        src.spatialize = true;
        src.spatialBlend = 1f;
        src.dopplerLevel = 0f;
        src.reverbZoneMix = 0f;
    }

    private void Update()
    {
        if (!door) return;

        var doorPos = door.transform.localPosition;
        door.transform.localPosition = Vector3.Lerp(
            doorPos,
            new Vector3(isDoorOpening ? 1.5f : 0f, 0, 0),
            Time.deltaTime * doorSpeed
        );
    }

    private void OnEnable()
    {
        isActive = true;

        // reset trigger state
        playerIsInside = false;
        if (dwellCoroutine != null)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = null;
        }

        // If you want to require exit before first enter
        armed = !requireExitBeforeEnter;

        // NEW: listen to global voice-line actions (for ExitElevator)
        GameEvents.PlayVoiceLine += OnVoiceLineAction;

        waitingForExitToClose = false;
        gotExitSignal = false;

        StartCoroutine(ElevatorActivationSequence());
    }

    private void OnDisable()
    {
        // NEW: clean up subscription and state
        GameEvents.PlayVoiceLine -= OnVoiceLineAction;
        waitingForExitToClose = false;
        gotExitSignal = false;
    }

    private IEnumerator ElevatorActivationSequence()
    {
        if (!isActive) yield break;

        PlayPling(); // pling both inside and outside
        if (plingClip) yield return new WaitForSeconds(plingClip.length);

        PlayElevatorMusic(); // music inside only (loop)
        PlayDoorOpen(); // door sound on door source
        if (doorOpenClip) yield return new WaitForSeconds(doorOpenClip.length);
    }

    // --- TRIGGER LOGIC ---
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Don't start dwell until we are "armed" (have seen an exit after activation, if required)
        // if (!armed) return;

        playerIsInside = true;
        if (dwellCoroutine == null)
        {
            dwellCoroutine = StartCoroutine(DwellThenStart());
        }

        GameEvents.Fire(VoiceLineAction.EnteredElevator);
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (!isActive) return;

    //     // First valid exit after activation "arms" the elevator
    //     armed = true;
    //     playerIsInside = false;

    //     if (dwellCoroutine != null)
    //     {
    //         StopCoroutine(dwellCoroutine);
    //         dwellCoroutine = null;
    //     }
    // }

    private IEnumerator DwellThenStart()
    {
        float t = 0f;
        while (t < requiredStaySeconds)
        {
            if (!isActive) yield break; // interrupted
            t += Time.deltaTime;
            yield return null;
        }

        // Start ride with default duration if no other is specified
        StartElevatorRide(defaultRideDuration);
    }

    /// <summary>
    /// Call this to start ride with desired duration (seconds).
    /// Can be called externally (e.g. from LevelVoiceController when sequence completes).
    /// </summary>
    public void StartElevatorRide(float durationSeconds)
    {
        currentRideDuration = Mathf.Max(0f, durationSeconds);

        if (levelSequenceCoroutine == null)
            levelSequenceCoroutine = StartCoroutine(LoadNextLevelSequence());
    }

    private IEnumerator LoadNextLevelSequence()
    {
        // Close door
        PlayDoorClose();
        if (doorCloseClip) yield return new WaitForSeconds(doorCloseClip.length);
        else yield return new WaitForSeconds(1.0f);

        // Play movement sound (loop) for chosen duration
        PlayElevatorMove(); // loop on

        while (voiceRouter.IsAnyVoicePlaying())
        {
            yield return new WaitForSeconds(1.0f);
        }


        if (currentRideDuration > 0f)
            yield return new WaitForSeconds(currentRideDuration);

        StopElevatorMove(); // loop off

        // Switch level
        LevelManager.StartNextLevel();

        // Arrival sequence
        PlayPling();
        if (plingClip) yield return new WaitForSeconds(Mathf.Min(1.0f, plingClip.length));
        else yield return new WaitForSeconds(1.0f);

        PlayDoorOpen();
        if (doorOpenClip) yield return new WaitForSeconds(doorOpenClip.length);
        else yield return new WaitForSeconds(1.0f);
        GameEvents.Fire(VoiceLineAction.DoorOpen);

        // --- NEW: keep door open until ExitZone fires ExitElevator ---
        waitingForExitToClose = true;
        gotExitSignal = false;

        // Wait until we get the signal (from ElevatorExitZone via GameEvents.Fire(ExitElevator))
        yield return new WaitUntil(() => gotExitSignal);

        // Now we can close the door
        PlayDoorClose();
        if (doorCloseClip) yield return new WaitForSeconds(doorCloseClip.length);

        StopElevatorMusic();

        gameObject.SetActive(false);
        isActive = false;
        levelSequenceCoroutine = null;

        // clean up state
        waitingForExitToClose = false;
        gotExitSignal = false;
    }

    // --- AUDIO HELPERS ---

    private void PlayPling()
    {
        if (!plingClip) return;
        if (insideSource) insideSource.PlayOneShot(plingClip, plingVolume * insideMaster);
        if (outsideSource) outsideSource.PlayOneShot(plingClip, plingVolume * outsideMaster);
    }

    private void PlayDoorOpen()
    {
        if (doorSource && doorOpenClip) doorSource.PlayOneShot(doorOpenClip, doorOpenVolume * doorMaster);
        isDoorOpening = true;
    }

    private void PlayDoorClose()
    {
        if (doorSource && doorCloseClip) doorSource.PlayOneShot(doorCloseClip, doorCloseVolume * doorMaster);
        isDoorOpening = false;
    }

    // Loops movement sound during ride (inside only)
    private void PlayElevatorMove()
    {
        if (!insideSource || !elevatorMoveClip) return;

        insideSource.Stop();
        insideSource.clip = elevatorMoveClip;
        insideSource.loop = true;
        insideSource.volume = elevatorMoveVolume * insideMaster; // ← important
        insideSource.Play();
    }

    private void StopElevatorMove()
    {
        if (!insideSource) return;
        if (insideSource.clip == elevatorMoveClip)
        {
            insideSource.Stop();
            insideSource.loop = false;
            insideSource.clip = null;
        }
    }

    // Music inside only
    private void PlayElevatorMusic()
    {
        if (!insideSource || !elevatorMusicClip) return;

        insideSource.clip = elevatorMusicClip;
        insideSource.loop = true;
        insideSource.volume = elevatorMusicVolume * insideMaster; // ← important
        insideSource.Play();
    }

    private void StopElevatorMusic()
    {
        if (!insideSource) return;

        if (insideSource.clip == elevatorMusicClip)
        {
            insideSource.Stop();
            insideSource.loop = false;
            insideSource.clip = null;
        }
        else if (insideSource.isPlaying)
        {
            // If something else is playing (e.g. moveClip), don't stop music blindly
        }
    }

    // --- NEW: event handler for ExitElevator (from ExitZone) ---
    private void OnVoiceLineAction(VoiceLineAction action)
    {
        Debug.Log(action);
        if (!waitingForExitToClose) return;                 // only relevant after arrival
        if (action != VoiceLineAction.ExitElevator) return; // only this action
        gotExitSignal = true;                                // signal to coroutine (FIXED)
    }
}
