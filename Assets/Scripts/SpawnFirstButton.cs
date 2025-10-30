using UnityEngine;
using System.Collections;

public class SpawnFirstbutton : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        SpawningFirstButtonLogic.LoadFirstButton();
    }
}
