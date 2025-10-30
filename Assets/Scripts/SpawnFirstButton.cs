using UnityEngine;
using System.Collections;

public class SpawnFirstbutton : MonoBehaviour
{

    [SerializeField] private SpawningFirstButtonLogic spawnButton;
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        spawnButton.LoadFirstButton();
    }
}
