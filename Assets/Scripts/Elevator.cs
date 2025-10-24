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
    [SerializeField] private GameObject door;

    private bool isDoorOpening = false;
    private float doorSpeed = 3;
    private bool isActive = false;

    // Trigger/dwell
    [SerializeField] private float requiredStaySeconds = 2f;
    private bool playerIsInside = false;
    private Coroutine dwellCoroutine;
    private Coroutine levelSequenceCoroutine;

    // For å hindre auto-start hvis noe allerede overlapper ved aktivering
    [SerializeField] private bool requireExitBeforeEnter = true;
    private bool armed = true;

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

    private void Update()
    {
        var doorPos = door.transform.localPosition;
        door.transform.localPosition = Vector3.Lerp(
            doorPos,
            new Vector3(isDoorOpening ? 1.5f : 0f, 0, 0),
            Time.deltaTime * doorSpeed
        );
    }

    private void OnEnable()
    {
        UnityEngine.Debug.Log("Enable_1");
        isActive = true;
        UnityEngine.Debug.Log("Enable_2");
        // reset trigger state
        playerIsInside = false;
        UnityEngine.Debug.Log("Enable_3");
        if (dwellCoroutine != null)
        {
            UnityEngine.Debug.Log("Enable_4");
            StopCoroutine(dwellCoroutine);
            UnityEngine.Debug.Log("Enable_5");
            dwellCoroutine = null;
            UnityEngine.Debug.Log("Enable_6");
        }

        // armed = !requireExitBeforeEnter; // hvis vi ikke krever exit→enter, er den armert ved aktivering
        UnityEngine.Debug.Log("Enable_7");
        StartCoroutine(ElevatorActivationSequence());
        UnityEngine.Debug.Log("Enable_8");
    }

    private IEnumerator ElevatorActivationSequence()
    {
        UnityEngine.Debug.Log("EAS_1");
        if (!isActive) yield break;
        UnityEngine.Debug.Log("EAS_2");
        PlayPling();
        if (plingClip) yield return new WaitForSeconds(plingClip.length);
        UnityEngine.Debug.Log("EAS_3");
        PlayDoorOpen();
        UnityEngine.Debug.Log("EAS_4");
        if (doorOpenClip) yield return new WaitForSeconds(doorOpenClip.length);
        UnityEngine.Debug.Log("EAS_5");
        PlayElevatorMusic();
        UnityEngine.Debug.Log("EAS_6");
    }

    // --- TRIGGER-LOGIKK ---
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter_1");
        Debug.Log(other.gameObject.name);

        if (!isActive)
        {
            Debug.Log("OnTriggerEnter_2");
            return;
        }
        // Ikke start dwell før vi er "armet" (har sett en exit etter aktivering, hvis krevd)
        if (!armed)
        {
            Debug.Log("OnTriggerEnter_3");
            return;
        }
        playerIsInside = true;
        Debug.Log("OnTriggerEnter_4");
        if (dwellCoroutine == null)
        {
            Debug.Log("OnTriggerEnter_5");
            dwellCoroutine = StartCoroutine(DwellThenStart());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit_1");

        if (!isActive)
        {
            Debug.Log("OnTriggerExit_2");
            return;
        }
        // Første gyldige exit etter aktivering "armer" heisen
        armed = true;
        Debug.Log("OnTriggerExit_3");
        playerIsInside = false;
        Debug.Log("OnTriggerExit_4");
        if (dwellCoroutine != null)
        {
            Debug.Log("OnTriggerExit_5");
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = null;
            Debug.Log("OnTriggerExit_6");
        }
    }

    private IEnumerator DwellThenStart()
    {
        Debug.Log("DwellThenStart_1");

        float t = 0f;
        // while (t < requiredStaySeconds)
        // {
        //     if (!isActive || !playerIsInside) yield break; // avbrutt
        //     t += Time.deltaTime;
        //     yield return null;
        // }
        yield return new WaitForSeconds(requiredStaySeconds);
        Debug.Log("DwellThenStart_2");
        if (levelSequenceCoroutine == null)
        {
            Debug.Log("DwellThenStart_3");
            levelSequenceCoroutine = StartCoroutine(LoadNextLevelSequence());
        }
    }

    private IEnumerator LoadNextLevelSequence()
    {
        Debug.Log("LNLS_1");
        PlayDoorClose();
        yield return new WaitForSeconds(1.5f);
        Debug.Log("LNLS_2");
        PlayElevatorMove();
        if (elevatorMoveClip) yield return new WaitForSeconds(elevatorMoveClip.length);
        Debug.Log("LNLS_3");
        StopElevatorMove();
        Debug.Log("LNLS_4");
        LevelManager.StartNextLevel();
        Debug.Log("LNLS_5");
        PlayPling();
        yield return new WaitForSeconds(1f);
        Debug.Log("LNLS_6");
        PlayDoorOpen();
        yield return new WaitForSeconds(1.5f);
        Debug.Log("LNLS_7");
        PlayDoorClose();
        StopElevatorMusic();
        Debug.Log("LNLS_8");
        gameObject.SetActive(false);
        isActive = false;
        Debug.Log("LNLS_9");
        levelSequenceCoroutine = null;
    }

    // --- AUDIO HELPERS ---
    private void PlayPling()
    {
        if (plingClip) audioSource.PlayOneShot(plingClip);
    }

    private void PlayDoorOpen()
    {
        if (doorOpenClip) audioSource.PlayOneShot(doorOpenClip);
        isDoorOpening = true;
        Debug.Log("PlayDoorOpen");
    }

    private void PlayDoorClose()
    {
        if (doorCloseClip) audioSource.PlayOneShot(doorCloseClip);
        isDoorOpening = false;
        Debug.Log("PlayDoorClose");
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
