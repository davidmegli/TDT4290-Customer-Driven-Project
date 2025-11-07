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
        // Simple (safer) singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optional: make sure the button is off to start
        if (button != null) button.SetActive(false);
    }

    // Static API as delegate to the instance
    public static void LoadFirstButton()
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.routine == null)
        {
            Instance.routine = Instance.StartCoroutine(Instance.LoadButton());
        }
    }

    private IEnumerator LoadButton()
    {
        // Wait until all voice is finished, if we have router
        if (voiceRouter != null)
        {
            while (voiceRouter.IsAnyVoicePlaying())
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        if (button != null)
        {
            button.SetActive(true);
        }
            // Reset so we can start again later if desired
            routine = null;
    }

    // Optional: cancel if necessary
    public void CancelLoading()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}
