using UnityEngine;

public class ElevatorAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    public void Open_Elevator() => animator.SetBool("Open", true);
    public void Reset_Elevator() => animator.SetBool("Open", false);
}
