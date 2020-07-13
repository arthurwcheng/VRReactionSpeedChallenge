using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.Extras;
using Valve.VR;

public class ScenePointerHandler : MonoBehaviour
{
    // Credit for original script for laser pointer handling goes to Jesus Najera, in his "SteamVR 2.0 Tutorial: Laser Pointer" tutorial on Medium.com
    // Source: https://medium.com/@setzeus/tutorial-steamvr-2-0-laser-pointer-bbc816ebeec5

    // This script handles all the interactions between the User and the UI via the laser pointer tool, the various options available in the 
    // Settings menu, and transitioning the player between Home and Game positions


    // Laser pointer
    public SteamVR_LaserPointer laserPointer;
    
    // Player variables
    public GameObject player;
    public Transform homePosition;
    public Transform gamePosition;

    // Audio variables
    public AudioSource bgmAudio;
    public AudioSource selectAudio;

    // Game machine variables
    public ReactionGameManager reactionGameManager;
    public GameObject reactionMachine;
    public GameObject scoreBox;
    public GameObject timerBox;
    public Transform machine100pcPosition;
    public Transform machine75pcPosition;
    public Transform scoreBox100pcPosition;
    public Transform scoreBox75pcPosition;
    public Transform timerBox100pcPosition;
    public Transform timerBox75pcPosition;

    // Settings variables
    private Color selectedColour = new Color(0.533f, 0.969f, 1, 1);
    private Color unselectedColour = Color.white;
    public Text best30sText;
    public Text best60sText;

    // UI variables
    public GameObject homeScreenCanvas;
    public GameObject howToPlayCanvas;
    public GameObject gameStartCanvas;
    public GameObject highScoreCanvas;
    public GameObject settingsCanvas;
    public GameObject aboutCanvas;
    public GameObject triggerInstruction;
    private bool firstTrigger = false;

    // Fade In/Out function variables
    public GameObject fadeCanvas;
    public Image fadeImage;

    // Haptic Feedback
    public SteamVR_Action_Vibration hapticAction;


    
    void Awake()
    {
        // This maps the laser pointer events to our user-defined functions below
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += PointerClick;
    }

    public void Start()
    {
        // This initialises the high score numbers shown in the Settings menu
        best30sText.text = "(" + PlayerPrefs.GetInt("HighScore30s",0).ToString() + ")";
        best60sText.text = "(" + PlayerPrefs.GetInt("HighScore60s",0).ToString() + ")";
    }

    public void PointerClick(object sender, PointerEventArgs e)
    {
        // PointerClick is for when the user uses the trigger button on their controller to select/click on UI buttons

        // If a UI button is clicked, play the relevant sound
        if(e.target.tag == "UIButton")
        {
            selectAudio.Play();

            if (!firstTrigger)
            {
                firstTrigger = true;
                triggerInstruction.SetActive(false);
            }
        }

        // UI buttons - first checks the name of the UI button clicked

        if (e.target.name == "PlayGameButton")
        {
            // Transition player to Game position  Disable laser pointer
            StartCoroutine(MovePlayer(gamePosition));

            // Disable laser pointer
            laserPointer.active = false;

            // Set UI panels
            gameStartCanvas.SetActive(true);
            homeScreenCanvas.SetActive(false);

            // Fade BGM out
            StartCoroutine(FadeOutAudio(bgmAudio, 2));

            // Resets any score numbers on the Reaction Game machine
            reactionGameManager.scoreBoxText.text = "0";
            reactionGameManager.timerBoxText.text = "0";
        }
        else if (e.target.name == "GameStartButton")
        {
            // Invokes the procedure to start game from ReactionGameManager
            reactionGameManager.GameStart();

            // Remove UI panel
            gameStartCanvas.SetActive(false);

            // Hides VR controllers from view
            player.GetComponent<ShowControllers>().showController = false;
        }
        else if (e.target.name == "HowToPlayButton")
        {
            // Set UI panels
            howToPlayCanvas.SetActive(true);
            homeScreenCanvas.SetActive(false);
        }
        else if (e.target.name == "SettingsButton")
        {
            // Set UI panels
            settingsCanvas.SetActive(true);
            homeScreenCanvas.SetActive(false);
        }
        else if (e.target.name == "AboutButton")
        {
            // Set UI panels
            aboutCanvas.SetActive(true);
            settingsCanvas.SetActive(false);
        }
        else if (e.target.name == "HowToPlayBackButton")
        {
            // Set UI panels
            homeScreenCanvas.SetActive(true);
            howToPlayCanvas.SetActive(false);
        }
        else if (e.target.name == "SettingsBackButton")
        {
            // Set UI panels
            homeScreenCanvas.SetActive(true);
            settingsCanvas.SetActive(false);
        }
        else if (e.target.name == "AboutBackButton")
        {
            // Set UI panels
            settingsCanvas.SetActive(true);
            aboutCanvas.SetActive(false);
        }
        else if (e.target.name == "ReturnHomeButton")
        {
            // Transitions player back to Home position
            StartCoroutine(MovePlayer(homePosition));

            // Disables laser pointer
            laserPointer.active = false;

            // Set UI panels
            highScoreCanvas.SetActive(false);
            homeScreenCanvas.SetActive(true);

            // Fade BGM in
            StartCoroutine(FadeInAudio(bgmAudio, 2, 0.3f));
        }
        else if (e.target.name == "PlayAgainButton")
        {
            // Set UI panels
            highScoreCanvas.SetActive(false);
            gameStartCanvas.SetActive(true);
        }
        else if (e.target.name == "QuitButton")
        {
            // Quit application
            Application.Quit();
        }

        // Settings buttons - first checks the name of the Settings button

        else if (e.target.name == "75pcButton")
        {
            // Sets size of game machine to 75% and positions appropriately
            reactionMachine.transform.localScale = new Vector3(0.75f, 0.75f, 1);
            reactionMachine.transform.position = machine75pcPosition.position;
            scoreBox.transform.position = scoreBox75pcPosition.position;
            timerBox.transform.position = timerBox75pcPosition.position;

            // Maintains button highlighted to show selected, switches off other option
            SettingColourOnOff(e.target.gameObject, "100pcButton");
        }
        else if (e.target.name == "100pcButton")
        {
            // Sets size of game machine to 100% and positions appropriately
            reactionMachine.transform.localScale = new Vector3(1, 1, 1);
            reactionMachine.transform.position = machine100pcPosition.position;
            scoreBox.transform.position = scoreBox100pcPosition.position;
            timerBox.transform.position = timerBox100pcPosition.position;

            // Maintains button highlighted to show selected, switches off other option
            SettingColourOnOff(e.target.gameObject, "75pcButton");
        }
        else if (e.target.name == "30sButton")
        {
            // Sets game timer mode to 30 seconds
            reactionGameManager.countdownTimeSetting = 30;
            reactionGameManager.mode30s = true;
            reactionGameManager.mode60s = false;

            // Maintains button highlighted to show selected, switches off other option
            SettingColourOnOff(e.target.gameObject, "60sButton");
        }
        else if (e.target.name == "60sButton")
        {
            // Sets game timer mode to 60 seconds
            reactionGameManager.countdownTimeSetting = 60;
            reactionGameManager.mode30s = false;
            reactionGameManager.mode60s = true;

            // Maintains button highlighted to show selected, switches off other option
            SettingColourOnOff(e.target.gameObject, "30sButton");
        }
        else if (e.target.name == "30sResetButton")
        {
            // Clears 30 second mode high score
            ResetHighScore(e.target.name);
        }
        else if (e.target.name == "60sResetButton")
        {
            // Clears 60 second mode high score
            ResetHighScore(e.target.name);
        }
    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        // Checks if laser pointer is hovering over a UI button. If it is, vibrate controller and highlight button
        if (e.target.tag == "UIButton")
        {
            Pulse(0.025f, 1, 1, SteamVR_Input_Sources.RightHand);
            e.target.gameObject.transform.localScale += new Vector3(0.05f, 0.05f, 0);
            var button = e.target.GetComponent<Button>();
            if(button != null)
            {
                button.Select();
            }
        }
    }

