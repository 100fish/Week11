using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI timeSlowText;

    [SerializeField]
    private float timeSlowIntensifier;

    [SerializeField]
    private float timeSlowDecreaseSpeed;

    [SerializeField]
    private float timeSlowIncreaseSpeed;

    [SerializeField]
    private TimeEventResult timeEventResult;

    private float timeSlow = 100;
    private float startTimeSlow = 100;

    private float timeScaleDefault;

    #region Inputsetup
    //private PlayerInput playerInput;
    //private void Awake()
    //{
    //    playerInput = new PlayerInput();
    //}

    //private void OnEnable()
    //{
    //    playerInput.Enable();
    //    playerInput.Standard.SlowTime.started += OnTimeSlowBegin;
    //    playerInput.Standard.SlowTime.canceled += OnTimeSlowEnd;
    //    playerInput.Standard.Reset.started += RestartScene;

    //}

    //private void OnDisable()
    //{
    //    playerInput.Standard.SlowTime.started -= OnTimeSlowBegin;
    //    playerInput.Standard.SlowTime.canceled -= OnTimeSlowEnd;
    //    playerInput.Standard.Reset.started -= RestartScene;
    //    playerInput.Disable();
    //}
    #endregion Inputsetup
    //private void Start()
    //{
    //    timeScaleDefault = Time.timeScale;
    //}

    //private void Update()
    //{
    //    if(playerInput.Standard.SlowTime.inProgress && startTimeSlow > 50)
    //    {
    //        timeSlow -= Time.unscaledDeltaTime * timeSlowDecreaseSpeed;
    //        timeSlow = Mathf.Clamp(timeSlow, 0, 100);
    //    }
    //    else
    //    {
    //        timeSlow += Time.unscaledDeltaTime * timeSlowIncreaseSpeed;
    //        timeSlow = Mathf.Clamp(timeSlow, 0, 100);
    //    }

    //    if(timeSlow < 0.01)
    //    {
    //        OnTimeSlowEnd();
    //    }

    //    timeSlowText.text = $"Time Slow: {Mathf.Round(timeSlow)}/100";

    //}

    //private void OnTimeSlowBegin(InputAction.CallbackContext context)
    //{
    //    startTimeSlow = timeSlow;

    //    if(startTimeSlow > 50)
    //    {
    //        timeEventResult.OnTimeSlowStart();
    //        Time.timeScale /= timeSlowIntensifier;
    //    }
    //}

    //private void OnTimeSlowEnd(InputAction.CallbackContext context)
    //{
    //    OnTimeSlowEnd();
    //}

    //private void OnTimeSlowEnd()
    //{
    //    timeEventResult.OnTimeSlowStop();

    //    Time.timeScale = timeScaleDefault;
    //}


    private void RestartScene(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
