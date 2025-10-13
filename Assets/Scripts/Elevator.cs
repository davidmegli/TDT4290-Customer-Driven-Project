using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour
{
    [SerializeField] public float Distance;
    
    private bool PlayerIsInside;
    private Coroutine levelSequenceCoroutine;

    void Update()
    {
        var player = Camera.main.transform; // får transform til hovedkameraet

        // Sjekker om spilleren er innenfor en viss avstand fra heisen
        if (Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(transform.position.x, transform.position.z)) < Distance)
        {
            if (!PlayerIsInside) levelSequenceCoroutine = StartCoroutine(LoadNextLevelSequence());
            PlayerIsInside = true;
        }
        else
        {
            PlayerIsInside = false;
            if (levelSequenceCoroutine != null)
            {
                StopCoroutine(levelSequenceCoroutine);
                levelSequenceCoroutine = null;
            }
        }
    }
    
    private IEnumerator LoadNextLevelSequence()
    {
        yield return new WaitForSeconds(3f);
        LevelManager.StartNextLevel();
        gameObject.SetActive(false);
    }
}
