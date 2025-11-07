using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public class WallAudioLogic : MonoBehaviour
{

    public bool first = false;
    [SerializeField] private SpawnFirstButton spawnFirstButton;
    public void FirstTrigger()
    {
        first = true;
    }


    public void StartRoutine()
    {
        StartCoroutine(CloseToFirstWall());
    }
    public IEnumerator CloseToFirstWall()
    {
        if (!first)
        {
            GameEvents.Fire(VoiceLineAction.NearWall);
            if (!first) yield return new WaitForSeconds(2f);
            FirstTrigger();
            if (first) yield break;
        }

        if (first)
        {
            GameEvents.Fire(VoiceLineAction.NearSecondWall);
            spawnFirstButton.ActivateHitbox();
        }
    }
}
