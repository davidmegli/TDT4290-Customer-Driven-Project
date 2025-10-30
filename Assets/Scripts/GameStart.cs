using UnityEngine;

public class GameStart : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        LevelManager.LevelCompleted();
    }
}
