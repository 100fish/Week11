using System.Collections;
using TMPro;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

public class NewPlayerMovement : Entities
{
    //When a new player is created, its because its about to be controlled
    //So we default to controlled
    public MovementState movementState = MovementState.Controlled;

    //This player's ID
    //Used for the replay
    public int id;


    public TextMeshProUGUI[] IDTexts = new TextMeshProUGUI[4];

    [Header("Player Params")]
    //MenuManager instance
    public NewMenuManager newMenuManager;

    [SerializeField]
    private PlayerAnimator playerAnimator;

    //The player's character controller
    [SerializeField] private CharacterController characterController;

    //This player's audio Listener
    [SerializeField] public AudioListener myAudioListener;

    //Animator for the camera
    [SerializeField] private Animator camera_shake;
    [SerializeField] Transform leftFist;
    [SerializeField] Transform rightFist;

    [Header("Camera")]
    //This player's camera
    [SerializeField] public Camera myCamera;
    //The highest angle the player can look
    [SerializeField] float camera_max = 80;

    [Header("Movement")]
    //This player's movespeed
    [SerializeField] private float moveSpeed;
    //This player's Lookspeed
    [SerializeField] public float lookSpeed;
    //This player's Gravity
    [SerializeField] private float gravity;


    [Header("Slide")]
    public float slide_amount;
    [SerializeField] private float slide_max = 1f;
    [SerializeField] private float slide_increase = 2;
    [SerializeField] private float slide_delay_max = 0.5f;
    [SerializeField] private float slide_memory_max = 0.1f;
    private float slide_memory;
    private float slide_delay = 0;
    private bool slide_delaying = false;
    Vector3 slide_move = Vector3.zero;
    float slide_bump = 0;

    bool zoom_in = false;
    float base_FOV = 60, zoomed_FOV = 30, zoom_in_current = 0, time_mult = 10;
    float base_gun_pos = 0.0f, new_gun_pos = 0.6f, current_gun_pos = 0.0f;

    #region Jump Setup
    [Header("Jump")]
    [SerializeField] private float jump_height = 1;
    [SerializeField] private float jump_multiplier = 0.3f;
    private float current_jump_multiplier = 1;
    private bool groundedPlayer;
    private bool double_jumped = false;

    [Header("Punch Params")]
    [SerializeField] bool windup_punching = false;
    [SerializeField] bool punching = false;
    bool stop_punch = false;
    [SerializeField] float cannot_punch_delay;
    private float current_cannot_punch_delay = 0;
    [SerializeField] private float hold_punch_time;
    [SerializeField] private float punch_distance;
    private float held_punch_for = 0;
    [SerializeField] private float punch_move_divide;
    public AnimationCurve PunchEvaluation;

    #endregion

    #region Punch Setup
    PunchManager.PunchType punchType;


    [SerializeField] public float punchRange = 3;

    #endregion

    //Whether this player is being controlled
    //Or being played back
    public enum MovementState
    {
        Controlled,
        Playback,
        Dead
    }

    #region Input and Camera setup
    private PlayerInput playerInput;
    private void Awake()
    {
        myEntityType = EntityType.Player;

        myCamera = GetComponentInChildren<Camera>();

        playerInput = new PlayerInput();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        //input setup moved to SetStateControlled & SetStatePlayback
    }
    protected override void OnDisable ()
    {
        base.OnDisable();

        SetStatePlayback();
    }


    #endregion Inputsetup


    protected override void Start()
    {
        base.Start();
        Cursor.visible = false;

        newMenuManager = NewMenuManager.instance;

        TargetManager.Add_Player_To_Scene(transform);

        playerVelocity = Vector3.zero;
        current_jump_multiplier = 1;

        playerAnimator.Idle();
    }

    public bool Is_Ghost()
    {
        if (movementState == MovementState.Controlled) return false;
        else return true;
    }

