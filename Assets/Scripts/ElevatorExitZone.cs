// ElevatorExitZone.cs
using UnityEngine;

public class ElevatorExitZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Elevator Exit Zone entered by: " + other.gameObject.name);
        GameEvents.Fire(VoiceLineAction.ExitElevator);
    }
}
