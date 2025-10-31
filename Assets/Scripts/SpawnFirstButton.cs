using UnityEngine;
using System.Collections;

public class SpawnFirstButton : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ActivateHitbox()
    {
        gameObject.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        SpawningFirstButtonLogic.LoadFirstButton();
    }
}
