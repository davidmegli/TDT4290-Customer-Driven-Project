using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the activation and interaction logic for the first button in the scene.
/// Handles enabling the button's hitbox and firing events when the player enters its trigger area.
/// </summary>
public class SpawnFirstButton : MonoBehaviour
{
    /// <summary>
    /// Disables the button's GameObject at the start to prevent interaction until explicitly activated.
    /// </summary>
    private void Start()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Activates the button's hitbox, making it interactable in the scene.
    /// </summary>
    public void ActivateHitbox()
    {
        gameObject.SetActive(true);
    }
    /// <summary>
    /// Handles trigger entry events, firing the appropriate game event and initiating the button loading logic.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        SpawningFirstButtonLogic.LoadFirstButton();
    }
}
