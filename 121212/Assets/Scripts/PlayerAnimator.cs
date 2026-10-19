using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator Player_Animator;

    public PlayerAnimationState CurrentState;

    bool _moving = false;
    bool _sliding = false;
    bool _falling = false;
    bool _doubleJump = false;

    private void Awake()
    {
        Player_Animator = GetComponent<Animator>();
    }

    public void PlayAnimation(PlayerAnimationState state)
    {
        if (state != PlayerAnimationState.Walk) _moving = false;
        if (state != PlayerAnimationState.Slide) _sliding = false;
        if (state != PlayerAnimationState.Falling || state != PlayerAnimationState.Jump) _falling = false;
        if (state != PlayerAnimationState.DoubleJump || state != PlayerAnimationState.Jump) _doubleJump = false;

        CurrentState = state;

        Player_Animator.Play(state.ToString());
    }

    public void Idle() => PlayAnimation(PlayerAnimationState.Idle);
    public void Walk()
    {
        if (_moving) return; 

        _moving = true;
        PlayAnimation(PlayerAnimationState.Walk);
    }

    public void Run() => PlayAnimation(PlayerAnimationState.Run);
    public void Jump()
    {
        if (_falling) return;

        _falling = true;
        PlayAnimation(PlayerAnimationState.Jump);
    }

    public void Falling()
    {
        if (_falling) return;

        _falling = true;
        PlayAnimation(PlayerAnimationState.Falling);
    }

    public void DoubleJump()
    {
        if (_doubleJump) return;

        _doubleJump = true;
        PlayAnimation(PlayerAnimationState.DoubleJump);
    }

    public void Slide()
    {
        if (_sliding) return;

        _sliding = true;
        PlayAnimation(PlayerAnimationState.Slide);
    }

    public void Start_Punch() => PlayAnimation(PlayerAnimationState.StartPunch);
    public void Punch() => PlayAnimation(PlayerAnimationState.Punch);

    public void Start_Uppercut() => PlayAnimation(PlayerAnimationState.StartUppercut);
    public void Uppercut() => PlayAnimation(PlayerAnimationState.Uppercut);
}

public enum PlayerAnimationState
{
    Idle,
    Walk,
    Run,
    Jump,
    Falling,
    DoubleJump,
    Slide,

    StartPunch,
    Punch,

    StartUppercut,
    Uppercut,
}
