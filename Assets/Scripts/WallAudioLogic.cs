using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public class WallAudioLogic : MonoBehaviour
{

    public bool first = false;
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
            Debug.Log("First Wall!!!!");
            GameEvents.Fire(VoiceLineAction.NearWall);
            if (!first) yield return new WaitForSeconds(2f);
            FirstTrigger();
            if (first) yield break;
        }

        if (first)
        {
            Debug.Log("Second wall!!!!!!!");
            GameEvents.Fire(VoiceLineAction.NearSecondWall);
        }
    }
}
