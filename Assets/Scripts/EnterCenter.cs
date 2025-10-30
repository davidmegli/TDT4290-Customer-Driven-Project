using UnityEngine;

public class EnterCenter : MonoBehaviour
{
    [SerializeField] private NearToTheWall left;
    [SerializeField] private NearToTheWall right;
    [SerializeField] private NearToTheWall center;


    private void OnTriggerEnter(Collider other)
    {
        left.activateHitbox();
        right.activateHitbox();
        center.activateHitbox();
        GameEvents.Fire(VoiceLineAction.RoomCenterEnter);
        Destroy(gameObject);
    }
}