    private bool Can_LeftPunch()
    {
        //If we're controlling this character, and we're in the correct movementstate
        ///I think the controlled check is unessary cause we setup the input so only the active character is on already right?

        if (punching) return false;
        if (current_cannot_punch_delay > 0) return false;
        if (movementState == MovementState.Controlled && (newMenuManager.roundState == NewMenuManager.RoundState.Gameplay || newMenuManager.roundState == NewMenuManager.RoundState.Tutorial || newMenuManager.roundState == NewMenuManager.RoundState.Countdown)) return true;
        return false;
    }


    private void LeftPunch(InputAction.CallbackContext context)
    {
        if (!Can_LeftPunch()) return;


        ///Maybe Harry should check over this type stuff
        StartUI.Move_Through_UI(3);
        //camera_shake.SetTrigger("Screenshake");

        // AudioManager.PlaySound(SFX.GunShot, 0.2f);

        // CameraShake();

        Punch(false);
    }

    private void RightPunch(InputAction.CallbackContext context)
    {
        if (!Can_LeftPunch()) return;


        ///Maybe Harry should check over this type stuff
        StartUI.Move_Through_UI(3);
        //camera_shake.SetTrigger("Screenshake");

        // AudioManager.PlaySound(SFX.GunShot, 0.2f);

        // CameraShake();

        Punch(true);
    }

    bool upper_cut = false;

    private void Punch(bool _upper_cut)
    {
        upper_cut = _upper_cut;

        if(upper_cut) playerAnimator.Start_Uppercut();
        else playerAnimator.Start_Punch();

        held_punch_for = 0;
        windup_punching = true;
    }

    private void PunchEnd(InputAction.CallbackContext context)
    {
        if (!windup_punching) return;

        stop_punch = true;
    }

    private void Slide(InputAction.CallbackContext contex) => Slide();
    private void Slide()
    {
        if (movementState == MovementState.Controlled)
        {
            StartUI.Move_Through_UI(2);

            if (!groundedPlayer) { slide_memory = slide_memory_max; return; }
            if (slide_delaying) return;

            AudioManager.PlaySound(SFX.SlideRoll, 0.2f);

            characterController.height = 1;

            slide_amount = slide_max;
            slide_move = Get_Movement_Direction(true);
            myCamera.transform.localPosition = new Vector3(myCamera.transform.localPosition.x, myCamera.transform.localPosition.y - 0.55f, myCamera.transform.localPosition.z);
            slide_bump = 2;
        }
    }

    private void StopSlide(InputAction.CallbackContext context) => StopSlide();
    private void StopSlide()
    {
        if (movementState == MovementState.Controlled)
        {
            AudioManager.PlaySound(SFX.SlideRoll, 0.2f, false, true);
            if (slide_amount <= 0) return;

            //AudioManager.PlaySound(SFX.SlideCymbol, 0.2f);

            characterController.height = 2;

            slide_amount = 0;
            myCamera.transform.localPosition = new Vector3(myCamera.transform.localPosition.x, myCamera.transform.localPosition.y + 0.55f, myCamera.transform.localPosition.z);
            slide_delay = slide_delay_max;
            slide_delaying = true;
        }
    }


    private void Update()
    {
        ///DEBUG
        for (int i = 0; i < IDTexts.Length; i++)
        {
            IDTexts[i].text = id.ToString();
        }

        if (movementState != MovementState.Controlled) return;

        //Punch
        if (punching) Punching();
        else if (windup_punching) WindUpPunch();
       

        //move character
        MovePlayer();
        //rotate character
        RotateCamera();
        transform.Rotate(new Vector3(0, playerInput.Standard.MouseLook.ReadValue<Vector2>().x, 0) * lookSpeed);

        if (zoom_in) zoom_in_current += Time.deltaTime * time_mult;
        else zoom_in_current -= Time.deltaTime * time_mult;

        zoom_in_current = Mathf.Clamp01(zoom_in_current);

       // myCamera.fieldOfView = Mathf.Lerp(base_FOV, zoomed_FOV, zoom_in_current);
        float _gun_transform_z = Mathf.Lerp(base_gun_pos, new_gun_pos, zoom_in_current);
        leftFist.localPosition = new Vector3(0, 0, _gun_transform_z);
        rightFist.localPosition = new Vector3(0, 0, _gun_transform_z);

        AnimatingPlayer();
    }

