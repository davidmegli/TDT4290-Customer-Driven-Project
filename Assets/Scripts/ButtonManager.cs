using System.Globalization;
using System.Numerics;
using UnityEngine;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private SimpleXRButton[] buttons;
    private int numberOfButtons = 0;
    private int numberOfButtonsPressed = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
