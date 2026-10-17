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

    public static void Death(NewMenuManager.RoundEndCause roundEndCause)// string _message = "RESET \n DONT FORGET")
    {
        Debug.Log("setting death thing");

        switch (roundEndCause)
        {
            case NewMenuManager.RoundEndCause.fall:
                text.text = "SWELLED\nBY WAVES";
                animator.SetTrigger("Death");
                break;
            case NewMenuManager.RoundEndCause.projectile:
                text.text = "DON'T\nFORGET";
                animator.SetTrigger("Death");
                break;
            case NewMenuManager.RoundEndCause.player:
                text.text = "DON'T\nSTOP";
                animator.SetTrigger("Continue");

                break;
            case NewMenuManager.RoundEndCause.time:
                text.text = "WATCH\nTHE CLOCK";
                animator.SetTrigger("Death");
                break;
            default:
                break;
        }
    }
}
