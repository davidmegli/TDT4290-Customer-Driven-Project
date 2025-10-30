using UnityEngine;
using System.Collections;
public class SpawningFirstButtonLogic : MonoBehaviour
{
    [SerializeField] private VoiceAudioRouter voiceRouter;
    [SerializeField] private GameObject button;

    public void LoadFirstButton()
    {
        StartCoroutine(LoadButton());
    }

        public IEnumerator LoadButton()
    {

        while (voiceRouter.IsAnyVoicePlaying())
        {
            yield return new WaitForSeconds(1.0f);
        }
        button.SetActive(true);
    }
}

