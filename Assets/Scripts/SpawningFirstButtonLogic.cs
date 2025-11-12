using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the logic for spawning and activating the first button in the scene.
/// Ensures the button appears only after all voice lines have finished and provides singleton access and cancellation support.
/// </summary>
public class SpawningFirstButtonLogic : MonoBehaviour
{
    public static SpawningFirstButtonLogic Instance { get; private set; }

    [SerializeField] private VoiceAudioRouter voiceRouter;
    [SerializeField] private GameObject button;

    private Coroutine routine;

    /// <summary>
    /// Initializes the singleton instance and ensures the button is inactive at the start.
    /// </summary>
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
    /// <summary>
    /// Static method to trigger loading and activation of the first button after all voice lines are finished.
    /// </summary>
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

    /// <summary>
    /// Coroutine that waits for all voice lines to finish before activating the button.
    /// </summary>
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
    /// <summary>
    /// Cancels the button loading coroutine if it is running, allowing interruption of the activation process.
    /// </summary>
    public void CancelLoading()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}