    #region Camera and Zoom

    void RotateCamera()
    {
        Vector3 rot = myCamera.transform.localRotation.eulerAngles + new Vector3(-playerInput.Standard.MouseLook.ReadValue<Vector2>().y * lookSpeed, 0f, 0f); //use local if your char is not always oriented Vector3.up
        rot.x = ClampAngle(rot.x, -camera_max, camera_max);

        myCamera.transform.localEulerAngles = rot;
    }

    float ClampAngle(float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }


    //Shakes the camera when shooting
    public void CameraShake()
    {
        camera_shake.SetTrigger("Screenshake");
    }

    //Called by input when zooming
    private void Start_ZoomIn(InputAction.CallbackContext contex)
    {
        zoom_in = true;
        //Why move the ui when zooming?
        StartUI.Move_Through_UI(4);
    }
    private void Stop_ZoomIn(InputAction.CallbackContext contex) => zoom_in = false;

    #endregion

    #region Movement

    public Vector3 Get_Movement_Direction(bool _forward_default = false)
    {
        Vector3 _direction = (transform.forward * playerInput.Standard.Movement.ReadValue<Vector2>().y) + (transform.right * playerInput.Standard.Movement.ReadValue<Vector2>().x);
        if (_forward_default && _direction == Vector3.zero) _direction = transform.forward;
        return _direction;
    }

    Vector3 playerVelocity = Vector3.zero;

    public void WindUpPunch()
    {
        //print("WindUpPunch");

        held_punch_for += Time.deltaTime;

        if (held_punch_for >= hold_punch_time || stop_punch == true && held_punch_for >= hold_punch_time / 2) punching = true;
    }

    public bool PunchHappening()
    {
        if (windup_punching || punching) return true;
        return false;
    }

    public void Punching()
    {
        //print("Punching: " + held_punch_for);

        if(windup_punching)
        {
            if(upper_cut) playerAnimator.Uppercut();
            else playerAnimator.Punch();
        }

        stop_punch = false;
        windup_punching = false;
        punching = true;

        Vector3 _moveAxis = Get_Movement_Direction(true);

        float _punch_distance = PunchEvaluation.Evaluate(held_punch_for / hold_punch_time);

        float _x_distance = 1;
        float _y_distance = 1;

        if (!upper_cut) _x_distance = _punch_distance * punch_distance;
        else _y_distance = _punch_distance * punch_distance;

        Vector3 finalMove = _moveAxis * (_x_distance) + Vector3.up * _y_distance + (Vector3.back * playerVelocity.z);

        characterController.Move(finalMove * Time.deltaTime);
        held_punch_for -= Time.deltaTime;

        PunchManager.Punch(this, transform, PunchManager.PunchType.Left, true, punchRange);
        punchType = PunchManager.PunchType.Left;

        if (held_punch_for <= 0)
        {
            punching = false;
        }



    }

