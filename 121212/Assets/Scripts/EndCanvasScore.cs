using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndCanvasScore : MonoBehaviour
{
    public TMP_Text Score;
    public Image Score_Color;

    public void Awake()
    {
        Set_Score(Random.Range(0, 1.5f));
    }

    public void Set_Score(float value)
    {
        if (value < 0.4f) Score_Color.color = Color.red;
        else if (value >= 1) Score_Color.color = Color.yellow;
        else Score_Color.color = Color.white;

        if (value < 0.4f) Score.text = "F";
        else if (value < 0.6f) Score.text = "C";
        else if (value < 0.8f) Score.text = "B";
        else if (value < 1f) Score.text = "A";
        else if (value >= 1) Score.text = "S";
    }
}
