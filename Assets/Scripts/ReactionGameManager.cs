using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Valve.VR.Extras;

public class ReactionGameManager : MonoBehaviour
{
    // Game variables
    public int currentButton;
    public int previousButton;
    public GameObject[] gameButtons;
    public Material buttonLightOff;
    public Material buttonLightOn;
    public Renderer currentButtonRend;
    public GameButton currentButtonSwitch;
    public int score = 0;
    public GameObject rightHand;
    public GameObject gameStartCanvas;
    public Text scoreBoxText;
    public Text timerBoxText;

    // Game timer variables
    public float countdownTimeSetting;  // 60s or 30s modes

    // Game start countdown variables
    public Text countdownText;
    private float currentTime = 0f;
    private float startingTime = 3f;
    public GameObject countdownCanvas;
    private AudioSource reactionMachineAudio; //attach audiosource to ReactionMachine
    public AudioClip countingDownClip;
    public AudioClip startClip;
    public AudioClip endClip;

    // High Score board variables
    public GameObject highScoreCanvas;
    public Text scoreText;
    public Text highScoreText;
    public AudioClip newHighScoreClip;
    public bool mode60s = false;
    public bool mode30s = true;
    private string modeHighScore;
    public Text gameEndText;

    // For Settings menu high score variables
    public ScenePointerHandler scenePointerHandler;

    // Start is called before the first frame update
    void Start()
    {
        gameButtons = GameObject.FindGameObjectsWithTag("GameButton");
        reactionMachineAudio = GetComponent<AudioSource>();

        // Initialise High Score (default at 0)
        highScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
    }

    // Function is called when player clicks the 'Ready!' button
    public void GameStart()
    {
        // Disable laser pointer
        rightHand.GetComponent<SteamVR_LaserPointer>().active = false;

        // Ensure high score board is not displayed
        highScoreCanvas.SetActive(false);

        // Reset score
        score = 0;

        // Reset machine box displays
        scoreBoxText.text = "0";
        timerBoxText.text = "0";

        // Initiate countdown to start game
        StartCoroutine(CountdownToStart());
    }

    // Implements a 3, 2, 1 countdown for game to start
    IEnumerator CountdownToStart()
    {
        currentTime = startingTime;
        
        // Display countdown canvas
        countdownCanvas.SetActive(true);

        // Play first countdown sound (at 3 seconds)
        reactionMachineAudio.clip = countingDownClip;
        reactionMachineAudio.Play();

        // Do 3, 2, 1 countdown for game to begin
        while (currentTime > 0)
        {
            countdownText.text = currentTime.ToString("0");

            if (currentTime == 2 || currentTime == 1)
            {
                reactionMachineAudio.clip = countingDownClip;
                reactionMachineAudio.Play();
            }

            yield return new WaitForSeconds(1);

            currentTime--;

        }

        // Play start sound and disable countdown canvas
        reactionMachineAudio.clip = startClip;
        reactionMachineAudio.Play();
        countdownCanvas.SetActive(false);

        // Set first random button on
        currentButton = Random.Range(0, 11);
        SwitchButtonOn(currentButton);

        // Track button that was just switched on as latest button
        previousButton = currentButton;

        // Start game timer countdown
        StartCoroutine(GameCountdown(countdownTimeSetting));
    }

    // Controls the game timer after game begins
    IEnumerator GameCountdown(float countdownTimeSetting)
    {
        currentTime = 0;
        while (currentTime < countdownTimeSetting)
        {
            yield return new WaitForSeconds(1);
            currentTime++;
            timerBoxText.text = currentTime.ToString();
        }

        // End game when timer reaches 0
        GameEnd();
    }

    // Function is called when a ('on') button is hit
    public void ButtonHit(GameButton hitButton, Renderer hitButtonRend)
    {
        // Update score
        score += 1;
        scoreBoxText.text = score.ToString();

        // Switch off button that was just hit
        hitButtonRend.material = buttonLightOff;
        hitButton.GetComponent<Light>().enabled = false;

        // Randomly pick next button to switch on, excluding the button that was just hit
        while(previousButton == currentButton)
        {
            currentButton = Random.Range(0, 11);
        }
        SwitchButtonOn(currentButton);

        // Track button that was just switched on as latest button
        previousButton = currentButton;
    }

    // Function is called to switch on next button after a button is hit
    public void SwitchButtonOn(int currentButton)
    {
        // Switch on next button
        currentButtonRend = gameButtons[currentButton].GetComponent<Renderer>();
        currentButtonRend.material = buttonLightOn;
        gameButtons[currentButton].GetComponent<Light>().enabled = true;
        currentButtonSwitch = gameButtons[currentButton].GetComponent<GameButton>();
        currentButtonSwitch.buttonOn = true;
    }

    // Function is called when game timer completes - signals game has ended
    public void GameEnd()
    {
        //Sswitch off the current button
        currentButtonRend = gameButtons[currentButton].GetComponent<Renderer>();
        currentButtonRend.material = buttonLightOff;
        currentButtonSwitch = gameButtons[currentButton].GetComponent<GameButton>();
        gameButtons[currentButton].GetComponent<Light>().enabled = false;
        currentButtonSwitch.buttonOn = false;

        // Re-enable laser pointer and controllers
        rightHand.GetComponent<SteamVR_LaserPointer>().active = true;
        scenePointerHandler.player.GetComponent<ShowControllers>().showController = true;

        // Display score, show high scores, provide buttons to play again or go back to home screen
        ShowScores();
    }

    // Displays scores at end of game
    public void ShowScores()
    {
        highScoreCanvas.SetActive(true);

        // Checks timer mode to determine which high score to display
        if (mode30s)
        {
            modeHighScore = "HighScore30s";
        }
        else if (mode60s)
        {
            modeHighScore = "HighScore60s";
        }

        // Checks if a new high score is achieved
        if (score > PlayerPrefs.GetInt(modeHighScore, 0))
        {
            // Show new high score title
            gameEndText.text = "NEW PERSONAL BEST!";

            // Store new high score
            PlayerPrefs.SetInt(modeHighScore, score);
            highScoreText.text = PlayerPrefs.GetInt(modeHighScore).ToString();

            // Play new high score sound
            reactionMachineAudio.clip = newHighScoreClip;
            reactionMachineAudio.Play();

            // Checks timer mode and updates relevant high score in Settings menu
            if (mode30s)
            {
                scenePointerHandler.best30sText.text = "(" + PlayerPrefs.GetInt(modeHighScore).ToString() + ")";
            }
            else if (mode60s)
            {
                scenePointerHandler.best60sText.text = "(" + PlayerPrefs.GetInt(modeHighScore).ToString() + ")";
            }

        }

        // If no high score was achieved
        else
        {
            // Show normal game complete title
            gameEndText.text = "TIME'S UP";

            // Play normal game complete sound
            reactionMachineAudio.clip = endClip;
            reactionMachineAudio.Play();
        }

        // Display scores
        scoreText.text = score.ToString();
        highScoreText.text = PlayerPrefs.GetInt(modeHighScore).ToString();
    }

    
}
