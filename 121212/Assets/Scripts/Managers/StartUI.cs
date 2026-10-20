using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.ReloadAttribute;

public class StartUI : MonoBehaviour
{
    public bool NoTutorial;
    static bool s_NoTutorial;

    NewMenuManager newMenuManager;

    public Animator StartUI_Animator_p;
    Animator StartUI_Animator;
    public Animator Elevator_Animator_p;
    Animator Elevator_Animator;
    public TMP_Text[] text_UI_p;
    TMP_Text[] text_UI;
    public UnityAction function_to_start_recording_p;
    UnityAction function_to_start_recording;
    int current_ID;
    float alpha = 0.0f;
    int alpha_direction = 1;
    float delay_betwee_text_max;
    float delay_betwee_text = 0;
    static bool _no_play = true;

    static bool startTutorial = false;

    public static StartUI Instance;

    void Awake()
    {
        s_NoTutorial = NoTutorial;
        Instance = this;

        current_ID = 0;
        alpha = 0.0f;
        alpha_direction = 1;
        delay_betwee_text_max = 0.25f;
        delay_betwee_text = 0;
    }

    static bool _punch_hit_enemy = false;

    public static void Punch_Hit_Enemy()
    {
        _punch_hit_enemy = true;
             print("WatchYourself 1: + " + _punch_hit_enemy);
    }

    public static void Play_WatchYourself()
    {
        //print("WatchYourself 2: + " + _punch_hit_enemy);
        if (!_punch_hit_enemy) return;

        Set_UI(4);
    }

    static NewPlayerMovement new_player_movement = null;

    private void Start()
    {
        newMenuManager = NewMenuManager.instance;

        text_UI = text_UI_p;
        StartUI_Animator = StartUI_Animator_p;
        Elevator_Animator = Elevator_Animator_p;
        function_to_start_recording = function_to_start_recording_p;

        Start_Room();

        if (s_NoTutorial) Set_UI(50);
    }

    [System.Obsolete]
    public static void First_Player()
    {
        if(s_NoTutorial) return;
        new_player_movement = (NewPlayerMovement)FindObjectOfType(typeof(NewPlayerMovement));
        print("hefffre"); 

        new_player_movement.Set_Jump(false);
        new_player_movement.Set_DoubleJump(false);
        new_player_movement.Set_Slide(false);
        new_player_movement.Set_Punch(false);
    }

    static bool _set_ui = false;

    public static void Move_Through_UI(int UID)
    {
        print("movethroughUI: current -  " + Instance.current_ID + ", new - " + UID);
        if (Instance.current_ID > UID) return;
        print("movethroughUI 2");
        Instance.alpha_direction = -1;
        Instance.delay_betwee_text = 0;
        Instance.alpha = 0;

        _no_play = false;

        if(UID == 0) new_player_movement.Set_Jump(true);
        else if (UID == 1) new_player_movement.Set_DoubleJump(true);
        else if (UID == 2) new_player_movement.Set_Slide(true);
        else if (UID == 3) new_player_movement.Set_Punch(true);
    }

    public static void Set_UI(int num)
    {
        _set_ui = true;
        Instance.current_ID = num + 1;

        Instance.alpha_direction = 1;
        Instance.delay_betwee_text = 0;
        Instance.alpha = -3;

        _no_play = false;
    }

    public static void Next_Text()
    {
        _overlap_alpha = 0;
        if (Instance.current_ID > 3) _no_play = true;

        Instance.current_ID++;

        Instance.alpha_direction = 1;
    }

    static float _overlap_alpha = 0.0f;

    public void Update()
    {
        if(_no_play) return;
        if (current_ID >= text_UI.Length || !startTutorial) return;

        alpha += alpha_direction * (Time.deltaTime * 10);
        _overlap_alpha += Time.deltaTime;
        float _alpha = Mathf.Clamp01(alpha);

        text_UI[current_ID].color = new Color32(255, 255, 255, (byte)(255 * _alpha));

        if(current_ID > 2 && _overlap_alpha > 2.25f) Instance.alpha_direction = -1;

        if (_alpha <= 0 && alpha_direction == -1)
        {
            delay_betwee_text += Time.deltaTime;
            if (delay_betwee_text >= delay_betwee_text_max)
            {
                delay_betwee_text = 0;
                alpha_direction = 1;
                if (_set_ui) return;
                Next_Text();
            }
        }
    }

    public static void Start_Room()
    {
        //Instance.Elevator_Animator.SetBool("Open", false);

        Instance.StartUI_Animator.SetTrigger("PlayText");
    }

    public static void Start_Countdown()
    {
        //Instance.newMenuManager.ChangeRoundState(NewMenuManager.RoundState.Countdown);
    }

    public static void Open_Elevator()
    {
        Instance.Elevator_Animator.SetBool("Open", true);

        if (Instance.function_to_start_recording != null) Instance.function_to_start_recording();

        
    }

    public static void StartTutorial()
    {
        startTutorial = true;
    }
}