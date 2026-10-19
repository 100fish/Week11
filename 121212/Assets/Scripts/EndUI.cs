using UnityEngine;

public class EndUI : MonoBehaviour
{
    [Header("EndValues")]
    public int BestAttempts;
    public float BestTime;
    public int ComboCount;

    float overall_score = 0;


    [Header("EndCanvasScores")]
    public EndCanvasScore Attempts;
    public EndCanvasScore Times;
    public EndCanvasScore Kills;
    public EndCanvasScore OverallScore;

    [Header("Animator")]
    public Animator UIAnimator;
    public Animator RetryAnimator;

    public void Pass_Attempts(float attempts)
    {
        overall_score = 0;

        float _value = BestAttempts / attempts;
        if (_value > 1) _value = 1;

        Attempts.Set_Score(_value);

        overall_score += _value;
    }

    public void Pass_Time(float time)
    {
        float _value = BestTime / time;
        if (_value > 1) _value = 1;

        Times.Set_Score(_value);

        overall_score += _value;
    }

    public void Pass_Kills(float Kills)
    {
        float _value = Kills / ComboCount;
        if (_value > 1) _value = 1;

        this.Kills.Set_Score(_value);

        overall_score += _value;
    }

    public void Pass_FinalScore()
    {
        float _value = overall_score / 3;
        if (_value > 1) _value = 1;

        OverallScore.Set_Score(_value);
    }

    public void Start_UI()
    {
        if(overall_score < 0.4f) RetryAnimator.SetBool("Retry", true);
        else RetryAnimator.SetBool("Retry", false);

        UIAnimator.SetBool("Play", true);
    }

    public void End_UI()
    {
        UIAnimator.SetBool("Play", false);
    }


}
