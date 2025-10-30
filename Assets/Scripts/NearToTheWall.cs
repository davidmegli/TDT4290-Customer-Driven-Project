using UnityEngine;
using System.Collections;

public class NearToTheWall : MonoBehaviour
{

    [SerializeField] private WallAudioLogic wallLogic;
    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void activateHitbox()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        wallLogic.StartRoutine();
        gameObject.SetActive(false);
    }
    
}
