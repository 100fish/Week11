using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PunchManager;

public class Enemy : Entities
{
    private NewReplayManager newReplayManager;

    private NewMenuManager newMenuManager;

    [SerializeField] private Transform barrel;

    [SerializeField] public Rigidbody enemyRigidBody;

    #region Shot Setup
    [SerializeField] private float attacking_distance = 20f;
    [SerializeField] private float time_between_shots_max = 3f;
    [SerializeField] private float randomness_between_shots = 0.5f;
    [SerializeField] private float charge_time_max = 0.25f;
    public float time_between_shots = 0;
    public float charge_time = 0;


    [SerializeField] private float shootDelay;
    public float shootDelayTimer = 0;
    private Vector3 delayTarget;
    private bool shooting = false;
    #endregion

    public enum EnemyReplayState
    {
        Active,
        Playback,
    }
    public enum EnemyState
    {
        Alive,
        Dead
    }

    public EnemyReplayState enemyReplayState = EnemyReplayState.Active;
    public EnemyState enemyState = EnemyState.Alive;

    public bool recording;
    public int id;

    [SerializeField]
    public Material aliveMaterial;

    [SerializeField]
    public Material deadMaterial;

    [SerializeField]
    private Color unchargedColour;
    [SerializeField]
    private Color chargedColour;

    [SerializeField]
    public MeshRenderer[] meshRenderers;
    [SerializeField]
    private Renderer[] renderers;

    [SerializeField]
    private EnemyTorsoRotator enemyTorsoRotator;

    private LineRenderer lineRenderer;

    void Awake()
    {
        Reset_Shot_Count();

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
    }

    public void Start()
    {
        // agent.updatePosition = false;
        newMenuManager = NewMenuManager.instance;

        newReplayManager = NewReplayManager.instance;
        newReplayManager.RegisterEnemy(this);


        renderers = new Renderer[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            renderers[i] = meshRenderers[i].gameObject.GetComponent<Renderer>();
        }

        shootTimer = shootTime;

        //agent.updatePosition = true;
    }

    void Update()
    {
        if (enemyState == EnemyState.Dead || newMenuManager.roundState != NewMenuManager.RoundState.Gameplay) return;

        if(shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }
        else
        {
            shootTimer = UnityEngine.Random.Range(shootTime-.5f, shootTime+.5f);
            Instantiate(projectile, projectileSpawn.position, Quaternion.identity);
        }

        /*if (shooting == true && shootDelayTimer > 0)
        {
            shootDelayTimer -= Time.deltaTime;
        }
        else if (shooting == true && shootDelayTimer <= 0)
        {
            ///Shoot here once that works
            shooting = false;
        }
        else
        {
            Shoot_Countdown();
        }*/
    }

    public Gradient LineColor;

    public void Shoot_Countdown()
    {
        if (charge_time > 0)
        {
            if (Can_See_Target() == false)
            {
                charge_time = 0;
                return;
            }

            charge_time -= Time.deltaTime;
            lineRenderer.enabled = true;
            lineRenderer.SetPositions(new Vector3[2] { transform.position, save_shot_target.position });

            float _speed_of_line = (Mathf.RoundToInt((1 - (charge_time / charge_time_max)) * 6)) / 2 + 1f;
            lineRenderer.material.SetFloat("_time", _speed_of_line);
            //print("speed of line: " + _speed_of_line);
            lineRenderer.material.SetColor("_LineColor", LineColor.Evaluate((1 - (charge_time / charge_time_max))));
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = Color.Lerp(chargedColour, unchargedColour, charge_time / charge_time_max);
            }

            Vector3 _new_direction = MathH.Direction_Between_Two_Vectors(transform.position, save_shot_target.position);
            direction = Vector3.Lerp(direction, _new_direction, (charge_time / charge_time_max) / ((1 - (charge_time / charge_time_max) * 5)));

            enemyTorsoRotator.SetNewTarget(save_shot_target, shootDelay);
            if (charge_time <= 0)
            {
                ///I've turned this off for now, I think a lot of this behaviour will have to move to the energy orb
                ///The enemy should just spawn them on a set duration while looking at a player
                //StartShot(direction);
            }
            return;
        }
        else lineRenderer.enabled = false;

        time_between_shots -= Time.deltaTime;

