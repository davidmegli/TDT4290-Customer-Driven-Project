using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; 

[RequireComponent(typeof(Collider))]
public class SimpleXRButton : MonoBehaviour
{
    public Color defaultColor = Color.green;
    public Color pressedColor = Color.red;
    public bool triggersLevelCompletion = true;
    private bool pressed = false;
    public event Action onButtonPressed;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = defaultColor;
        setPressed(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        setPressed(true);
        rend.material.color = pressedColor;
        Debug.Log("Button touched by: " + other.gameObject.name);
        if (triggersLevelCompletion)
        {
            LevelManager.LevelCompleted();
        }
        GameEvents.Fire(VoiceLineAction.PushedButton);
    }

    private void OnTriggerExit(Collider other)
    {
        rend.material.color = defaultColor;
        Debug.Log("Button released by: " + other.gameObject.name);
    }

    public void setTriggersLevelCompletion(bool triggers)
    {
        triggersLevelCompletion = triggers;
    }

    public bool isPressed()
    {
        return pressed;
    }

    public void setPressed(bool pressed)
    {
        this.pressed = pressed;
        if (pressed == true)
        {
            onButtonPressed?.Invoke();
        }
    }
}
