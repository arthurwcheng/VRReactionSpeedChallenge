using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GameButton : MonoBehaviour
{

    // This script controls the behaviour of the Reaction Machine buttons. It should be attached as a component on each Button gameobject of the Reaction Machine prefab

    public bool buttonOn = false;                   // This sets the button state to be 'off' at the start
    public ReactionGameManager gameManager;         // The gameobject containing the ReactionGameManager script should be attached here
    private AudioSource buttonAudio;            
    public SteamVR_Action_Vibration hapticAction;   // Provides for haptic feedback in hand controllers

    void Awake()
    {
        buttonAudio = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Checks if the button that was hit is 'on'
        if (buttonOn)
        {
            // Registers hit to ReactionGameManager
            gameManager.ButtonHit(GetComponent<GameButton>(), GetComponent<Renderer>());

            // Play sound of button being hit
            buttonAudio.Play();

            // Sets button state back to 'off'
            buttonOn = false;

            // Determines which hand hit the button vibrates the corresponding hand controller
            if(other.transform.parent.transform.parent.gameObject.name == "HandColliderLeft(Clone)")
            {
                Debug.Log("Left hand");
                Pulse(0.2f, 150, 75, SteamVR_Input_Sources.LeftHand);
            } else if(other.transform.parent.transform.parent.gameObject.name == "HandColliderRight(Clone)") {
                Debug.Log("Right hand");
                Pulse(0.2f, 150, 75, SteamVR_Input_Sources.RightHand);
            }
        }
    }


    // Personalised function to implement haptic feedback on hand controllers
    private void Pulse(float duration, float frequency, float amplitude, SteamVR_Input_Sources source)
    {
        hapticAction.Execute(0, duration, frequency, amplitude, source);
    }
}
