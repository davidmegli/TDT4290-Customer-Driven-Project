using UnityEngine;

public class NearToTheWall : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.NearWall);
        gameObject.SetActive(false);

    }
}
