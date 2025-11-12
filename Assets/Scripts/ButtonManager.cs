using System.Globalization;
using System.Numerics;
using UnityEngine;

/// <summary>
/// ButtonsManager is a MonoBehaviour that manages a collection of XR buttons and tracks their pressed states.
/// It monitors when all buttons except one have been pressed, then enables level completion for the last unpressed button.
/// </summary>
public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private SimpleXRButton[] buttons;
    private int numberOfButtons = 0;
    private int numberOfButtonsPressed = 0;
    
    /// <summary>
    /// Initializes the button manager by registering the OnButtonPressed callback for all buttons
    /// and setting them to not trigger level completion initially.
    /// </summary>
    void Start()
    {
        foreach(SimpleXRButton btn in buttons)
        {
            btn.onButtonPressed += HandleButtonPressed;
            btn.setTriggersLevelCompletion(false);
            numberOfButtons++;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    /// <summary>
    /// Handles button press events and tracks the number of buttons pressed.
    /// When all buttons except one have been pressed, enables level completion on the remaining unpressed button.
    /// </summary>
    private void HandleButtonPressed()
    {
        numberOfButtonsPressed++;
        if(numberOfButtonsPressed == numberOfButtons - 1)
        {
            foreach(SimpleXRButton btn in buttons)
            {
                if(!btn.isPressed())
                {
                    btn.setTriggersLevelCompletion(true);
                    break;
                }
            }
        }
    }
}
