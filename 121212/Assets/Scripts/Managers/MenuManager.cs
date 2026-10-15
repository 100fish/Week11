using NUnit.Framework.Interfaces;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;
using static UnityEngine.Rendering.DebugUI;

public class MenuManager : MonoBehaviour
{
    /*
    //singleton
    public static MenuManager instance;

    [SerializeField] private Camera menuCamera;
    [SerializeField] public Camera gameCamera;

    [SerializeField] private float gameTimerLimit;
    private float gameTimer;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField] private GameObject[] UIPanels;

    [SerializeField] private GameObject gameStartTimerPanel;

    [SerializeField] private TextMeshProUGUI gameStartTimerText;
    [SerializeField] private TextMeshProUGUI gameTimerText;
    [SerializeField] private TextMeshProUGUI gameAmmoText;

    [SerializeField] private TextMeshProUGUI gameEndText;

    private Vector2 mousePosOnUI = new Vector2(Screen.width / 2, Screen.height / 2);

    private bool hasSetupPlay = false;

    enum GameState
    {
        Menu,
        Playing,
        Paused,
        End,
    }

    private GameState gameState = GameState.Menu;

    public enum RoundState
    {
        Menu,
        Tutorial,
        Countdown,
        Gameplay,
        Killcam,
        DeathUI
    }

    public RoundState roundState = RoundState.Menu;

    private ReplayManager replayManager;
    private int runIndex = -1;

    [SerializeField]
    private float runStartTimeMax;
    private float runStartTimer;

    private bool skipTutorial = false;

    private int enemyDeathCount;
    [SerializeField]
    private TextMeshProUGUI enemyDeathCounterText;

    private float totalTime = 0;
    private float totalBullets = 0;


    [Header("Killcam setup")]
    [SerializeField]
    private float killcamTime;
    private float killcamTimer;

    [SerializeField]
    private float killcamHoverTime;
    private float killcamHoverTimer;

    public Vector3 kc_cameraStartPosition;
    private Quaternion kc_cameraStartRotation;

    private Vector3 kc_cameraEndPosition;
    private Quaternion kc_cameraEndRotation;

    [SerializeField]
    private float kc_rotateSpeed;
    private float kc_cameraZAdd = 3;
    private float kc_cameraYAdd = 1;

    private Transform kc_target;

    #region input setup

    private PlayerInput playerControls;

    private void OnEnable()
    {
        playerControls.Enable(); //Enabling and disabling the Input Asset is required 

        playerControls.Standard.Reset.started += ResetScene;
        playerControls.Standard.Pause.started += PauseButtonPressed;

    }

    private void OnDisable()
    {
        playerControls.Disable(); //Enabling and disabling the Input Asset is required 

        playerControls.Standard.Reset.started -= ResetScene;
        playerControls.Standard.Pause.started -= PauseButtonPressed;
    }

    private void Awake()
    {
        instance = this;

        playerControls = new PlayerInput();
    }

    #endregion

    private void Start()
    {
        replayManager = ReplayManager.instance;

        gameTimer = gameTimerLimit;
        runStartTimer = runStartTimeMax;

        killcamTimer = killcamTime;
        killcamHoverTimer = killcamHoverTime;

        player.SetActive(false);

        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        //Debug.Log(roundState);
        switch (gameState)
        {
            case GameState.Playing:

                switch (roundState) 
                {
                    case RoundState.Menu:
                        break;
                    case RoundState.Tutorial:
                        break;
                    case RoundState.Countdown:

                        if (runStartTimer > 0)
                        {//keep counting until round starts
                            runStartTimer -= Time.unscaledDeltaTime;

                            gameStartTimerText.text =  Mathf.Abs(Mathf.CeilToInt( runStartTimer / runStartTimeMax *  (10 - runIndex))).ToString();

                            return;
                        }
                        else if (runStartTimer < 0)
                        {//final round setup
                            gameStartTimerPanel.SetActive(false);
                            ChangeToGameplay();
                        }

                        break;
                    case RoundState.Gameplay:

                        totalTime += Time.deltaTime;

                        gameTimer -= Time.deltaTime;

                        gameTimerText.text = Mathf.Round(gameTimer).ToString() + " seconds";
                        gameAmmoText.text = replayManager.players[runIndex].ammo.ToString() + " bullets";


                        if (enemyDeathCount == 40)
                        {
                            Debug.Log("win");

                            ChangeToEnd();
                        }
                        else if (gameTimer < 0)
                        {
                            if (runIndex >= 9)
                            {
                                ChangeToEnd();
                                return;
                            }

                            EndRound(RoundEndCause.time, null, RoundState.Countdown);

                        }
                        else if (replayManager.players[runIndex].ammo <= 0)
                        {
                            EndRound(RoundEndCause.bullets, null, RoundState.Countdown);
                        }

                        break;
                    case RoundState.Killcam:
                        Time.timeScale = 0;

                        if(killcamTimer > 0)
                        {
                            killcamTimer -= Time.unscaledDeltaTime;

                            gameCamera.transform.position = Vector3.Lerp(kc_cameraEndPosition, kc_cameraStartPosition, killcamTimer / killcamTime);
                            gameCamera.transform.rotation = Quaternion.Lerp(kc_cameraEndRotation, kc_cameraStartRotation, killcamTimer / killcamTime);
                        }
                        else if (killcamHoverTimer > 0)
                        {
                            //gameCamera.transform.LookAt(kc_target);

                            gameCamera.transform.RotateAround(kc_target.position, Vector3.up, kc_rotateSpeed * Time.unscaledDeltaTime);

                            gameCamera.transform.LookAt(kc_target.position);

                            killcamHoverTimer -= Time.unscaledDeltaTime;
                        }
                        else
                        {
                            gameCamera.transform.rotation = kc_cameraStartRotation;

                            killcamTimer = killcamTime;
                            killcamHoverTimer = killcamHoverTime;

                            Time.timeScale = 1;

                            if (runIndex >= 9)
                            {
                                ChangeToEnd();
                                return;
                            }

                            EndRound(RoundEndCause.enemy, null, RoundState.Countdown);
                        }

                        break;
                    case RoundState.DeathUI:
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    #region GameState State Machine

    public void ChangeState(int newState) //interface for other scripts to more easily change scripts
    {
        switch (newState) //Switch chooses which state's function to run from the inputted state
        {
            case (int)GameState.Menu: //explicit cast from Enum to Int (in-class technique)
                ChangeToMenu();
                break;
            case (int)GameState.Playing:
                ChangeToPlaying();
                break;
            case (int)GameState.Paused:
                ChangeToPaused();
                break;
            case (int)GameState.End:
                ChangeToEnd();
                break;
        }
    }

    private void ChangeToMenu() //implementation for the above interface
    {
        gameState = GameState.Menu; //All 'ChangeTo..." functions start by changing to their actual state right away
        FocusUIPanel(UIPanels[(int)GameState.Menu]);
        UnityEngine.Cursor.lockState = CursorLockMode.None;

    }

    private void ChangeToPlaying()
    {
        if(hasSetupPlay == false)
        {
            InitializePlayers();

            if (skipTutorial)
            {
                //replayManager.StartRecording();
                
                StartUI.SkipTutorial();
                ChangeToCountdown();
            }
            else
            {
                StartUI.StartTutorial();
                ChangeToTutorial();
            }

            InitializePlayers();

            hasSetupPlay = true;

        }
        Time.timeScale = 1;

        gameState = GameState.Playing;
        FocusUIPanel(UIPanels[(int)GameState.Playing]);

        playerMovement.enabled = true;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        Mouse.current.WarpCursorPosition(new Vector2(mousePosOnUI.x, mousePosOnUI.y));
    }

    private void ChangeToPaused()
    {
        gameState = GameState.Paused;
        FocusUIPanel(UIPanels[(int)GameState.Paused]);
        Time.timeScale = 0;
        playerMovement.enabled = false;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        mousePosOnUI = new Vector2(Mouse.current.position.x.value, Mouse.current.position.y.value);
    }

    private void ChangeToEnd()
    {
        gameState = GameState.End;
        FocusUIPanel(UIPanels[(int)GameState.End]);

        totalBullets += 10 - replayManager.players[runIndex].ammo;



        gameEndText.text = "USED UP\n" + (Mathf.Round(totalTime / 100) * 100).ToString() + " Seconds\n" + totalBullets + " Bullets\n" + (runIndex + 1) + " Lives\n" + enemyDeathCount + " Enemies";

        Time.timeScale = 1;
        playerMovement.enabled = false;

        SetCameras(0);

        replayManager.Stop();
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    #endregion

    #region RoundState State Machine

    public void ChangeRoundState(int newState) //interface for other scripts to more easily change scripts
    {
        switch ((RoundState)newState)
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
            default:
                break;
        }

    }

    public enum RoundEndCause
    {
        fall,
        enemy,
        bullets,
        time
    }

    private void EndRound(RoundEndCause roundEndCause, GameObject perpetrator, RoundState nextState)
    {
        totalBullets += 10 - replayManager.players[runIndex].ammo;

        switch (nextState)
        {
            case RoundState.Countdown:

                roundState = RoundState.DeathUI;

                switch (roundEndCause)
                {
                    case RoundEndCause.fall:
                        DeathUI.Death("SWALLED \n BY WAVES");

                        break;
                    case RoundEndCause.enemy:
                        DeathUI.Death("RESET \n DONT FORGET");

                        break;
                    case RoundEndCause.bullets:
                        DeathUI.Death("GUNS EMPTY");

                        break;
                    case RoundEndCause.time:
                        DeathUI.Death("TIMES UP");

                        break;
                    default:
                        break;
                }
                
                break;
            case RoundState.Killcam:
                ChangeToKillcam(perpetrator);

                break;
            case RoundState.DeathUI:
                break;
            default:
                break;
        }
    }

    private void ChangeRoundToMenu()
    {
        roundState = RoundState.Menu;
    }

    private void ChangeToTutorial()
    {
        roundState = RoundState.Tutorial;
    }

    private void ChangeToCountdown()
    {
        SetupNextRun();

        roundState = RoundState.Countdown;
    }

    private void ChangeToGameplay()
    {
        StartUI.Open_Elevator();

        roundState = RoundState.Gameplay;
    }

    private void ChangeToKillcam(GameObject perpetrator)
    {
        Time.timeScale = 0;

        if(perpetrator != null)
        {
            kc_cameraStartPosition = gameCamera.transform.position;
            kc_cameraStartRotation = gameCamera.transform.rotation;

            kc_cameraEndPosition = perpetrator.transform.position;
            kc_cameraEndPosition += new Vector3(perpetrator.transform.forward.x * kc_cameraZAdd, perpetrator.transform.forward.y * kc_cameraZAdd + kc_cameraYAdd, perpetrator.transform.forward.z * kc_cameraZAdd);

            kc_cameraEndRotation = Quaternion.LookRotation((perpetrator.transform.position - kc_cameraEndPosition).normalized, Vector3.up);

            kc_target = perpetrator.transform;

            replayManager.players[runIndex].lookSpeed = 0;
        }
        else
        {

        }


        roundState = RoundState.Killcam;
    }

    #endregion

    private void SetupNextRun()
    {
        //Debug.Log("startnextrun");
        runIndex++;

        //Debug.Log("play at " + runIndex + "is " + replayManager.players[runIndex].isActiveAndEnabled);
        if (!replayManager.players[runIndex].isActiveAndEnabled)
        {
            replayManager.NextActivePlayer(runIndex);
            gameCamera = replayManager.players[runIndex].myCamera;
        }

        gameAmmoText.text = replayManager.players[runIndex].ammo.ToString() + " bullets";

        gameStartTimerPanel.SetActive(true);
        runStartTimer = runStartTimeMax;

        gameTimer = gameTimerLimit;

        enemyDeathCount = 0;
        enemyDeathCounterText.text = enemyDeathCount.ToString() + " enemies";

        //resets resetabble objects
        replayManager.ResetScene();
        replayManager.StartRoundReplay();
        //Debug.Log("endnextrun");

    }

    private void PauseButtonPressed(CallbackContext context)
    {
        if (Time.timeScale == 1)
        {
            FocusUIPanel(UIPanels[(int)GameState.Paused]);
            Time.timeScale = 0;
        }
        else if (Time.timeScale == 0)
        {
            FocusUIPanel(UIPanels[(int)GameState.Playing]);
            Time.timeScale = 1;
        }
    }

    private void FocusUIPanel(GameObject focusPanel)
    {
        foreach (var panel in UIPanels)
        {
            panel.gameObject.SetActive(false);
        }
        focusPanel.SetActive(true);
    }

    public void PlayerDeath(GameObject perpetrator)
    {
        if (runIndex >= 9)
        {
            ChangeToEnd();
            return;
        }

        EndRound(RoundEndCause.enemy, perpetrator, RoundState.Killcam);
    }

    private void ResetScene(CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //reset scene
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //reset scene
    }

    private void SwapCameras() //Swaps between the camera for the menu and the player camera
    {
        if (menuCamera.depth == 1)
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

    public void SwapSkipTutorial()
    {
        skipTutorial = !skipTutorial;
    }

    private void InitializePlayers()
    {
        for (int i = 0; i < replayManager.enemies.Count; i++)
        {
            replayManager.enemies[i].id = i;
        }

        for (int i = 0; i < replayManager.players.Length; i++)
        {
            replayManager.players[i].id = i;
            replayManager.players[i].gameObject.SetActive(false);
        }

        replayManager.NextActivePlayer(0);
        gameCamera = replayManager.players[0].myCamera;


    }

    public void EnemyDeath()
    {
        enemyDeathCount++;

        enemyDeathCounterText.text = enemyDeathCount.ToString() + " enemies";
    }
    */
}
