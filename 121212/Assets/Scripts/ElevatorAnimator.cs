using System.Threading;
using TMPro;
using UnityEngine;

public class ElevatorAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] public TextMeshProUGUI deathText;
    [SerializeField] private bool startOpen;

    [HideInInspector] public bool _mute = false;

    private void Start()
    {
        if(startOpen)
        {
            _mute = true;
            Open_Elevator();
        }
    }

    public void Open_Elevator()
    {
        animator.SetBool("Open", true);
        StartUI.Play_WatchYourself();
        StartUI.Play_WatchYourself();
        StartUI.Play_WatchYourself();
        StartUI.Play_WatchYourself();

        StartUI.Play_WatchYourself();
    }

    public void Reset_Elevator() => animator.SetBool("Open", false);
}
