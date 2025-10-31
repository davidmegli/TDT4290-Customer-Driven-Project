using UnityEngine;
using System.Collections;

public class SpawningFirstButtonLogic : MonoBehaviour
{
    public static SpawningFirstButtonLogic Instance { get; private set; }

    [SerializeField] private VoiceAudioRouter voiceRouter;
    [SerializeField] private GameObject button;

    private Coroutine routine;

    private void Awake()
    {
        // Enkel (tryggere) singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Valgfritt: sørg for at knappen er av til start
        if (button != null) button.SetActive(false);
    }

    // Statisk API som delegere til instansen
    public static void LoadFirstButton()
    {
        if (Instance == null)
        {
            Debug.LogError("[SpawningFirstButtonLogic] Ingen Instance i scenen.");
            return;
        }

        if (Instance.routine == null)
        {
            Instance.routine = Instance.StartCoroutine(Instance.LoadButton());
        }
    }

    private IEnumerator LoadButton()
    {
        // Vent til all voice er ferdig, hvis vi har router
        if (voiceRouter != null)
        {
            while (voiceRouter.IsAnyVoicePlaying())
            {
                Debug.Log("Checking the thing");
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            Debug.LogWarning("[SpawningFirstButtonLogic] voiceRouter er null – viser knapp med en gang.");
        }

        if (button != null)
        {
            Debug.Log("Activating button");
            button.SetActive(true);
        }
            // Nullstill så vi kan starte på nytt senere om ønskelig
            routine = null;
    }

    // Valgfritt: avbryt om nødvendig
    public void CancelLoading()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}
