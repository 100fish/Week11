using NUnit.Framework;
using System.Collections;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class NewPlayerMovement : Entities
{
    bool check_framerate = false;
    [SerializeField] Material[] materials;

    [SerializeField] SkinnedMeshRenderer meshRenderer;

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
    [SerializeField] public CapsuleCollider player_collider;
    [SerializeField] Transform leftFist;
    [SerializeField] Transform rightFist;

    [Header("Camera")]
    //This player's camera
    [SerializeField] public Camera myCamera;
    [SerializeField] public float min_FOV, max_FOV;
    //The highest angle the player can look
    [SerializeField] float camera_max = 80;

    [SerializeField] float camera_speed_x = 0.0f;
    [SerializeField] float camera_speed_y = 0.0f;
    [SerializeField] float camera_speed_z = 0.0f;
    [SerializeField] private float slide_camera_y_change = 0.0f;
    private float current_slide_camera_y_change;

    [SerializeField] private LayerMask cameraLayerMask;

    [SerializeField] private float cameraDistance;

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
    [SerializeField] private float max_can_decrease_slide_time = 0.0f;
    float can_decrease_slide_time = 0.0f;

    #region Jump Setup
    [Header("Jump")]
    [SerializeField] private float jump_height = 1;
    [SerializeField] private float jump_multiplier = 0.3f;
    public  float current_jump_multiplier = 1;
    private bool groundedPlayer;
    [SerializeField] private float max_kyote_time = 0.5f;
    private float current_kyote_time = 0;
    [SerializeField] private bool double_jumped = false;
    private bool jumped = false;
    //[SerializeField] private LayerMask groundMask;
    //[SerializeField] private float ground_distance = 0.5f;
    private float jump_input_storage = 0;
    [SerializeField] private float max_jump_input_storage = 0.5f;
    [SerializeField] private float falling_time_to_reach_anim = 0.0f;
    [SerializeField] private float max_falling_time_to_reach_anim = 0.1f;
    [SerializeField] private float animation_y_pos_dif = 0.0f;

    [Header("Punch Params")]
    [SerializeField] bool windup_punching = false;
    [SerializeField] bool punching = false;
    private bool punched_in_air = false;
    bool stop_punch = false;
    [SerializeField] float cannot_punch_delay;
    private float current_cannot_punch_delay = 0;
    [SerializeField] private float hold_punch_time;
    [SerializeField] private float punch_distance;
    private float held_punch_for = 0;
    [SerializeField] private float punch_move_divide;
    public AnimationCurve PunchEvaluation;
    bool upper_cut = false;

    bool _slide_audio;
    bool can_jump = true;
    public void Set_Jump(bool _can)
    {
        can_jump = _can;
        print("hefffre 3: " + can_jump);
    }
    bool can_double_jump = true;
    public void Set_DoubleJump(bool _can) => can_double_jump = _can;
    bool can_slide= true;
    public void Set_Slide(bool _can) => can_slide = _can;
    bool can_punch = true;
    public void Set_Punch(bool _can) => can_punch = _can;
    bool can_uppercut = true;
    public void Set_Uppercut(bool _can) => can_uppercut = _can;

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

        Application.targetFrameRate = 60;

        print("hefffre 2");
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

    //[SerializeField] private Vector3 startCameraPos = new Vector3 (0.42899999f, 1.704f, -2.24099994f);

    protected override void Start()
    {
        base.Start();

        previousCameraPos = myCamera.transform.position;
        previousCameraRot = myCamera.transform.rotation;

        UnityEngine.Cursor.visible = false;

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

        if (punched_in_air) return false;
        if (punched_in_air) return false;
        if (punching) return false;
        if (current_cannot_punch_delay > 0) return false;
        if (movementState == MovementState.Controlled && (newMenuManager.roundState == NewMenuManager.RoundState.Gameplay || newMenuManager.roundState == NewMenuManager.RoundState.Tutorial || newMenuManager.roundState == NewMenuManager.RoundState.Countdown)) return true;
        return false;
    }


    private void LeftPunch(InputAction.CallbackContext context)
    {
        if (!can_punch) return;
        if (!Can_LeftPunch()) return;


        ///Maybe Harry should check over this type stuff
        //camera_shake.SetTrigger("Screenshake");

        // AudioManager.PlaySound(SFX.GunShot, 0.2f);

        // CameraShake();

        Punch(false);
    }

    private void RightPunch(InputAction.CallbackContext context)
    {
        if (!can_uppercut) return;
        if (!Can_LeftPunch()) return;


        ///Maybe Harry should check over this type stuf
        //camera_shake.SetTrigger("Screenshake");

        // AudioManager.PlaySound(SFX.GunShot, 0.2f);

        // CameraShake();

        //Debug.Log("starting Righpunch func1");


        Punch(true);
    }

    private void Punch(bool _upper_cut)
    {
        Debug.Log("starting punch func0");


        if (!groundedPlayer)
        {
            punched_in_air = true;
        }

        upper_cut = _upper_cut;

        if(upper_cut) 
        {
            Debug.Log("starting uppercut");
            playerAnimator.Start_Uppercut();
            
        }
        else playerAnimator.Start_Punch();

        held_punch_for = 0;
        windup_punching = true;
    }

    private void PunchEnd(InputAction.CallbackContext context)
    {
        if (!windup_punching) return;

        stop_punch = true;
    }

    bool _slide_sfx = false;
    bool _no_ground_slide = false;

    public void Slide_Roll(bool _slide)
    {
        AudioManager.PlaySound(SFX.SlideRoll, 0.2f, true, !_slide);
    }

    private void Slide(InputAction.CallbackContext contex) => Slide();
    private void Slide()
    {
        if (!can_slide) return;

        if (movementState == MovementState.Controlled)
        {
            if (!groundedPlayer && current_kyote_time <= 0) { slide_memory = slide_memory_max; return; }
            if (slide_delaying) return;

            AudioManager.PlaySound(SFX.StartSlide, 0.1f);
            _slide_sfx = true;
            _no_ground_slide = false;
            Slide_Roll(true);

            slide_amount = slide_max;
            slide_move = Get_Movement_Direction(true);
            slide_bump = 2;
        }
    }

    private void StopSlide(InputAction.CallbackContext context) => StopSlide();
    private void StopSlide()
    {
        if (movementState == MovementState.Controlled)
        {
            if (slide_amount <= 0) return;

            //AudioManager.PlaySound(SFX.SlideCymbol, 0.2f);
            double_jumped = false;

            AudioManager.PlaySound(SFX.EndSlide, 0.1f);
            _slide_sfx = false;
            _no_ground_slide = false;
            Slide_Roll(false);

            slide_amount = 0;
            slide_delay = slide_delay_max;
            slide_delaying = true;
        }
    }

    Vector3 previousCameraPos;
    Quaternion previousCameraRot;

    int[] fps = new int[30];
    int current_fps = 0;

    public void Reset_Player()
    {
        groundedPlayer = true;
        current_jump_multiplier = 1;
        transform.position = new Vector3(0, 0, 0);

        _slide_sfx = false;
        _no_ground_slide = false;
        Slide_Roll(false);

        slide_amount = 0;
        slide_delay = slide_delay_max;
        slide_delaying = true;

        playerAnimator.Set_Pos_And_Rotations();
    }

    private void Update()
    {

        ///DEBUG
        for (int i = 0; i < IDTexts.Length; i++)
        {
            IDTexts[i].text = NumberToWords(id + 1).ToUpper();
        }

        Check_FrameRate();

        if (movementState != MovementState.Controlled) return;

        myCamera.transform.localRotation = previousCameraRot;
        myCamera.transform.localPosition = previousCameraPos;

        //Punch
        if (punching) Punching();
        else if (windup_punching)
        {
            WindUpPunch();
        }

        if (movementState != MovementState.Controlled) return;

        //move character
        MovePlayer();
        //rotate character
        RotateCamera();
        transform.Rotate(new Vector3(0, playerInput.Standard.MouseLook.ReadValue<Vector2>().x, 0) * lookSpeed);

        AnimatingPlayer();

        previousCameraRot = myCamera.transform.localRotation;
        previousCameraPos = myCamera.transform.localPosition;

        CameraRaycast();

        if (slide_amount > 0)
        {
            if (!groundedPlayer && current_kyote_time <= max_kyote_time - 0.05f && !_no_ground_slide)
            {
                _no_ground_slide = true;
                Slide_Roll(false);
            }
            else if (groundedPlayer && _no_ground_slide)
            {
                _no_ground_slide = false;
                Slide_Roll(true);
            }

            characterController.height = 1;
            new_camera_position_change_y = -0.55f;

            player_collider.height = 1;
            player_collider.center = new Vector3(0, -0.499f, 0);
        }
        else
        {
            characterController.height = 2;
            new_camera_position_change_y = 0.55f;

            player_collider.height = 2;
            player_collider.center = Vector3.zero;
        }

        float _current_jump_mult = current_jump_multiplier - 1.5f;
        if (_current_jump_mult <= 0) _current_jump_mult = 0;
        if (_current_jump_mult > 2) _current_jump_mult = 2;

        float _FOV = Mathf.Lerp(min_FOV, max_FOV, _current_jump_mult / 2);

        myCamera.fieldOfView = _FOV;

        //cameraDistance = Vector3.Distance(myCamera.transform.position, transform.position);

        if (NewMenuManager.instance.is_UpperCut_On()) Set_Uppercut(true);
        else Set_Uppercut(false);
    }

    public void Check_FrameRate()
    {
        IDTexts[2].gameObject.SetActive(check_framerate);

        if (!check_framerate) return;

        current_fps++;
        if (current_fps >= fps.Length) current_fps = 0;

        fps[current_fps] = Mathf.RoundToInt(1.0f / Time.deltaTime);

        float _current_fps = 0;

        for (int i = 0; i < fps.Length; i++)
        {
            _current_fps += fps[i];
        }

        _current_fps /= fps.Length;

        IDTexts[2].text = (Mathf.RoundToInt(_current_fps)).ToString() + "fps";
    }

    private void CameraRaycast()
    {
        RaycastHit ray;

        Vector3 _camera_pos = new Vector3(myCamera.transform.position.x, myCamera.transform.position.y + current_camera_position_change_y, myCamera.transform.position.z);
        //print("camerapos: " + _camera_pos);

        if (Physics.Raycast(transform.position, (_camera_pos - transform.position).normalized, out ray, cameraDistance, cameraLayerMask))
        {
            myCamera.transform.position = ray.point;
        }

    }

    #region Camera and Zoom

    Vector3 current_camera_rotation = Vector3.zero;
    float current_camera_position_change_y = 0;
    float new_camera_position_change_y = 0;

    void RotateCamera()
    {
        Vector3 rot = myCamera.transform.localRotation.eulerAngles + new Vector3(-playerInput.Standard.MouseLook.ReadValue<Vector2>().y * lookSpeed, 0f, 0f); //use local if your char is not always oriented Vector3.up
        rot.x = ClampAngle(rot.x, -camera_max, camera_max);

        current_camera_rotation = rot;

        if (current_camera_rotation == Vector3.zero) current_camera_rotation = rot;

        float _x_camera_pos_lerp = Mathf.Lerp(current_camera_rotation.x, rot.x, camera_speed_x / 1);
        float _y_camera_pos_lerp = Mathf.Lerp(current_camera_rotation.y, rot.y, camera_speed_x / 1);
        float _z_camera_pos_lerp = Mathf.Lerp(current_camera_rotation.z, rot.y, camera_speed_x / 1);

        current_camera_rotation = new Vector3(_x_camera_pos_lerp, _y_camera_pos_lerp, _z_camera_pos_lerp);
        myCamera.transform.localEulerAngles = current_camera_rotation;

        //current_camera_position_change_y = Mathf.Lerp(current_camera_position_change_y, new_camera_position_change_y, camera_speed_x / 1);
        //myCamera.transform.localPosition = new Vector3(myCamera.transform.localPosition.x, myCamera.transform.localPosition.y + current_camera_position_change_y, myCamera.transform.localPosition.z);
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
            AudioManager.PlaySound(SFX.Punch_Woosh);

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

        //Debug.Log("Hi I'm " + id);
        
        if(upper_cut)
        {
            PunchManager.Punch(this, transform, PunchManager.PunchType.Right, true, punchRange + 3);
            punchType = PunchManager.PunchType.Right;
        } else
        {
            PunchManager.Punch(this, transform, PunchManager.PunchType.Left, true, punchRange);
            punchType = PunchManager.PunchType.Left;
        }

        if (held_punch_for <= 0)
        {
            punching = false;

            if (upper_cut) playerVelocity.y = Mathf.Sqrt(jump_height * -1f * gravity);
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

            float _jump_mult_mult = 0.5f;
            if (groundedPlayer) _jump_mult_mult = 1;

            current_jump_multiplier -= Time.deltaTime * _jump_mult_mult;
            if (current_jump_multiplier <= 1) current_jump_multiplier = 1;

            current_kyote_time = max_kyote_time;

            double_jumped = false;
            jumped = false;
            punched_in_air = false;
        } else
        {
            current_kyote_time -= Time.deltaTime;
        }

        if (slide_move != Vector3.zero && slide_amount > 0)
        {
            float _lerp_amount = 0.01f;
            float _divide_slide_amount = 2;
            if (slide_amount < slide_max / _divide_slide_amount) _lerp_amount = 0.01f + (1 - (slide_amount / (slide_max / _divide_slide_amount)));

            if (_moveAxis == Vector3.zero) _moveAxis = slide_move;
            else _moveAxis = Vector3.Lerp(slide_move, _moveAxis, _lerp_amount);

            _moveSpeed *= slide_increase;

            if (Can_Decrease_Slide()) slide_amount -= Time.deltaTime;


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
        if (playerInput.Standard.Jump.WasPressedThisFrame() || jump_input_storage > 0)
        {
            if (Can_Jump())
            {
                punched_in_air = false;

                jump_input_storage = 0;

                playerAnimator.Jump();
                playerVelocity.y = Mathf.Sqrt(jump_height * -2f * gravity);
                wallJump();

                if ((!groundedPlayer && current_kyote_time <= 0) || jumped == true)
                {
                    jumped = false;
                    double_jumped = true;
                    AudioManager.PlaySound(SFX.DoubleJump, 0.1f);
                }
                else
                {
                    jumped = true;
                    double_jumped = false;
                    AudioManager.PlaySound(SFX.Jump, 0.25f);

                    current_kyote_time = 0;
                }

                if (slide_amount > 0) current_jump_multiplier = slide_increase;

                StopSlide();

                current_jump_multiplier += jump_multiplier;
            } else if (jump_input_storage == 0)
            {
                jump_input_storage = max_jump_input_storage;
            }

            jump_input_storage -= Time.deltaTime;
            if (jump_input_storage < 0) jump_input_storage = 0;
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
        //Debug.Log("Hi I'm: " + id + "and I'm: " + movementState);
        characterController.Move(finalMove * Time.deltaTime);

        //characterController.Move(new Vector3(_moveAxis.x * _moveSpeed, gravity, _moveAxis.z * _moveSpeed));


        velocity = finalMove;
    }

    float _last_y_pos = 0;

    public bool Can_Jump()
    {
        if (!can_jump) return false;

        if (!groundedPlayer)
        {
            if (current_kyote_time >= 0) return true;
            else if (!can_double_jump || double_jumped) return false;
        }

        return true;
    }

    //public bool Close_Enough_To_Floor()
    //{
    //    RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector3.up, ground_distance, groundMask);

    //    if(hit.collider)
    //}

    float _distance_between_y_pos = 0;

    public bool Can_Decrease_Slide()
    {
        float _current_y_pos = transform.position.y;

        bool _can_decrease_slide = true;
        if (Mathf.Round(_current_y_pos * 5) != Mathf.Round(_last_y_pos * 10)) _can_decrease_slide = false;
 
        _last_y_pos = _current_y_pos;

        //print("slide_decrease: lsst_pos = " + Mathf.Round(_last_y_pos * 10) + ", new_pos: " + Mathf.Round(_current_y_pos * 5) + ", can_decrease_slide: " + _can_decrease_slide);

        

        if (!_can_decrease_slide)
        {
            can_decrease_slide_time = 0;
            return false;
        }

        can_decrease_slide_time += Time.deltaTime;

        if (can_decrease_slide_time < max_can_decrease_slide_time) return false;

        return true;
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

    float current_walk_time = 0;
    float max_walk_time = 0.36f;

    float _animation_last_y_pos = 0;

    public void AnimatingPlayer()
    {
        float _press_direction = playerInput.Standard.Movement.ReadValue<Vector2>().y;

        if (_press_direction > 0) current_walk_time += Time.deltaTime;
        else current_walk_time = 0;

        if (PunchHappening()) return;

        if (slide_amount > 0)
        {
            playerAnimator.Slide();
            return;
        }

        if (!groundedPlayer)
        {
            if(falling_time_to_reach_anim < max_falling_time_to_reach_anim)
            {
                falling_time_to_reach_anim += Time.deltaTime;

            }


            _distance_between_y_pos = Mathf.Abs(_animation_last_y_pos - transform.position.y);
            _animation_last_y_pos = transform.position.y;

            float _dist = Mathf.Round(_distance_between_y_pos * 500) / 500;
            //print("distance: " + _dist);

            if ((falling_time_to_reach_anim >= max_falling_time_to_reach_anim || falling_time_to_reach_anim >= 0.1f && _dist > 0.075f) && _dist > 0.05f)
            {
                if (double_jumped) playerAnimator.DoubleJump();
                else playerAnimator.Falling();
            }

            return;
        }
        else falling_time_to_reach_anim = 0;

        if (jumped || double_jumped) return;

        Vector3 _moveAxis = Get_Movement_Direction();

        if (_moveAxis != Vector3.zero)
        { 
            if(current_walk_time < max_walk_time) playerAnimator.Walk();
            else playerAnimator.Run();
        }
        else
        {
            playerAnimator.Idle();
        }
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
                    newMenuManager.PlayerDeath(perpetrator.gameObject, NewMenuManager.RoundEndCause.projectile);
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
        else if(other.gameObject.tag == "Elevator" && movementState == MovementState.Controlled)
        {
            if(newMenuManager.gameState == NewMenuManager.GameState.End || newMenuManager.gameState == NewMenuManager.GameState.Playing)
            {
                newMenuManager.ChangeState(NewMenuManager.GameState.Elevator);
            }
        }
    }

    #region replay

    //Called by the replaymanager when changing the active player
    public void SetStateControlled()
    {
        playerInput = new PlayerInput();

        

        //myCamera.transform.position = startCameraPos;
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
        //myCamera.enabled = false;
        myAudioListener.enabled = false;



        movementState = MovementState.Playback;

        //playerInput.Disable();
        playerInput.Disable();
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
            replay_dead = isDead,
            replay_animationState = playerAnimator.CurrentState
        };

        //Punch type is initiall changed from none in the Punch Function
        //Once we've saved an active punch we make sure we dont save it again
        if (punchType != PunchManager.PunchType.None)
        {
            punchType = PunchManager.PunchType.None;
        }

        return NewSnapshotInfo;
    }

    private PlayerAnimationState previousReplayState;
    public void LoadSnapshot(NewPlayerSnapshotInfo info)
    {
        //Debug.Log("I'm: " + id + " and I'm: " + (newMenuManager.runIndex - id));
        if(newMenuManager.runIndex - id < materials.Length && newMenuManager.runIndex - id < -1)
        {
            meshRenderer.material = materials[newMenuManager.runIndex - id];

        }
        else
        {
            meshRenderer.material = materials[^1];
        }


        //if (newMenuManager.runIndex - id < 10 && newMenuManager.runIndex >= 0)
        //{
         //   meshRenderer.material = materials[newMenuManager.runIndex - id];
        //}
        //else
        //{
        //    meshRenderer.material = materials[^1];
        //}

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

        if(newMenuManager.newReplayManager.m_time < 0.2f)
        {

            playerAnimator.PlayAnimation(PlayerAnimationState.Idle);
        }
        else if(info.replay_animationState != previousReplayState)
        {
            playerAnimator.PlayAnimation(info.replay_animationState);
        }


        previousReplayState = info.replay_animationState;
    }

    #endregion


    public static string NumberToWords(int number)
    {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "minus " + NumberToWords(Mathf.Abs(number));

        string words = "";

        if ((number / 1000000) > 0)
        {
            words += NumberToWords(number / 1000000) + " million ";
            number %= 1000000;
        }

        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            if (words != "")
                words += "and ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words;
    }
}
