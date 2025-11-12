using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the activation and interaction logic for the wall proximity trigger.
/// Coordinates with WallAudioLogic to start related routines when the player approaches the wall.
/// </summary>
public class NearToTheWall : MonoBehaviour
{

    [SerializeField] private WallAudioLogic wallLogic;
    /// <summary>
    /// Disables the hitbox GameObject at the start to prevent interaction until explicitly activated.
    /// </summary>
    private void Start()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Activates the hitbox, making it interactable in the scene.
    /// </summary>
    public void activateHitbox()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Handles trigger entry events, starts the wall audio routine, and disables the hitbox after activation.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        wallLogic.StartRoutine();
        gameObject.SetActive(false);
    }
    
}
