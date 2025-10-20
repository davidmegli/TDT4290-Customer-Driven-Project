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

    private void OnCollisionEnter(Collision collision)
    {
        rend.material.color = pressedColor;
        Debug.Log("Button touched by: " + collision.gameObject.name);
        if (triggersLevelCompletion)
        {
            LevelManager.LevelCompleted();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        rend.material.color = defaultColor;
        Debug.Log("Button released by: " + collision.gameObject.name);
    }
}
