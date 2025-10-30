using UnityEngine;

public class SpawnFirstbutton : MonoBehaviour
{

    [SerializeField] private SimpleXRButton button;
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        gameObject.SetActive(false);

        button.gameObject.SetActive(true);
    }
}
