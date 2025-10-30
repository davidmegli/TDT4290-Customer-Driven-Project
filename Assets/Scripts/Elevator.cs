using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour
{
    [SerializeField] private VoiceAudioRouter voiceRouter;

    [SerializeField] public float Distance; // ikke brukt lenger, beholdt for minimal diff

    // --- AUDIO SOURCES (nye) ---
    [Header("Audio Sources")]
    [SerializeField] private AudioSource insideSource;  // inne i heisen
    [SerializeField] private AudioSource outsideSource; // utenfor heisen
    [SerializeField] private AudioSource doorSource;    // på dør-GameObjectet

    // --- CLIPS ---
    [Header("Clips")]
    [SerializeField] private AudioClip plingClip;          // pling (spilles inne + ute)
    [SerializeField] private AudioClip doorOpenClip;       // dør lyd
    [SerializeField] private AudioClip doorCloseClip;      // dør lyd
    [SerializeField] private AudioClip elevatorMoveClip;   // “bevegelses”-lyd (loopes under tur)
    [SerializeField] private AudioClip elevatorMusicClip;  // heismusikk (loopes kun inne)

    [Header("Volumes (0..1)")]
    [Range(0f,1f)] [SerializeField] private float plingVolume = 1f;
    [Range(0f,1f)] [SerializeField] private float doorOpenVolume = 1f;
    [Range(0f,1f)] [SerializeField] private float doorCloseVolume = 1f;
    [Range(0f,1f)] [SerializeField] private float elevatorMoveVolume = 1f;   // loop (inne)
    [Range(0f,1f)] [SerializeField] private float elevatorMusicVolume = 1f;  // loop (inne)

    // (valgfritt) master per kilde:
    [Header("Master per kilde (0..1)")]
    [Range(0f,1f)] [SerializeField] private float insideMaster = 1f;
    [Range(0f,1f)] [SerializeField] private float outsideMaster = 1f;
    [Range(0f,1f)] [SerializeField] private float doorMaster = 1f;

    [Header("Door")]
    [SerializeField] private GameObject door;
    [SerializeField] private float doorSpeed = 3f;
    private bool isDoorOpening = false;

    // Aktivering / trigger
    private bool isActive = false;
    [Header("Trigger/Dwell")]
    [SerializeField] private float requiredStaySeconds = 2f;
    private bool playerIsInside = false;
    private Coroutine dwellCoroutine;
    private Coroutine levelSequenceCoroutine;

    [Header("Ride duration")]
    [Tooltip("Standard lengde på heistur om ikke spesifisert utenfra")]
    [SerializeField] private float defaultRideDuration = 5f;
    private float currentRideDuration = 0f;

    [Header("Arming")]
    [SerializeField] private bool requireExitBeforeEnter = true;
    private bool armed = true;

    // --- NYTT: vent-på-exit-state ---
    private bool waitingForExitToClose = false;
    private bool gotExitSignal = false;

    private void Awake()
    {
        // Sikre at kilder finnes; ikke opprett nye hvis du vil plassere dem manuelt i prefab
        if (insideSource == null)
        {
            // fallback: prøv å finne en på samme objekt
            insideSource = GetComponent<AudioSource>();
            if (insideSource == null) insideSource = gameObject.AddComponent<AudioSource>();
        }

        // Grunnoppsett for 3D-lyd på alle kilder (juster i Inspector etter behov)
        Setup3DAudio(insideSource);
        if (outsideSource != null) Setup3DAudio(outsideSource);
        if (doorSource != null) Setup3DAudio(doorSource);

        // Startverdi for tur-lengde
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

        // Om du vil kreve exit før første enter
        armed = !requireExitBeforeEnter;

        // NYTT: lytt på global voice-line actions (for ExitElevator)
        GameEvents.PlayVoiceLine += OnVoiceLineAction;

        waitingForExitToClose = false;
        gotExitSignal = false;

        StartCoroutine(ElevatorActivationSequence());
    }

    private void OnDisable()
    {
        // NYTT: rydd abonnement og state
        GameEvents.PlayVoiceLine -= OnVoiceLineAction;
        waitingForExitToClose = false;
        gotExitSignal = false;
    }

    private IEnumerator ElevatorActivationSequence()
    {
        if (!isActive) yield break;

        PlayPling(); // pling både inne og ute
        if (plingClip) yield return new WaitForSeconds(plingClip.length);

        PlayElevatorMusic(); // musikk kun inne (loop)
        PlayDoorOpen(); // dør-lyd på dørkilden
        if (doorOpenClip) yield return new WaitForSeconds(doorOpenClip.length);
    }

    // --- TRIGGER-LOGIKK ---
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Ikke start dwell før vi er "armet" (har sett en exit etter aktivering, hvis krevd)
        if (!armed) return;

        playerIsInside = true;
        if (dwellCoroutine == null)
        {
            dwellCoroutine = StartCoroutine(DwellThenStart());
        }

        GameEvents.Fire(VoiceLineAction.EnteredElevator);
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

        // Start tur med standard varighet hvis ingen annen er spesifisert
        StartElevatorRide(defaultRideDuration);
    }

    /// <summary>
    /// Kall denne for å starte tur med ønsket varighet (sekunder).
    /// Kan kalles utenfra (f.eks. fra LevelVoiceController når sekvens fullføres).
    /// </summary>
    public void StartElevatorRide(float durationSeconds)
    {
        currentRideDuration = Mathf.Max(0f, durationSeconds);

        if (levelSequenceCoroutine == null)
            levelSequenceCoroutine = StartCoroutine(LoadNextLevelSequence());
    }

    private IEnumerator LoadNextLevelSequence()
    {
        // Lukk dør
        PlayDoorClose();
        if (doorCloseClip) yield return new WaitForSeconds(doorCloseClip.length);
        else yield return new WaitForSeconds(1.0f);

        // Spill bevegelseslyd (loop) i valgt varighet
        PlayElevatorMove(); // loop on

        while (voiceRouter.IsAnyVoicePlaying())
        {
            yield return new WaitForSeconds(1.0f);
        }
        
        
        if (currentRideDuration > 0f)
            yield return new WaitForSeconds(currentRideDuration);

        StopElevatorMove(); // loop off

        // Bytt level
        LevelManager.StartNextLevel();

        // Ankomst-sekvens
        PlayPling();
        if (plingClip) yield return new WaitForSeconds(Mathf.Min(1.0f, plingClip.length));
        else yield return new WaitForSeconds(1.0f);

        PlayDoorOpen();
        if (doorOpenClip) yield return new WaitForSeconds(doorOpenClip.length);
        else yield return new WaitForSeconds(1.0f);
        GameEvents.Fire(VoiceLineAction.DoorOpen);

        // --- NYTT: hold døra åpen til ExitZone fyrer ExitElevator ---
        waitingForExitToClose = true;
        gotExitSignal = false;

        // Vent til vi får signalet (fra ElevatorExitZone via GameEvents.Fire(ExitElevator))
        yield return new WaitUntil(() => gotExitSignal);

        // Nå kan vi lukke døra
        PlayDoorClose();
        if (doorCloseClip) yield return new WaitForSeconds(doorCloseClip.length);

        StopElevatorMusic();

        gameObject.SetActive(false);
        isActive = false;
        levelSequenceCoroutine = null;

        // rydd state
        waitingForExitToClose = false;
        gotExitSignal = false;
    }

    // --- AUDIO HELPERS ---

    private void PlayPling()
    {
        if (!plingClip) return;
        if (insideSource)  insideSource.PlayOneShot(plingClip,  plingVolume * insideMaster);
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

    // Looper bevegelseslyd under tur (kun inne)
    private void PlayElevatorMove()
    {
        if (!insideSource || !elevatorMoveClip) return;

        insideSource.Stop();
        insideSource.clip = elevatorMoveClip;
        insideSource.loop = true;
        insideSource.volume = elevatorMoveVolume * insideMaster; // ← viktig
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

    // Musikk kun inne
    private void PlayElevatorMusic()
    {
        if (!insideSource || !elevatorMusicClip) return;

        insideSource.clip = elevatorMusicClip;
        insideSource.loop = true;
        insideSource.volume = elevatorMusicVolume * insideMaster; // ← viktig
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
            // Hvis noe annet spiller (f.eks. moveClip), stopp ikke musikken blindt
        }
    }

    // --- NYTT: event-handler for ExitElevator (fra ExitZone) ---
    private void OnVoiceLineAction(VoiceLineAction action)
    {
        Debug.Log(action);
        if (!waitingForExitToClose) return;                 // bare relevant etter ankomst
        if (action != VoiceLineAction.ExitElevator) return; // kun denne actionen
        gotExitSignal = true;                                // signal til coroutine (FIXED)
    }
}
