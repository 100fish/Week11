using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerAnimator : MonoBehaviour
{
    Animator Player_Animator;

    public PlayerAnimationState CurrentState;

    bool _moving = false;
    bool _sliding = false;
    bool _falling = false;
    bool _doubleJump = false;

    List<Vector3> rotations = new List<Vector3>();
    List<Vector3> positions = new List<Vector3>();

    [SerializeField] bool _set = false;

    private void Awake()
    {
        Player_Animator = GetComponent<Animator>();

        Add_Positions_And_Rotations();
        Set_Pos_And_Rotations();
    }

    public void Add_Positions_And_Rotations()
    {
        if (_set) return;
        _set = true;

        Add_Pos(transform);
    }

    public void Add_Pos(Transform _child)
    {
        if (_child.childCount <= 0) return;

        foreach (Transform child in _child)
        {
            positions.Add(child.position);
            rotations.Add(child.eulerAngles);

            Add_Pos(child);
        }

    }

    public void OnDisable()
    {
        Player_Animator.enabled = false;
    }

    int _increment = 0;

    public void Set_Pos_And_Rotations()
    {
        _increment = 0;

        Set_Pos(transform);

        print("increment: " + _increment);

        Player_Animator.enabled = true;
    }

    public void Set_Pos(Transform _child)
    {
        if (_child.childCount <= 0) return;

        foreach (Transform child in _child)
        {
            child.position = positions[_increment];
            child.eulerAngles = positions[_increment];
            _increment++;

            Set_Pos(child);
        }

        _moving = false;
        _sliding = false;
        _falling = false;
        _doubleJump = false;
    }

    public void PlayAnimation(PlayerAnimationState state)
    {
        if (state != PlayerAnimationState.Slide) _sliding = false;
        if (state != PlayerAnimationState.Walk) _moving = false;
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
