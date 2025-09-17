using UnityEngine;

public class SimpleXRButton : MonoBehaviour
{
    public Color defaultColor = Color.green;
    public Color pressedColor = Color.red;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = defaultColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Optional: filter by tag so only hands (or player cube) can press
        if (other.CompareTag("PlayerHand") || other.CompareTag("Player"))
        {
            rend.material.color = pressedColor;
            Debug.Log("Button touched by: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand") || other.CompareTag("Player"))
        {
            rend.material.color = defaultColor;
            Debug.Log("Button released by: " + other.gameObject.name);
        }
    }
}
