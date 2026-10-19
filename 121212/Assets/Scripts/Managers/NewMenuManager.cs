using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;

public class NewMenuManager : MonoBehaviour
{
    [SerializeField] private EndUI endUI;
    [SerializeField] private ElevatorAnimator elevatorAnimator;
    [SerializeField] private CameraTransition camTrans;
    [SerializeField] private MeshRenderer ocean;
    [SerializeField] private Material oceanBump;

    #region Input and Singleton setup

    //singleton
    public static NewMenuManager instance;

    private PlayerInput playerControls;

    private void OnEnable()
    {
        playerControls.Enable(); //Enabling and disabling the Input Asset is required 

        playerControls.Standard.Reset.started += ResetScene;
        playerControls.Standard.Pause.started += PauseButtonPressed;
        playerControls.Standard.NextLevel.started += NextLevel;

    }

    private void OnDisable()
    {
        playerControls.Disable(); //Enabling and disabling the Input Asset is required 

        playerControls.Standard.Reset.started -= ResetScene;
        playerControls.Standard.Pause.started -= PauseButtonPressed;
        playerControls.Standard.NextLevel.started -= NextLevel;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }


        instance = this;

        DontDestroyOnLoad(gameObject);

        playerControls = new PlayerInput();
    }

    #endregion

    #region Camera Setup
    //The camera used for the menu
    [SerializeField] private Camera menuCamera;
    //The camera used during gameplay
    ///Set it somewhere
    [HideInInspector] public Camera gameCamera;
    #endregion

    #region Player Setup
    //The player
    ///Which is set by someone after its created or needs to be found
    private PlayerMovement playerMovement;

    //Vector2 used to get the mouse to the center of the screen when pausing
    private Vector2 mousePosOnUI = new Vector2(Screen.width / 2, Screen.height / 2);
    #endregion

    #region Enum Setup

    //the state of the GAME
    //Different to state of round
    public enum GameState
    {
        Menu,
        Playing,
        End,
        Elevator
    }

    //the state of the ROUND
    //different to state of the game
    public enum RoundState
    {
        Menu,
        Tutorial,
        Countdown,
        Gameplay,
        Pause,
        Killcam,
        DeathUI
    }

    //Used with the EndRound() Function
    public enum RoundEndCause
    {
        fall,
        projectile,
        player,
        time
    }

    //the game's state
    public GameState gameState = GameState.Menu;

    //the round's state
    public RoundState roundState = RoundState.Menu;

    //Used for pausing the game
    private RoundState stateBeforePause;
    #endregion

    #region Timers/Counters Setup
    //The amount of time the start of the run goes for
    [SerializeField] private float runStartTimeMax;
    //Timer used to keep track of how long the start of the run has so far taken
    private float runStartTimer;

    //The maximum amount of time the game can go on for
    [SerializeField] private float gameTimerLimit;
    //The variable used to time the game
    private float gameTimer;

    //The amount of enemies that have died within the current run
    public int enemyDeathCount;

    //Counts total amount of time for the end stats
    private float totalTime = 0;
    //Counts total amount of bullets for the end stats
    private float totalBullets = 0;
    #endregion

    #region UI Setup

    //The list of UI panels
    [SerializeField] private GameObject[] GameStateUIPanels;

    //The Gameobject of the panel that shows the countdown at the start of each round
    [SerializeField] private GameObject gameStartTimerPanel;

    //The text that shows the countdown at the start of each round
    [SerializeField] private TextMeshProUGUI gameStartTimerText;

    //The text that shows the round's time
    [SerializeField] private TextMeshProUGUI gameTimerText;

    //The text that shows the player's stats after the game ends
    [SerializeField] private TextMeshProUGUI elevatorText;

    //The text that displays the amount of enemies that have died
    [SerializeField] private TextMeshProUGUI enemyDeathCounterText;
    #endregion
    ///No more hitscan, so cut the killcam? Maybe this is a replay now?
    #region Killcam Setup
    [Header("Killcam setup")]
    [SerializeField] private float killcamLerpTime; //How long the killcam lerp should last for
    private float killcamLerpTimer; //Timer used to track how long the killcam lerp has gone for

    [SerializeField] private float killcamHoverTime; //How long the killcam hover should last for
    private float killcamHoverTimer; //Timer used to track how long the killcam hover has gone for

    public Vector3 kc_cameraStartPosition; 
    private Quaternion kc_cameraStartRotation; 

    private Vector3 kc_cameraEndPosition;
    private Quaternion kc_cameraEndRotation;

    [SerializeField]
    private float kc_rotateSpeed;
    private float kc_cameraZAdd = 3; //How far back the killcam should hover
    private float kc_cameraYAdd = 1; //How far up the killcam should hover

    private Transform kc_target;

    #endregion

    #region ReplayManager Setup
    //Script that manages the recording and playback
    [HideInInspector] public NewReplayManager newReplayManager;
    //The run index keeps track of which number run the player is on during the game
    //The run index starts at -1, because when the round starts its moved to 0 for the first player
    public int runIndex = -1;
    #endregion

    #region Start and Update

    private void Start()
    {
        newReplayManager = NewReplayManager.instance;

        oceanBump = ocean.material;

        LoadLevel(0);

        //Timer setup
        gameTimer = gameTimerLimit;
        runStartTimer = runStartTimeMax;

        killcamLerpTimer = killcamLerpTime;
        killcamHoverTimer = killcamHoverTime;

        //gimme that
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    }

    private bool elevatorClosed;
    private void Update()
    {
        //Which gamestate is it
        switch (gameState)
        {
            case GameState.Playing:
                //If we're playing the actual rounds then lets check roundstate
                switch (roundState)
                {
                    case RoundState.Menu:
                        //??? what does this even mean cause menu is a gamestate how is there a roundstate for menu. maybe pause. idek.
                        //ok yeah I checked and its basicalyl just a dummy state for when the round isnt being used
                        ///Rename it?
                        break;
                    case RoundState.Tutorial:
                        //its all good cause all the dynamic tutorial stuff is  handled by whatever other script actually idk
                        break;
                    case RoundState.Countdown:

                        if (runStartTimer > 0)
                        {//keep counting until round starts
                            runStartTimer -= Time.unscaledDeltaTime;

                            //This is the line that makes the countdown as long as the amount of lives you have left
                            ///we aren't doing lives anymore so change this
                            gameStartTimerText.text = Mathf.Abs(Mathf.CeilToInt(runStartTimer / runStartTimeMax * (3))).ToString();

                            ///????
                            return;
                        }
                        else if (runStartTimer < 0)
                        {//final round setup
                            //turn off the countdown Panel
                            gameStartTimerPanel.SetActive(false);
                            //start the round
                            ChangeRoundState(RoundState.Gameplay);
                        }

                        break;
                    case RoundState.Gameplay:

                        //Keep track of game time for end stats
                        totalTime += Time.deltaTime;
                        currentStats.y += Time.deltaTime;

                        //Keep track of round time 
                        gameTimer -= Time.deltaTime;

                        //Set and format timer text
                        gameTimerText.text = Mathf.Round(gameTimer).ToString() + " seconds";

                        //Debug.Log("Runindex player position: " + new Vector2(newReplayManager.players[runIndex].transform.position.x, newReplayManager.players[runIndex].transform.position.z) / 2000);
                        //Shader.SetGlobalVector("_Offset", new Vector2(newReplayManager.players[runIndex].transform.position.x, newReplayManager.players[runIndex].transform.position.z) / 1000);
                        oceanBump.SetVector("_Offset", new Vector2(newReplayManager.players[runIndex].transform.position.x, newReplayManager.players[runIndex].transform.position.z) / 2000);

                        #region Round / Game end checks
                        ///Win condition should be dynamic
                        if (enemyDeathCount == newReplayManager.enemies.Count)
                        {
                            //if all the enemies are dead then end the game
                            ChangeState(GameState.End);
                        }
                        else if (gameTimer < 0)
                        {
                            if (runIndex >= 9)
                            {
                                //If the timer runs out on the final run then end the game
                                ChangeState(GameState.End);
                                return;
                            }

                            //If the timer runs out on a regular run then end the round
                            //With cause for the killcam and reset screen text
                            //And what state to be in cause idk ///tell me
                            EndRound(RoundEndCause.time, null);
                        }
                        #endregion

                        break;
                    case RoundState.Pause:
                        break;
                    case RoundState.Killcam:
                        
                        ///I'm not commenting all of this cause its pretty simple and we're gonna remove it anyway
                        ///The important part is the game HAS TO GO THROUGH HERE to kill the player via being shot
                        ///Idk where it happens yet though but something calls a function when the player is shot
                        ///that changes the roundstate to killcam

                        Time.timeScale = 0;
                            
                        if (killcamLerpTimer > 0)
                        {
                            killcamLerpTimer -= Time.unscaledDeltaTime;

                            //gameCamera.transform.position = Vector3.Lerp(kc_cameraEndPosition, kc_cameraStartPosition, killcamLerpTimer / killcamLerpTime);
                            //gameCamera.transform.rotation = Quaternion.Lerp(kc_cameraEndRotation, kc_cameraStartRotation, killcamLerpTimer / killcamLerpTime);
                        }
                        else if (killcamHoverTimer > 0)
                        { 
                            //gameCamera.transform.RotateAround(kc_target.position, Vector3.up, kc_rotateSpeed * Time.unscaledDeltaTime);

                            //gameCamera.transform.LookAt(kc_target.position);

                            killcamHoverTimer -= Time.unscaledDeltaTime;
                        }
                        else
                        {
                            //gameCamera.transform.rotation = kc_cameraStartRotation;

                            killcamLerpTimer = killcamLerpTime;
                            killcamHoverTimer = killcamHoverTime;

                            Time.timeScale = 1;

                            if (runIndex >= 9)
                            {
                                ChangeState(GameState.End);
                                return;
                            }

                            //Once the killcam is done we start the new round
                            ChangeRoundState(RoundState.DeathUI, RoundEndCause.projectile);
                        }

                        break;
                    case RoundState.DeathUI:
                        //There's a tiny moment where the deathUI is onscreen and its all that should be happening
                        //The actual state machine passing through this state is handled by that script iirc
                        //This should be in the killcam but I wont move it since killcam is going anyway
                        break;
                    default:
                        break;
                }
                break;
            case GameState.End:
                break;
            case GameState.Elevator:
                break;
        }
    }

