using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

/// <summary>
/// Handles audio logic and event triggering related to wall interactions in the scene.
/// Manages state transitions and coordinates voice line events and hitbox activation.
/// </summary>
public class WallAudioLogic : MonoBehaviour
{

    public bool first = false;
    [SerializeField] private SpawnFirstButton spawnFirstButton;
    /// <summary>
    /// Sets the 'first' flag to true, indicating the first wall interaction has occurred.
    /// </summary>
    public void FirstTrigger()
    {
        first = true;
    }


    /// <summary>
    /// Starts the coroutine that manages logic for approaching the first wall.
    /// </summary>
    public void StartRoutine()
    {
        StartCoroutine(CloseToFirstWall());
    }
    /// <summary>
    /// Coroutine that handles the sequence of events when the player approaches the first wall.
    /// Fires appropriate voice line events and activates the hitbox for the next interaction.
    /// </summary>
    public IEnumerator CloseToFirstWall()
    {
        if (!first)
        {
            GameEvents.Fire(VoiceLineAction.NearWall);
            if (!first) yield return new WaitForSeconds(2f);
            FirstTrigger();
            if (first) yield break;
        }

        if (first)
        {
            GameEvents.Fire(VoiceLineAction.NearSecondWall);
            spawnFirstButton.ActivateHitbox();
        }
    }
}
