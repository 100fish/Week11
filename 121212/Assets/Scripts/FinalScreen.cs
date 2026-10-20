using TMPro;
using UnityEngine;

public class FinalScreen : MonoBehaviour
{
    public GameObject All;
    public TMP_Text time;
    public EndCanvasScore _overall_score;

    public static FinalScreen instance;

    public void Awake()
    {
        instance = this;

        All.SetActive(false);
    }

    public void Activate_FinalScreen()
    {
        All.SetActive(true);

        Pass_FinalTime(NewMenuManager.instance.total_time);
        Pass_FinalScore(NewMenuManager.instance.total_score);
    }

    public void Pass_FinalTime(float _time)
    {

        time.text = "TIME: " + Mathf.Round(_time).ToString();
    }

    public void Pass_FinalScore(float _value)
    {
        _overall_score.Set_Score(_value);
    }
}
