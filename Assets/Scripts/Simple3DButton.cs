using UnityEngine;
// Script to create a simple 3D button that changes color when clicked or collided with.
public class Simple3DButton : MonoBehaviour
{
    public Color defaultColor = Color.green;
    public Color pressedColor = Color.red;
    public Color collisionColor = Color.blue;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = defaultColor;
    }

    void OnMouseDown()  // Click with mouse
    {
        rend.material.color = pressedColor;
        Debug.Log("Button Pressed (Mouse)!");
    }

    void OnMouseUp()
    {
        rend.material.color = defaultColor;
        Debug.Log("Button Released (Mouse)!");
    }

    void OnCollisionEnter(Collision collision)  // Triggered by physics collision
    {
        rend.material.color = collisionColor;
        Debug.Log("Button Collided with: " + collision.gameObject.name);
    }

    void OnCollisionExit(Collision collision)
    {
        rend.material.color = defaultColor;
        Debug.Log("Collision Ended with: " + collision.gameObject.name);
    }
    void OnTriggerEnter(Collider other)
    {
        rend.material.color = collisionColor;
        Debug.Log("Button Triggered by: " + other.gameObject.name);
    }

    void OnTriggerExit(Collider other)
    {
        rend.material.color = defaultColor;
        Debug.Log("Trigger Ended with: " + other.gameObject.name);
    }

}