        if (time_between_shots <= 0)
        {
            ///I've turned this off for now, I think a lot of this behaviour will have to move to the energy orb
            ///The enemy should just spawn them on a set duration while looking at a player
            //Detect_A_Shot();
        }
    }

    public PunchManager.PunchType punched = PunchManager.PunchType.None;

    public override void Hit_Entity(GameObject perpetrator, PunchManager.PunchType punchType)
    {
        ///Add punch behaviour here 
        ///Explosions and physics
        ///Lives
        ///

        enemyReplayState = EnemyReplayState.Active;

        if (enemyState != EnemyState.Dead)
        {
            enemyState = EnemyState.Dead;

            punched = PunchManager.PunchType.Left;
            lineRenderer.enabled = false;

            SaveSnapshot();
            NewMenuManager newMenuManager = NewMenuManager.instance;

            newMenuManager.EnemyDeath();

            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.material = deadMaterial;
            }
        }
    }

    public void ReviveEnemy()
    {
        enemyState = EnemyState.Alive;

        punched = PunchManager.PunchType.None;
        lineRenderer.enabled = true;

        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.material = aliveMaterial;
        }
    }

    public void Reset_Shot_Count()
    {
        time_between_shots = time_between_shots_max + UnityEngine.Random.Range(-randomness_between_shots, randomness_between_shots);
    }

    Transform save_shot_target;
    public LayerMask WallLayer;
    Vector3 direction = Vector3.zero;

    public void Detect_A_Shot()
    {
        ///Defunct for now
        Transform _target = TargetManager.Get_Active_Target(transform, attacking_distance, "Ball");

        if (_target == null) return;

        Reset_Shot_Count();
        save_shot_target = _target;
        charge_time = charge_time_max;

        if (Can_See_Target() == false)
        {
            charge_time = 0;
            return;
        }

        AudioManager.PlaySound(SFX.LockOn, 0.2f);
    }

    public bool Can_See_Target()
    {
        Debug.DrawLine(transform.position, save_shot_target.position, Color.purple, attacking_distance);

        if (Physics.Linecast(transform.position, save_shot_target.position, WallLayer)) return false;
        return true;
    }

    public void StartShot(Vector3 _targetDirection)
    {
        direction = MathH.Direction_Between_Two_Vectors(transform.position, save_shot_target.position);
        shootDelayTimer = shootDelay;
        delayTarget = _targetDirection;
        shooting = true;

        lineRenderer.enabled = false;

        AudioManager.PlaySound(SFX.Shot, 0.1f);
        //ShootManager.Shoot(transform, direction, transform, new Color(255, 0, 0, 255), gameObject, (newMenuManager.roundState != NewMenuManager.RoundState.DeathUI));

    }

    public NewSnapshotInfoEnemy SaveSnapshot()
    {

        NewSnapshotInfoEnemy NewSnapshotInfo = new NewSnapshotInfoEnemy()
        {
            replay_id = id,
            replay_position = transform.position,
            replay_linearVelocity = enemyRigidBody.linearVelocity,
            replay_rotation = transform.rotation,
            replay_angularVelocity = enemyRigidBody.angularVelocity,
            replay_wasPunched = punched
        };

        //if (punched != PunchManager.PunchType.None)
            //punched = PunchManager.PunchType.None;

        return NewSnapshotInfo;
    }
    public void LoadSnapshot(NewSnapshotInfoEnemy info)
    {
        transform.position = info.replay_position;
        enemyRigidBody.linearVelocity = info.replay_linearVelocity;
        transform.rotation = info.replay_rotation;
        enemyRigidBody.angularVelocity = info.replay_angularVelocity;
        

        if (info.replay_wasPunched != PunchManager.PunchType.None)
        {
            Debug.Log("Killing enemy while loading -----------------------  ------------------------------------------------------- ----------------");
            Hit_Entity(gameObject, info.replay_wasPunched);
        }
    }

    private List<Transform> myPlayerList = new List<Transform>();
    private float closestPlayerDistance;
    private Transform closestPlayer;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            enemyReplayState = EnemyReplayState.Active;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            myPlayerList.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            myPlayerList.Remove(other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player" && enemyState != EnemyState.Dead)
        {
            if (closestPlayer == null)
            {
                closestPlayer = other.transform;
            }
            else
            {

                float newPlayerDistance;
                newPlayerDistance = Vector3.Distance(other.transform.position, transform.position);
                closestPlayerDistance = Vector3.Distance(closestPlayer.position, transform.position);

                if (newPlayerDistance < closestPlayerDistance)
                {
                    closestPlayerDistance = newPlayerDistance;
                    closestPlayer = other.transform;
                }
            }
            barrel.transform.LookAt(closestPlayer);
        }
    }

    [SerializeField] private Transform projectileSpawn;

    [SerializeField] private GameObject projectile;

    private float shootTimer;
    [SerializeField] private float shootTime;


}