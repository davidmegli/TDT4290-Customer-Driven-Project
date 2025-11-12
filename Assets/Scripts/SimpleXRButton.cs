using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; 

/// <summary>
/// Represents a simple XR button that responds to trigger events, changes color, and can signal level completion.
/// Handles button press state and event firing for interactions in XR environments.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SimpleXRButton : MonoBehaviour
{
    public Color defaultColor = Color.green;
    public Color pressedColor = Color.red;
    public bool triggersLevelCompletion = true;
    private bool pressed = false;
    public event Action onButtonPressed;

    private Renderer rend;

    /// <summary>
    /// Initializes the button's renderer and sets its default color and pressed state.
    /// </summary>
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = defaultColor;
        setPressed(false);
    }

    /// <summary>
    /// Handles trigger entry, sets the button as pressed, changes color, fires events, and optionally completes the level.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        setPressed(true);
        rend.material.color = pressedColor;
        if (triggersLevelCompletion)
        {
            LevelManager.LevelCompleted();
        }
        GameEvents.Fire(VoiceLineAction.PushedButton);
    }

    /// <summary>
    /// Handles trigger exit, resetting the button's color to default.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        rend.material.color = defaultColor;
    }

    /// <summary>
    /// Sets whether pressing the button should trigger level completion.
    /// </summary>
    public void setTriggersLevelCompletion(bool triggers)
    {
        triggersLevelCompletion = triggers;
    }

    /// <summary>
    /// Returns whether the button is currently pressed.
    /// </summary>
    public bool isPressed()
    {
        return pressed;
    }

    /// <summary>
    /// Sets the pressed state of the button and invokes the press event if pressed.
    /// </summary>
    public void setPressed(bool pressed)
    {
        this.pressed = pressed;
        if (pressed == true)
        {
            onButtonPressed?.Invoke();
        }
    }
}
