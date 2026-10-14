using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

public class PlayerMovement : Entities
{
    [SerializeField]
    private MeshRenderer gunRenderer;

    [SerializeField]
    private PlayerParticles playerParticles;
    private CharacterController characterController;
    private MenuManager menuManager;

    [SerializeField]
    private Transform gunTransform;
    [SerializeField]
    private Transform gunPoint;
    [SerializeField]
    private Color[] shootColors;
    [SerializeField]
    private float shootFadeOutSpeed;

    [SerializeField]
    public Camera myCamera;
    [SerializeField]
    public AudioListener myAudioListener;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    public float lookSpeed;
    [SerializeField]
    private float gravity;

    [SerializeField] private Animator gun_animator;
    [SerializeField] private Animator camera_shake;

    public struct ShootInfo
    {
        public Transform s_cameraTransform;
        public Transform s_gunPointTransform;
        public Color32 s_shootColour;
        public bool s_didShoot;

        public ShootInfo(Transform cameraTransform, Transform gunPointTransform, Color32 shootColour, bool didShoot)
        {
            s_cameraTransform = cameraTransform;
            s_gunPointTransform = gunPointTransform;
            s_shootColour = shootColour;
            s_didShoot = didShoot;
        }
    }

    ShootInfo shootInfo;
    private int shootIndex = 0;
    private int previoushootIndex = -1;

    public enum MovementState
    {
        Inactive,
        Controlled,
        Playback
    }

    public MovementState movementState = MovementState.Inactive;

    public int id;

    public float ammo { get; private set; } = 10;
    public bool Is_Ghost()
    {
        if (movementState == MovementState.Controlled) return false;
        else return true;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        menuManager = MenuManager.instance;

        TargetManager.Add_Player_To_Scene(transform);

        playerVelocity = Vector3.zero;
        current_jump_multiplier = 1;
    }

    #region Inputsetup
    private PlayerInput playerInput;
    private void Awake()
    {
        myCamera = GetComponentInChildren<Camera>();

        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        //input setup moved to SetStateControlled & SetStatePlayback
    }

    private void OnDisable()
    {
        //input setup moved to SetStateControlled & SetStatePlayback
        SetStatePlayback(); 
    }
    #endregion Inputsetup

    private void Shoot(InputAction.CallbackContext context)
    {
        if(ammo > 0 && (menuManager.roundState == MenuManager.RoundState.Gameplay || menuManager.roundState == MenuManager.RoundState.Tutorial))
        {
            if(menuManager.roundState == MenuManager.RoundState.Gameplay)
            {
                ammo--;
            }

            ShootManager.Shoot(myCamera.transform, myCamera.transform.forward, gunPoint, new Color32(54, 23, 244, 255), gameObject, (menuManager.roundState != MenuManager.RoundState.DeathUI));
            shootInfo = new(myCamera.transform, gunPoint, new Color32(54, 23, 244, 255), true);
            shootIndex++;

            StartUI.Move_Through_UI(3);

            gun_animator.SetTrigger("Shoot");
            camera_shake.SetTrigger("Screenshake");

            AudioManager.PlaySound(SFX.GunShot, 0.2f);
            
            gun_animator.SetTrigger("Shoot");
            CameraShake();
 
        }

    }

    public void CameraShake()
    {
        camera_shake.SetTrigger("Screenshake");
    }

    //float current_roll = 0;
    //float roll_count = 10;
    //float randomness_divide = 7;

    //private void ScreenShake()
    //{
        
    //}

    //private void SetCurrentRoll()
    //{
    //    current_roll = 
    //}

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

    public Transform gun_transform;
    bool zoom_in = false;
    float base_FOV = 60, zoomed_FOV = 30, zoom_in_current = 0, time_mult = 10;
    float base_gun_pos = 0.0f, new_gun_pos = 0.6f, current_gun_pos = 0.0f;

    private void Start_ZoomIn(InputAction.CallbackContext contex)
    {
        zoom_in = true;
        StartUI.Move_Through_UI(4);
    }
    private void Stop_ZoomIn(InputAction.CallbackContext contex) => zoom_in = false;