    public void PointerOutside(object sender, PointerEventArgs e)
    {
        // Checks if laser pointer leaves a UI button. If it does 'un-highlight' the button
        if (e.target.tag == "UIButton")
        {
            e.target.gameObject.transform.localScale -= new Vector3(0.05f, 0.05f, 0);
            var button = e.target.GetComponent<Button>();
            if (button != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    // Function to control player transition. Game position or Home position should be passed in as argument
    IEnumerator MovePlayer(Transform pos)
    {
        // Fade to black over 2 seconds - sets fading canvas gameobject to 'Active'
        fadeCanvas.SetActive(true);
        fadeImage.canvasRenderer.SetAlpha(0);
        fadeImage.CrossFadeAlpha(1, 2, false);

        // Waits for view to be completely black and then moves player
        yield return new WaitForSeconds(2);
        player.transform.position = pos.position;
        player.transform.rotation = pos.rotation;

        // Waits for 1 second then fades view back in over 2 seconds
        yield return new WaitForSeconds(1);
        fadeImage.CrossFadeAlpha(0, 2, false);

        // Re-enable laser pointer
        laserPointer.active = true;

        // Waits for view to be fully faded back in and sets fading canvas gameobject back to 'Inactive'
        yield return new WaitForSeconds(2);
        fadeCanvas.SetActive(false);
    }

    // Function to fade out audio
    IEnumerator FadeOutAudio(AudioSource audio, float fadeTime)
    {
        float startVolume = audio.volume;
        while (audio.volume > 0)
        {
            audio.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        audio.Stop();
    }

    // Function to Fade in audio
    IEnumerator FadeInAudio(AudioSource audio, float fadeTime, float endVolume)
    {
        audio.Play();
        while (audio.volume < endVolume)
        {
            audio.volume += endVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
    }

    // Controls Settings button colours
    public void SettingColourOnOff(GameObject on, string off)
    {
        on.GetComponent<Image>().color = selectedColour;
        string searchString = "/SettingsCanvas/" + off;
        GameObject.Find(searchString).GetComponent<Image>().color = unselectedColour;
    }

    // Checks which timer mode high score to reset and resets high score
    public void ResetHighScore(string modeButton)
    {
        if (modeButton == "30sResetButton")
        {
            PlayerPrefs.DeleteKey("HighScore30s");
            best30sText.text = "(" + PlayerPrefs.GetInt("HighScore30s",0).ToString() + ")";
        }
        else if (modeButton == "60sResetButton")
        {
            PlayerPrefs.DeleteKey("HighScore60s");
            best60sText.text = "(" + PlayerPrefs.GetInt("HighScore60s",0).ToString() + ")";
        }

    }

    // Personalised function to implement haptic feedback on hand controllers
    public void Pulse(float duration, float frequency, float amplitude, SteamVR_Input_Sources source)
    {
        hapticAction.Execute(0, duration, frequency, amplitude, source);
    }

}
