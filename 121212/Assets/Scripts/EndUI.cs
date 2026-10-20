using UnityEngine;

public class EndUI : MonoBehaviour
{
    [Header("EndValues")]
    public float BestAttempts;
    public float BestTime;
    public float KillCount;

    float overall_score = 0;


    [Header("EndCanvasScores")]
    public EndCanvasScore Attempts;
    public EndCanvasScore Times;
    public EndCanvasScore Kills;
    public EndCanvasScore OverallScore;

    [Header("Animator")]
    public Animator UIAnimator;
    public Animator RetryAnimator;


    // Pass Beats
    // Pass Attempts
    // Pass Time
    // Pass Kills
    // Then finally play the animation: Start_UI

    public void Pass_Bests(Vector3 _stats)
    {
        BestAttempts = _stats.x;
        BestTime = _stats.y;
        KillCount = _stats.z;
    }

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

    public void Pass_Kills(float kills)
    {
        float _value = kills / KillCount;
        if (_value > 1) _value = 1;

        Kills.Set_Score(_value);

        overall_score += _value;
    }

    public float FinalScore()
    {
        float _value = overall_score / 3;
        if (_value > 1) _value = 1;

        OverallScore.Set_Score(_value);

        return _value;
    }

    public float Start_UI()
    {
        float final = FinalScore();

        AudioManager.PlayMusic(MUSIC.Applause);
        AudioManager.PlayMusic(MUSIC.EndMusic);

        if (overall_score < 1.2f) RetryAnimator.SetBool("Retry", true);
        else RetryAnimator.SetBool("Retry", false);

        UIAnimator.SetBool("Play", true);

        return final;
    }

    public void End_UI()
    {
        UIAnimator.SetBool("Play", false);
    }


}