    public void MovePlayer()
    {
        //if (transform.position.y <= -15) Hit_Entity();
        Vector3 _moveAxis = Get_Movement_Direction();
        float _moveSpeed = moveSpeed;

        groundedPlayer = characterController.isGrounded;

        if (groundedPlayer)
        {
            if (playerVelocity.y < -2f)
                playerVelocity.y = -2f;

            current_jump_multiplier -= Time.deltaTime / 2;
            if (current_jump_multiplier <= 1) current_jump_multiplier = 1;
        }

        if (slide_move != Vector3.zero && slide_amount > 0)
        {
            float _lerp_amount = 0.01f;
            float _divide_slide_amount = 2;
            if (slide_amount < slide_max / _divide_slide_amount) _lerp_amount = 0.01f + (1 - (slide_amount / (slide_max / _divide_slide_amount)));

            if (_moveAxis == Vector3.zero) _moveAxis = slide_move;
            else _moveAxis = Vector3.Lerp(slide_move, _moveAxis, _lerp_amount);

            _moveSpeed *= slide_increase;
            slide_amount -= Time.deltaTime;
            if (slide_amount <= 0)
            {
                StopSlide();
            }
        }

        if (slide_delaying)
        {
            slide_delay -= Time.deltaTime;
            if (slide_delay <= 0) slide_delaying = false;
        }

        if (slide_memory > 0)
        {
            slide_memory -= Time.deltaTime;
            if (groundedPlayer) Slide();
        }

        // Jump using WasPressedThisFrame()
        if (playerInput.Standard.Jump.WasPressedThisFrame() && (groundedPlayer || !double_jumped))
        {
            playerAnimator.Jump();
            playerVelocity.y = Mathf.Sqrt(jump_height * -2f * gravity);
            wallJump();

            if (!groundedPlayer)
            {
                double_jumped = true;
                StartUI.Move_Through_UI(1);
                AudioManager.PlaySound(SFX.Jump, 0.1f);
            }
            else
            {
                StartUI.Move_Through_UI(0);
                double_jumped = false;
                AudioManager.PlaySound(SFX.DoubleJump, 0.25f);
            }

            if (slide_amount > 0) current_jump_multiplier = slide_increase;

            StopSlide();

            current_jump_multiplier += jump_multiplier;
        }

        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;

        if (playerVelocity.z > 0)
        {
            playerVelocity.z += (gravity * Time.deltaTime);
            if (playerVelocity.z <= 0) playerVelocity.z = 0;
        }

        // Move
        Vector3 finalMove = _moveAxis * (_moveSpeed * current_jump_multiplier) + Vector3.up * playerVelocity.y + (Vector3.back * playerVelocity.z * wall_jump_dir);
        if (slide_bump != 0)
        {
            finalMove *= slide_bump;
            slide_bump = 0;
        }

        if(PunchHappening()) finalMove = new Vector3(finalMove.x / punch_move_divide, finalMove.y / punch_move_divide, finalMove.z / punch_move_divide);

        //print("finalMove: " + finalMove + ", moveAxis: " + _moveAxis + ", _moveSpeed" + _moveSpeed + ", _moveAxis: " + playerVelocity.y);
        characterController.Move(finalMove * Time.deltaTime);

        //characterController.Move(new Vector3(_moveAxis.x * _moveSpeed, gravity, _moveAxis.z * _moveSpeed));


        velocity = finalMove;
    }

    Vector3 velocity;

    int wall_jump_dir = 1;

    public void wallJump()
    {
        RaycastHit Lhit;
        RaycastHit Rhit;

        Physics.Raycast(transform.position, transform.right, out Lhit, 1);
        Physics.Raycast(transform.position, -transform.right, out Rhit, 1);

        if (Lhit.collider != null)
        {
            playerVelocity.z = 10f;
            playerVelocity.y += 2.5f;
            wall_jump_dir = 1;
        }
        else if (Rhit.collider != null)
        {
            playerVelocity.z = 10f;
            playerVelocity.y += 2.5f;
            wall_jump_dir = -1;
        }
    }

   

    public void AnimatingPlayer()
    {
        if (PunchHappening()) return;


        if (slide_amount > 0)
        {
            playerAnimator.Slide();
            return;
        }

        if (!groundedPlayer)
        {
            if(double_jumped) playerAnimator.DoubleJump();
            else playerAnimator.Falling();
            return;
        }

        Vector3 _moveAxis = Get_Movement_Direction();

        if(_moveAxis != Vector3.zero) playerAnimator.Move();
        else playerAnimator.Idle();
    }

    #endregion