#endregion

    #region GameState State Machine
    //All the functions used to enter and exit states for the gamestate

    //If anyone anywhere wants to change the gamestate they do it through this script
    ///I'm gonna take a big fucking poop on it hold on rq
    public void ChangeState(GameState newState)
    {
        //Exit functions
        switch (gameState)
        {
            case GameState.Menu: //explicit cast from Enum to Int (in-class technique) (haha look its a comment from when this was an assignment from semester 3)
                ChangeFromMenu();
                break;
            case GameState.Playing:
                ChangeFromPlaying();
                break;
            case GameState.End:
                ChangeFromEnd();
                break;
            case GameState.Elevator:
                ChangeFromElevator();
                break;
        }

        gameState = newState;

        //Entrance functions
        switch (newState) 
        {
            case GameState.Menu: //explicit cast from Enum to Int (in-class technique) (haha look its a comment from when this was an assignment from semester 3)
                ChangeToMenu();
                break;
            case GameState.Playing:
                ChangeToPlaying();
                break;
            case GameState.End:
                ChangeToEnd();
                break;
            case GameState.Elevator:
                ChangeToElevator();
                break;
        }
    }

    public void ButtonChangeGameState(int newState)
    {
        ChangeState((GameState)newState);

        StartUI.First_Player();
    }

    private void ChangeFromMenu() 
    {
        menuCamera.depth = 80;
        Time.timeScale = 1;
    }
    private void ChangeToMenu()
    {
        FocusUIPanel(GameStateUIPanels[(int)GameState.Menu]);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

    [SerializeField] Vector3[] levelBests;

    private void ChangeFromPlaying()
    {
        //Stop time
        Time.timeScale = 0;
        //Free the cursor
        //save mouse's position on UI
        mousePosOnUI = new Vector2(Mouse.current.position.x.value, Mouse.current.position.y.value);

        elevatorClosed = true;
        elevatorAnimator.Reset_Elevator();

        newReplayManager.Stop();


    }
    private void ChangeToPlaying()
    {
        //Setup players and time
        if(!tutorialCompleted)
        {
            InitializePlayerAndEnemies();
            endUI.End_UI();
        }
        Time.timeScale = 1;

        //Turn the gameplay UI on
        FocusUIPanel(GameStateUIPanels[(int)GameState.Playing]);

        //Lock the cursor 
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        //load mouse's position on UI
        Mouse.current.WarpCursorPosition(mousePosOnUI);

        //Change the roundstate to tutorial, which is what actually begins the game
        if(tutorialCompleted /*|| StartUI.Instance.NoTutorial*/)
        {
            ChangeRoundState(RoundState.Countdown);
        }
        else
        {
            ChangeRoundState(RoundState.Tutorial);
        }

        currentStats.x = 0;
        currentStats.y = 0;
        currentStats.z = 0;

    }
    private void ChangeFromEnd()
    {

    }

    private void ChangeToEnd()
    {

    }

    private void ChangeFromElevator()
    {

    }
    private Vector3 currentStats = new Vector3(0,0,0);

    private void ChangeToElevator()
    {

        Time.timeScale = 1;

        currentStats.x = runIndex + 1;
        currentStats.z = enemyDeathCount;

        UnloadLevel(currentLevelObject);
        currentLevel++;

        //call this when the player walks back into the elevator
        FocusUIPanel(GameStateUIPanels[(int)GameState.Elevator]);
        if((int)currentLevel > levels.Length)
        {
            Debug.Log("oops, no more levels!");
        }
        else
        {
            LoadLevel(currentLevel);
        }



        endUI.Pass_Attempts(currentStats.x);
        endUI.Pass_Time(currentStats.y);
        endUI.Pass_Kills(currentStats.z);
        endUI.Pass_Bests(levelBests[(int)currentLevel]);

        currentStats = new Vector3(0, 0, 0);

        Debug.Log("Starting ENDUI");
        endUI.Start_UI();
    }
    #endregion

    public enum Levels
    {
        One,
        Two
    }

    private Levels currentLevel = Levels.One;
    private GameObject currentLevelObject;
    [SerializeField] private GameObject[] levels;

   

    private void LoadLevel(Levels newLevel)
    {
        currentLevelObject = Instantiate(levels[(int)newLevel]);
    }

    private void UnloadLevel(GameObject oldLevel)
    {
        //Clear out old arrays
        newReplayManager.enemies.Clear();


        newReplayManager.players[^1].SetStatePlayback();


        newReplayManager.players[0].transform.position = newReplayManager.players[^1].transform.position;
        newReplayManager.players[0].transform.rotation = newReplayManager.players[^1].transform.rotation;

        for (int i = 0; i < newReplayManager.players.Count; i++)
        {
            newReplayManager.players[i].transform.position = newReplayManager.playerSpawn.position;
            newReplayManager.players[i].transform.rotation = newReplayManager.playerSpawn.rotation;
            newReplayManager.players[i].gameObject.SetActive(false);
        }

        //Moves the first player to the position of the current player
        ///Have to change the first player's texture as well

        InitializePlayerAndEnemies();
        //Reset the player to the first one
        //We keep the old players, deactivated

        //um idk if this is okay
        //Gemini said do this :D
        Destroy(oldLevel);
    }

    #region RoundState State Machine
    //Second verse same as the first
    public void ChangeRoundState(RoundState newState) //interface for other scripts to more easily change scripts
    {
        //Exit Functions
        switch (roundState)
        {
            case RoundState.Menu:
                ChangeRoundFromMenu();
                break;
            case RoundState.Tutorial:
                ChangeFromTutorial();
                break;
            case RoundState.Countdown:
                ChangeFromCountdown();
                break;
            case RoundState.Gameplay:
                ChangeFromGameplay();
                break;
            case RoundState.Pause:
                ChangeFromPause();
                break;
            case RoundState.Killcam:
                ChangeFromKillcam();
                break;
            case RoundState.DeathUI:
                ChangeFromDeathUI();
                break;
            default:
                break;
        }

        roundState = newState;

        //Entrance Functions
        switch (roundState)
        {
            case RoundState.Menu:
                ChangeRoundToMenu();
                break;
            case RoundState.Tutorial:
                ChangeToTutorial();
                break;
            case RoundState.Countdown:
                ChangeToCountdown();
                break;
            case RoundState.Gameplay:
                ChangeToGameplay();
                break;
            case RoundState.Pause:
                ChangeToPause();
                break;
            case RoundState.Killcam:
                ChangeToKillcam(null);
                break;
            case RoundState.DeathUI:
                ChangeToDeathUI(RoundEndCause.fall);
                break;
            default:
                break;
        }
    }

    public void ChangeRoundState(RoundState newState, GameObject perp) //If you wanna go to the killcam use this one
    {
        //Exit Functions
        switch (roundState)
        {
            case RoundState.Menu:
                ChangeRoundFromMenu();
                break;
            case RoundState.Tutorial:
                ChangeFromTutorial();
                break;
            case RoundState.Countdown:
                ChangeFromCountdown();
                break;
            case RoundState.Gameplay:
                ChangeFromGameplay();
                break;
            case RoundState.Killcam:
                ChangeFromKillcam();
                break;
            case RoundState.DeathUI:
                ChangeFromDeathUI();
                break;
            default:
                break;
        }

        roundState = newState;

        //Entrance Functions
        switch (roundState)
        {
            case RoundState.Menu:
                ChangeRoundToMenu();
                break;
            case RoundState.Tutorial:
                ChangeToTutorial();
                break;
            case RoundState.Countdown:
                ChangeToCountdown();
                break;
            case RoundState.Gameplay:
                ChangeToGameplay();
                break;
            case RoundState.Killcam:
                ChangeToKillcam(perp);
                break;
            case RoundState.DeathUI:
                ChangeToDeathUI(RoundEndCause.fall);
                break;
            default:
                break;
        }

    }

    public void ChangeRoundState(RoundState newState, RoundEndCause roundEndCause) //If you wanna go to the DeathUI use this one
    {
        //Exit Functions
        switch (roundState)
        {
            case RoundState.Menu:
                ChangeRoundFromMenu();
                break;
            case RoundState.Tutorial:
                ChangeFromTutorial();
                break;
            case RoundState.Countdown:
                ChangeFromCountdown();
                break;
            case RoundState.Gameplay:
                ChangeFromGameplay();
                break;
            case RoundState.Killcam:
                ChangeFromKillcam();
                break;
            case RoundState.DeathUI:
                ChangeFromDeathUI();
                break;
            default:
                break;
        }

        roundState = newState;

        //Entrance Functions
        switch (roundState)
        {
            case RoundState.Menu:
                ChangeRoundToMenu();
                break;
            case RoundState.Tutorial:
                ChangeToTutorial();
                break;
            case RoundState.Countdown:
                ChangeToCountdown();
                break;
            case RoundState.Gameplay:
                ChangeToGameplay();
                break;
            case RoundState.Killcam:
                ChangeToKillcam(null);
                break;
            case RoundState.DeathUI:
                ChangeToDeathUI(roundEndCause);
                break;
            default:
                break;
        }

    }

    private void ChangeRoundFromMenu()
    {
        ///menu functions are empty cause its a dummy state that might change or be removed
    }
    private void ChangeRoundToMenu()
    {
        ///menu functions are empty cause its a dummy state that might change or be removed
    }

    private bool tutorialCompleted; 

    private void ChangeFromTutorial()
    {
        tutorialCompleted = true;
    }
    private void ChangeToTutorial()
    {
        StartUI.StartTutorial();
    }

    private void ChangeFromCountdown()
    {

    }
    private void ChangeToCountdown()
    {
        //This function is where all of the between round work happens, as its the last
        //Thing that is called before the player is dropped into a new round

        if(runIndex > -1)
        {
            //newReplayManager.NextActivePlayer(runIndex);
            //gameCamera = newReplayManager.players[runIndex + 1].myCamera;
            //camTrans.StartTransition(newReplayManager.players[runIndex].myCamera, newReplayManager.players[runIndex + 1].myCamera, CameraTransition.TransitionCause.Environment);
        }

        //Increase the Run index because its a new round
        runIndex++;

        //Reset countdown Timer 
        gameStartTimerPanel.SetActive(true);
        runStartTimer = runStartTimeMax;

        //Reset game Timer
        gameTimer = gameTimerLimit;

        //Reset enemy death counter
        enemyDeathCount = 0;
        enemyDeathCounterText.text = enemyDeathCount.ToString() + " enemies";



        //Play/Start recording the tape
        newReplayManager.StartRoundPlaybackAndRecording(runIndex);
    }

    private void ChangeFromGameplay()
    {
        //newReplayManager.StopRound(runIndex);
    }
    private void ChangeToGameplay()
    {
        Debug.Log("Hi im the elevator opening");
        elevatorClosed = false;
        elevatorAnimator.Open_Elevator();
    }

    private void ChangeFromPause()
    {
        Time.timeScale = 1;
        playerMovement.enabled = true;

        //Lock the cursor 
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        //load mouse's position on UI
        Mouse.current.WarpCursorPosition(mousePosOnUI);
    }
    private void ChangeToPause()
    {
        ///Fix later
        FocusUIPanel(GameStateUIPanels[2]);
        Time.timeScale = 0;
        playerMovement.enabled = false;

        //Free the cursor
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        //save the mouse's position on the UI
        mousePosOnUI = new Vector2(Mouse.current.position.x.value, Mouse.current.position.y.value);
    }

    ///Killcam is probably being removed
    private void ChangeFromKillcam()
    {
      
    }
    private void ChangeToKillcam(GameObject perpetrator)
    {
        Time.timeScale = 0;

        if (perpetrator != null)
        {
            //kc_cameraStartPosition = gameCamera.transform.position;
            //kc_cameraStartRotation = gameCamera.transform.rotation;

            //kc_cameraEndPosition = perpetrator.transform.position;
            //kc_cameraEndPosition += new Vector3(perpetrator.transform.forward.x * kc_cameraZAdd, perpetrator.transform.forward.y * kc_cameraZAdd + kc_cameraYAdd, perpetrator.transform.forward.z * kc_cameraZAdd);

            //kc_cameraEndRotation = Quaternion.LookRotation((perpetrator.transform.position - kc_cameraEndPosition).normalized, Vector3.up);

            //kc_target = perpetrator.transform;

            //newReplayManager.players[runIndex].lookSpeed = 0;
        }
    }

    private void ChangeFromDeathUI()
    {
        ///Maybe do some stuff about resetting the player here too, maybeeeee ee e ee e e !
    }
    private void ChangeToDeathUI(RoundEndCause roundEndCause)
    {
        //The DEATHUI Script is a harry script
        //Here, we tell it that the player died and it does the UI Animation
        //We set the text depending on the death type
        //Then, it calls back to the menuManager and sets it back to countdown (in DEATHUIOUT)

        Debug.Log("RunIndex: " + runIndex);
        //If the player that is meant to be controlled at this index hasn't been turned on already


        elevatorClosed = true;
        elevatorAnimator.Reset_Elevator();

        switch (roundEndCause)
        {
            case RoundEndCause.fall:
                PlayerTransition(RoundEndCause.fall);
                elevatorAnimator.deathText.text = "SWALLED\nBY WAVES";
                break;
            case RoundEndCause.projectile:
                PlayerTransition(RoundEndCause.projectile);
                elevatorAnimator.deathText.text = "DONT\nSTOP";
                break;
            case RoundEndCause.player:
                PlayerTransition(RoundEndCause.player);
                elevatorAnimator.deathText.text = (newReplayManager.enemies.Count - enemyDeathCount).ToSafeString() +"\nREMAINING";
                break;
            case RoundEndCause.time:
                PlayerTransition(RoundEndCause.time);
                elevatorAnimator.deathText.text = "WATCH\nTHE CLOCK"; 
                break;
            default:
                break;
        }

        //ChangeRoundState(RoundState.Countdown);
    }

    private void PlayerTransition(RoundEndCause roundEndCause)
    {
        if (runIndex != -1)
        {
            //Move to the next player
            newReplayManager.NextActivePlayer(runIndex);
            gameCamera = newReplayManager.players[runIndex + 1].myCamera;
            camTrans.StartTransition(newReplayManager.players[runIndex].myCamera, newReplayManager.players[runIndex +1].myCamera, roundEndCause);
        }
    }

    //This is called when the round ends, there are a lot of checks for it in the Update function of this script
    //It takes the reason that the round ended, and if possible the gameobject that caused it for the killcam
    //But its ok if there's no perp (null)! as long as the cause isn't an enemy
    private void EndRound(RoundEndCause roundEndCause, GameObject perp)
    {

        if(roundEndCause != RoundEndCause.projectile)
        {
            ChangeRoundState(RoundState.DeathUI, roundEndCause);
        }
        else
        {
            ///Killcam off for now, come back later
            //ChangeRoundState(RoundState.Killcam, perp);
            ChangeRoundState(RoundState.DeathUI, roundEndCause);
        }
    }

    #endregion

    #region misc
    //Makes a certain ui panel the only one thats active
    ///now that the state machines have exits and entrances, we can probably make this way better
    private void FocusUIPanel(GameObject focusPanel)
    {
        foreach (var panel in GameStateUIPanels)
        {
            panel.gameObject.SetActive(false);
        }
        focusPanel.SetActive(true);
    }

    //Called by the player movement script when the player is shot or falls into the water
    public void PlayerDeath(GameObject perpetrator, RoundEndCause roundEndCause)
    {
        if (runIndex >= 9)
        {
            //ChangeState(GameState.End);
            return;
        }

        EndRound(roundEndCause, perpetrator);
    }

    //Input function for pausing the game
    private void PauseButtonPressed(CallbackContext context)
    {
        if (gameState == GameState.Playing)
        {
            if (roundState != RoundState.Pause)
            {
                stateBeforePause = roundState;
                ChangeRoundState(RoundState.Pause);
            }
            else
            {
                ChangeRoundState(stateBeforePause);
            }
        }
    }

    private void ResetScene(CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //reset scene
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //reset scene
    }

    private void NextLevel(CallbackContext context)
    {
        if(gameState == GameState.Elevator)
        {
            ChangeState(GameState.Playing);
        }
    }

    private void SetCameras(int gameCameraTrue) //Swaps between the camera for the menu and the player camera
    {
        if (gameCameraTrue == 1)
        {
            menuCamera.depth = 0;
            gameCamera.depth = 1;
        }
        else
        {
            menuCamera.depth = 1;
            gameCamera.depth = 0;
        }
    }

    //This is used by the menumanager, and creates IDs for the players
    //This is for the replaymanager to keep track of them mostly
    ///Will change a lot with dynamic players
    private void InitializePlayerAndEnemies()
    {
        //Foreach enemy give the enemy an ID
        /*for (int i = 0; i < newReplayManager.enemies.Count; i++)
        {
            ///newReplayManager.enemies[i].id = i;
        }*/// COME BACK
        ///COME BACK AND TURN ENEMIES ON
        runIndex = -1;
        newReplayManager.NextActivePlayer(runIndex);
        gameCamera = newReplayManager.players[0].myCamera;
        camTrans.SetupFirstCamera(gameCamera);
    }

    //Called by the enemy when they dide
    public void EnemyDeath()
    {
        enemyDeathCount++;

        enemyDeathCounterText.text = enemyDeathCount.ToString() + " enemies";
    }

#endregion
}
