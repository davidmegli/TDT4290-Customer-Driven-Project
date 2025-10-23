using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleXRButton : MonoBehaviour
{
    public Color defaultColor = Color.green;
    public Color pressedColor = Color.red;
    public bool triggersLevelCompletion = true;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = defaultColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        rend.material.color = pressedColor;
        Debug.Log("Button touched by: " + other.gameObject.name);
        if (triggersLevelCompletion)
        {
            LevelManager.LevelCompleted();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        rend.material.color = defaultColor;
        Debug.Log("Button released by: " + other.gameObject.name);
    }
}
