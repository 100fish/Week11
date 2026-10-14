using TMPro;
using UnityEngine;

public class DeathUI : MonoBehaviour
{
    public TMP_Text text_p;
    static TMP_Text text;
    static Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        text = text_p;
    }

    public static void Death(string _message = "RESET \n DONT FORGET")
    {
        Debug.Log("setting death thing");
        text.text = _message;
        animator.SetTrigger("Death");
    }
}
