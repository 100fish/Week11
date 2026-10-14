using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StartUI : MonoBehaviour
{
    MenuManager menuManager;

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
    float delay_betwee_text_max = 0.5f;
    float delay_betwee_text = 0;

    static bool startTutorial = false;

    public static StartUI Instance;

    void Awake()
    {
        Instance = this;

        current_ID = 0;
        alpha = 0.0f;
        alpha_direction = 1;
        delay_betwee_text_max = 0.5f;
        delay_betwee_text = 0;
    }

    private void Start()
    {
        menuManager = MenuManager.instance;

        text_UI = text_UI_p;
        StartUI_Animator = StartUI_Animator_p;
        Elevator_Animator = Elevator_Animator_p;
        function_to_start_recording = function_to_start_recording_p;

        for (var i = 0; text_UI.Length > i; i++)
        {
            text_UI[i].color = new Color32(255, 255, 255, (byte)(0));
        }
        current_ID = 0;
        alpha = 0.0f;
        alpha_direction = 1;
        delay_betwee_text_max = 0.5f;
        delay_betwee_text = 0;
        startTutorial = false;
    }

    public static void Move_Through_UI(int UIID)
    {
        if (Instance.current_ID != UIID || Instance.alpha <= 0.5f) return;

        Instance.alpha_direction = -1;
    }

    public static void Next_Text()
    {
        Instance.current_ID++;

        if (Instance.current_ID >= Instance.text_UI.Length)
        {
            Start_Room();
            return;
        }

        Instance.alpha_direction = 1;
    }

    public void Update()
    {
        if (current_ID >= text_UI.Length || !startTutorial) return;

        alpha += alpha_direction * (Time.deltaTime * 3);
        alpha = Mathf.Clamp01(alpha);

        text_UI[current_ID].color = new Color32(255, 255, 255, (byte)(255 * alpha));

        if(alpha <= 0 && alpha_direction == -1)
        {
            delay_betwee_text += Time.deltaTime;
            if(delay_betwee_text >= delay_betwee_text_max)
            {
                delay_betwee_text = 0;
                alpha_direction = 1;
                Next_Text();
            }
        }
    }

    public static void Start_Room()
    {
        Instance.Elevator_Animator.SetBool("Open", false);

        Instance.StartUI_Animator.SetTrigger("PlayText");
    }

    public static void Start_Countdown()
    {
        Instance.menuManager.ChangeRoundState(2);
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

    public static void SkipTutorial()
    {
        startTutorial = true;
        Debug.Log(Instance.text_UI.Length);
        Instance.text_UI[4].text = "";
        Instance.alpha = 1;
        Instance.current_ID = 4;
        Move_Through_UI(4);

    }
}