    private void Slide(InputAction.CallbackContext contex) => Slide();
    private void Slide()
    {
        StartUI.Move_Through_UI(2);

        if (!groundedPlayer) { slide_memory = slide_memory_max; return; }
        if (slide_delaying) return;

        AudioManager.PlaySound(SFX.SlideRoll, 0.2f);

        slide_amount = slide_max;
        slide_move = Get_Movement_Direction(true);
        myCamera.transform.localPosition = new Vector3(0, 0.1f, 0);
        slide_bump = 2;
    }

    private void StopSlide(InputAction.CallbackContext contex) => StopSlide();
    private void StopSlide()
    {

        AudioManager.PlaySound(SFX.SlideRoll, 0.2f, false, true);
        if (slide_amount <= 0) return;
        
        //AudioManager.PlaySound(SFX.SlideCymbol, 0.2f);

        slide_amount = 0;
        myCamera.transform.localPosition = new Vector3(0, 0.65f, 0);
        slide_delay = slide_delay_max;
        slide_delaying = true;
    }

   private bool double_jumped = false;

    [SerializeField] private float jump_height = 1;
    [SerializeField] private float jump_multiplier = 0.3f;
    private float current_jump_multiplier = 1;
    private bool groundedPlayer;

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            gun_transform.gameObject.SetActive(false);
        }
        else
        {
            gun_transform.gameObject.SetActive(true);
        }

        Debug.DrawRay(gunPoint.position, gunPoint.forward, Color.green);

        //move character
        MovePlayer();
        //rotate character
        RotateCamera();
        transform.Rotate(new Vector3(0, playerInput.Standard.MouseLook.ReadValue<Vector2>().x, 0) * lookSpeed);

        if (zoom_in) zoom_in_current += Time.deltaTime * time_mult;
        else zoom_in_current -= Time.deltaTime * time_mult;

        zoom_in_current = Mathf.Clamp01(zoom_in_current);

        myCamera.fieldOfView = Mathf.Lerp(base_FOV, zoomed_FOV, zoom_in_current);
        float _gun_transform_z = Mathf.Lerp(base_gun_pos, new_gun_pos, zoom_in_current);
        gun_transform.localPosition = new Vector3(0, 0, _gun_transform_z);
    }

    float ClampAngle(float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }

    float camera_max = 80;

    void RotateCamera()
    {
        Vector3 rot = myCamera.transform.localRotation.eulerAngles + new Vector3(-playerInput.Standard.MouseLook.ReadValue<Vector2>().y * lookSpeed, 0f, 0f); //use local if your char is not always oriented Vector3.up
        rot.x = ClampAngle(rot.x, -camera_max, camera_max);

        myCamera.transform.localEulerAngles = rot;
    }

    public Vector3 Get_Movement_Direction(bool _forward_default = false)
    {
        Vector3 _direction = (transform.forward * playerInput.Standard.Movement.ReadValue<Vector2>().y) + (transform.right * playerInput.Standard.Movement.ReadValue<Vector2>().x);
        if(_forward_default && _direction == Vector3.zero) _direction = transform.forward;
        return _direction;
    }

    Vector3 playerVelocity = Vector3.zero;

    public void MovePlayer()
    {
        //if (transform.position.y <= -15) Hit_Entity();
        Vector3 _moveAxis = Get_Movement_Direction();
        float _moveSpeed = moveSpeed;

        groundedPlayer = characterController.isGrounded;

        if(groundedPlayer)
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

        if(slide_delaying)
        {
            slide_delay -= Time.deltaTime;
            if(slide_delay <= 0) slide_delaying = false;
        }

        if(slide_memory > 0)
        {
            slide_memory-= Time.deltaTime;
            if (groundedPlayer) Slide();
         }

        // Jump using WasPressedThisFrame()
        if (playerInput.Standard.Jump.WasPressedThisFrame() && (groundedPlayer || !double_jumped))
        {
            

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
        //print("finalMove: " + finalMove + ", moveAxis: " + _moveAxis + ", _moveSpeed" + _moveSpeed + ", _moveAxis: " + playerVelocity.y);
        characterController.Move(finalMove * Time.deltaTime);

        //characterController.Move(new Vector3(_moveAxis.x * _moveSpeed, gravity, _moveAxis.z * _moveSpeed));
    }

    int wall_jump_dir = 1;

    public void wallJump()
    {
        RaycastHit Lhit;
        RaycastHit Rhit;

        Physics.Raycast(transform.position, transform.right, out Lhit, 1);
        Physics.Raycast(transform.position, -transform.right, out Rhit, 1);

        if(Lhit.collider != null)
        {
            playerVelocity.z = 10f;
            playerVelocity.y += 2.5f;
            wall_jump_dir = 1;
        } else if (Rhit.collider != null) 
        {
            playerVelocity.z = 10f;
            playerVelocity.y += 2.5f;
            wall_jump_dir = -1;
        }
    }

    public override void Hit_Entity(GameObject perpetrator)
    {
        if(movementState == MovementState.Controlled && menuManager.roundState == MenuManager.RoundState.Gameplay)
        {
            CameraShake();

            menuManager.PlayerDeath(perpetrator);

        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Kill" && movementState == MovementState.Controlled && menuManager.roundState == MenuManager.RoundState.Gameplay)
        {
            menuManager.PlayerDeath(gameObject);
        }
    }

    #region replay

    public void SetStateControlled()
    {
        playerInput = new PlayerInput();

        myCamera.enabled = true;
        myAudioListener.enabled = true;

        movementState = MovementState.Controlled;

        playerInput.Enable();
        playerInput.Standard.Shoot.started += Shoot;

        playerInput.Standard.Slide.started += Slide;
        playerInput.Standard.Slide.canceled += StopSlide;

        playerInput.Standard.ZoomIn.started += Start_ZoomIn;
        playerInput.Standard.ZoomIn.canceled += Stop_ZoomIn;

        playerInput.Standard.ESC.started += Exit;

        playerParticles.DisableParticles();
    }

    public void SetStatePlayback()
    {
        myCamera.enabled = false;
        myAudioListener.enabled = false;

        movementState = MovementState.Playback;

        //playerInput.Disable();
        playerInput.Standard.Disable();
        playerInput.Standard.Shoot.started -= Shoot;

        playerInput.Standard.Slide.started -= Slide;
        playerInput.Standard.Slide.canceled -= StopSlide;

        playerInput.Standard.ZoomIn.started -= Start_ZoomIn;
        playerInput.Standard.ZoomIn.canceled -= Stop_ZoomIn;

        playerInput.Standard.ESC.started -= Exit;

        playerParticles.EnableParticles();

    }

    private void Exit(InputAction.CallbackContext context) => Application.Quit();

    public NewSnapshotInfo SaveSnapshot()
    {

        NewSnapshotInfo NewSnapshotInfo = new NewSnapshotInfo()
        {
            replay_id = id,
            replay_position = transform.position,
            replay_rotation = transform.rotation,
            replay_shootInfo = shootInfo
        };

        if(shootInfo.s_didShoot == true)
        {
            shootInfo.s_didShoot = false;   
        }

        return NewSnapshotInfo;
    }

    public void LoadSnapshot(NewSnapshotInfo info)
    {
        transform.position = info.replay_position;

        //if its at the default position get it out of the fucking way
        if (transform.position == Vector3.zero)
        {
            transform.position = new Vector3(0, -1000, 0);
        }

        transform.rotation = info.replay_rotation;
        if(info.replay_shootInfo.s_didShoot == true && info.replay_shootInfo.s_cameraTransform != null)
        {
            ShootManager.Shoot(info.replay_shootInfo.s_cameraTransform,
                               info.replay_shootInfo.s_cameraTransform.forward,
                               info.replay_shootInfo.s_gunPointTransform,
                               info.replay_shootInfo.s_shootColour,
                               gameObject,
                               (menuManager.roundState != MenuManager.RoundState.DeathUI));
        }
    }

    #endregion

}
