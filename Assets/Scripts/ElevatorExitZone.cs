// ElevatorExitZone.cs
using UnityEngine;

public class ElevatorExitZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.Fire(VoiceLineAction.ExitElevator);
    }
}
