using TMPro;
using UnityEngine;

public class ElevatorAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] public TextMeshProUGUI deathText;
    public void Open_Elevator() => animator.SetBool("Open", true);
    public void Reset_Elevator() => animator.SetBool("Open", false);
}
