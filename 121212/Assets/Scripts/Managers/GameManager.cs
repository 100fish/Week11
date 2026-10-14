using System.Threading;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI scoreText;

    [SerializeField]
    TextMeshProUGUI timerText;
    [SerializeField]
    TextMeshProUGUI endScoreText;

    [SerializeField]
    GameObject gameplayPanel;
    [SerializeField]
    GameObject endPanel;

    [SerializeField]
    private float gameTimerMax = 60;
    private float gameTimer;

    private int score;

    private void Update()
    {
        gameTimer -= Time.deltaTime;
        timerText.text = $"Time Remaining: {Mathf.Round(gameTimer)}";

        if(gameTimer < 0)
        {
            EndGame();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        gameTimer = gameTimerMax;
    }

    private void EndGame()
    {
        gameplayPanel.SetActive(false);
        endPanel.SetActive(true);

        endScoreText.text = $"Your score was: {score}";
        Time.timeScale = 0f;
    }

    public void AddScore()
    {
        score += 10;
        scoreText.text = $"Score: {score}";
    }
}
