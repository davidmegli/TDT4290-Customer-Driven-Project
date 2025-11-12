using UnityEngine;
using System.Collections;

/// <summary>
/// Manages elevator mechanics in the VR environment, including:
/// - Audio playback for different elevator sounds (pling, door open/close, movement, music)
/// - Door animation and movement
/// - Player entry/exit detection and dwell timer
/// - Level progression via elevator ride sequence
/// - Integration with voice line events for contextual audio responses
/// </summary>
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

    /// <summary>
    /// Initializes elevator audio sources and configures 3D audio settings.
    /// Falls back to creating or finding AudioSource components if not assigned in Inspector.
    /// Sets the initial ride duration to the default value.
    /// </summary>
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

    /// <summary>
    /// Configures 3D spatial audio settings for the given AudioSource.
    /// Enables spatial audio with maximum blend for immersive positioning,
    /// and disables Doppler and reverb effects for cleaner sound.
    /// </summary>
    /// <param name="src">The AudioSource to configure with 3D audio settings.</param>
    private void Setup3DAudio(AudioSource src)
    {
        if (!src) return;
        src.playOnAwake = false;
        src.spatialize = true;
        src.spatialBlend = 1f;
        src.dopplerLevel = 0f;
        src.reverbZoneMix = 0f;
    }

    /// <summary>
    /// Updates the door's position each frame using linear interpolation (Lerp).
    /// When opening, moves the door to position 1.5f on the X-axis.
    /// When closing, returns the door to position 0f.
    /// </summary>
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

    /// <summary>
    /// Called when the elevator GameObject is enabled.
    /// Initializes elevator state, subscribes to voice line events,
    /// and starts the activation sequence (pling sound, opening door, background music).
    /// </summary>
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

    /// <summary>
    /// Called when the elevator GameObject is disabled.
    /// Unsubscribes from voice line events and cleans up the exit-waiting state.
    /// </summary>
    private void OnDisable()
    {
        // NEW: clean up subscription and state
        GameEvents.PlayVoiceLine -= OnVoiceLineAction;
        waitingForExitToClose = false;
        gotExitSignal = false;
    }

    /// <summary>
    /// Coroutine that plays the initial elevator arrival sequence.
    /// Plays pling sound (inside and outside), starts looping elevator music (inside only),
    /// and triggers door opening animation with sound.
    /// </summary>
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
    /// <summary>
    /// Called when the player enters the elevator trigger zone.
    /// Marks the player as inside the elevator and starts a dwell timer.
    /// If the player stays for the required duration, the elevator ride begins.
    /// Fires an EnteredElevator voice line event.
    /// </summary>
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

    /// <summary>
    /// Coroutine that waits for the player to remain inside the elevator for a required duration.
    /// After the dwell time is complete, automatically starts the elevator ride.
    /// Can be interrupted if the elevator becomes inactive.
    /// </summary>
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
    /// Initiates the elevator ride with the specified duration.
    /// Can be called externally (e.g., from LevelVoiceController when a sequence completes)
    /// to override the default ride duration.
    /// Starts the level transition sequence including door close, movement sounds, and level loading.
    /// </summary>
    /// <param name="durationSeconds">Duration of the elevator ride in seconds.</param>
    public void StartElevatorRide(float durationSeconds)
    {
        currentRideDuration = Mathf.Max(0f, durationSeconds);

        if (levelSequenceCoroutine == null)
            levelSequenceCoroutine = StartCoroutine(LoadNextLevelSequence());
    }

    /// <summary>
    /// Coroutine that orchestrates the complete elevator ride and level transition.
    /// Sequence:
    /// 1. Closes door with sound
    /// 2. Plays movement/ambience sound during the ride
    /// 3. Waits for any ongoing voice lines to finish
    /// 4. Waits for the specified ride duration
    /// 5. Loads the next level
    /// 6. Opens door with arrival pling
    /// 7. Waits for player to exit before closing door again
    /// 8. Disables the elevator and cleans up state
    /// </summary>
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

    /// <summary>
    /// Plays the pling sound (arrival chime) on both inside and outside audio sources.
    /// Uses the configured pling volume and master volume settings.
    /// </summary>
    private void PlayPling()
    {
        if (!plingClip) return;
        if (insideSource) insideSource.PlayOneShot(plingClip, plingVolume * insideMaster);
        if (outsideSource) outsideSource.PlayOneShot(plingClip, plingVolume * outsideMaster);
    }

    /// <summary>
    /// Plays the door opening sound and initiates the door opening animation.
    /// Sets isDoorOpening flag to true, which is used in Update() for door movement.
    /// </summary>
    private void PlayDoorOpen()
    {
        if (doorSource && doorOpenClip) doorSource.PlayOneShot(doorOpenClip, doorOpenVolume * doorMaster);
        isDoorOpening = true;
    }

    /// <summary>
    /// Plays the door closing sound and initiates the door closing animation.
    /// Sets isDoorOpening flag to false, which causes the door to return to closed position.
    /// </summary>
    private void PlayDoorClose()
    {
        if (doorSource && doorCloseClip) doorSource.PlayOneShot(doorCloseClip, doorCloseVolume * doorMaster);
        isDoorOpening = false;
    }

    /// <summary>
    /// Plays the looping elevator movement/ambience sound on the inside audio source only.
    /// Stops any previous audio clip and sets the movement clip to loop during the ride.
    /// Applies elevator move volume and inside master volume settings.
    /// </summary>
    private void PlayElevatorMove()
    {
        if (!insideSource || !elevatorMoveClip) return;

        insideSource.Stop();
        insideSource.clip = elevatorMoveClip;
        insideSource.loop = true;
        insideSource.volume = elevatorMoveVolume * insideMaster; // ← important
        insideSource.Play();
    }

    /// <summary>
    /// Stops the elevator movement sound loop on the inside audio source.
    /// Safely stops only if the movement clip is currently playing to avoid interfering with other audio.
    /// </summary>
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

    /// <summary>
    /// Plays the looping elevator background music on the inside audio source only.
    /// Music plays continuously while the player is inside the elevator.
    /// Applies elevator music volume and inside master volume settings.
    /// </summary>
    private void PlayElevatorMusic()
    {
        if (!insideSource || !elevatorMusicClip) return;

        insideSource.clip = elevatorMusicClip;
        insideSource.loop = true;
        insideSource.volume = elevatorMusicVolume * insideMaster; // ← important
        insideSource.Play();
    }

    /// <summary>
    /// Stops the elevator background music on the inside audio source.
    /// Safely stops only if the music clip is currently playing to avoid interfering with other audio.
    /// </summary>
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

    /// <summary>
    /// Event handler for voice line actions broadcasted via GameEvents.
    /// Specifically listens for ExitElevator action during the post-arrival waiting phase.
    /// When the player exits the elevator after arrival, this signals the elevator to close the door
    /// and complete its sequence.
    /// </summary>
    /// <param name="action">The voice line action that was fired.</param>
    private void OnVoiceLineAction(VoiceLineAction action)
    {
        if (!waitingForExitToClose) return;                 // only relevant after arrival
        if (action != VoiceLineAction.ExitElevator) return; // only this action
        gotExitSignal = true;                                // signal to coroutine (FIXED)
    }
}