    public override void Hit_Entity(Entities perpetrator, PunchManager.PunchType punchType)
    {
        if (movementState == MovementState.Controlled && newMenuManager.roundState == NewMenuManager.RoundState.Gameplay)
        {
            CameraShake();

            switch (perpetrator.myEntityType)
            {
                case EntityType.Player:
                    newMenuManager.PlayerDeath(perpetrator.gameObject, NewMenuManager.RoundEndCause.player);
                    break;
                case EntityType.Enemy:
                    break;
                case EntityType.Projectile:
                    newMenuManager.PlayerDeath(perpetrator.gameObject, NewMenuManager.RoundEndCause.projectile);
                    break;
                default:
                    break;
            }

        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Kill" && movementState == MovementState.Controlled && newMenuManager.roundState == NewMenuManager.RoundState.Gameplay)
        {
            newMenuManager.PlayerDeath(gameObject, NewMenuManager.RoundEndCause.fall);
        }
        else if(other.gameObject.tag == "Elevator" && movementState == MovementState.Controlled && newMenuManager.gameState == NewMenuManager.GameState.End)
        {
            newMenuManager.ChangeState(NewMenuManager.GameState.Elevator);
        }
    }

    #region replay

    //Called by the replaymanager when changing the active player
    public void SetStateControlled()
    {
        playerInput = new PlayerInput();

        myCamera.enabled = true;
        myAudioListener.enabled = true;

        movementState = MovementState.Controlled;

        playerInput.Enable();
        playerInput.Standard.Shoot.started += LeftPunch;
        playerInput.Standard.Shoot.canceled += PunchEnd;

        playerInput.Standard.ZoomIn.started += RightPunch;
        playerInput.Standard.ZoomIn.canceled += PunchEnd;

        playerInput.Standard.Slide.started += Slide;
        playerInput.Standard.Slide.canceled += StopSlide;



        playerInput.Standard.ESC.started += Exit;

        characterController.enabled = true;
    }

    //Called by the replaymanager when changing the active player
    public void SetStatePlayback()
    {
        myCamera.enabled = false;
        myAudioListener.enabled = false;

        movementState = MovementState.Playback;

        //playerInput.Disable();
        playerInput.Standard.Disable();
        playerInput.Standard.Shoot.started -= LeftPunch;
        playerInput.Standard.Shoot.canceled -= PunchEnd;


        playerInput.Standard.ZoomIn.started -= RightPunch;
        playerInput.Standard.ZoomIn.canceled -= PunchEnd;


        playerInput.Standard.Slide.started -= Slide;
        playerInput.Standard.Slide.canceled -= StopSlide;


        playerInput.Standard.ESC.started -= Exit;

        characterController.enabled = false;
    }

    private void Exit(InputAction.CallbackContext context) => Application.Quit();

    bool isDead;

    public NewPlayerSnapshotInfo SaveSnapshot()
    {

        NewPlayerSnapshotInfo NewSnapshotInfo = new NewPlayerSnapshotInfo()
        {
            replay_id = id,
            replay_position = transform.position,
            replay_rotation = transform.rotation,
            replay_punched = punchType,
            replay_dead = isDead
        };

        //Punch type is initiall changed from none in the Punch Function
        //Once we've saved an active punch we make sure we dont save it again
        if (punchType != PunchManager.PunchType.None)
        {
            punchType = PunchManager.PunchType.None;
        }

        return NewSnapshotInfo;
    }


    public void LoadSnapshot(NewPlayerSnapshotInfo info)
    {
        transform.position = info.replay_position;
        transform.rotation = info.replay_rotation;

        if (info.replay_punched != PunchManager.PunchType.None)
        {
            ///Right now this only punhces enemies
            //its false cause its kind of a fake punch
            //When a replay punches we want some things to happen, but not others
            //Entities that are being replayed should not get punched by this
            //But entities that aren't being replayed should
            //And all of the right effects should play
            ///So when an enemy is set to playback, then they shouldn't get affected by punches, atleast in a force way
            ///They'll push themselves back / kill themselves
            ///UNLESSTHISPUNCHISOVERWRITTENINWHICHCASENEVERMIND
            PunchManager.Punch(this, transform, info.replay_punched, true, punchRange);
        }
    }

    #endregion

}
