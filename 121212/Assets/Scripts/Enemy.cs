using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using static PunchManager;

public class Enemy : Entities
{
    private NewReplayManager newReplayManager;

    private NewMenuManager newMenuManager;

    [SerializeField] public bool startHealth;
    public bool health;

    [SerializeField] private Transform barrel;

    [SerializeField] public Rigidbody enemyRigidBody;

    // We make a list of each punch that the thing records
    //and save the timestamp of each punch

    //then when recording, if we pass that timestamp we run the punch

    ////HONEY FLASH ! ! ! ! !////
   
    //X IS PUNCHTYPE Y IS PUNCHTIME
    private List<Vector3> myScuffedPunches = new List<Vector3>();

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

    [SerializeField] Material[] materials;

    private LineRenderer lineRenderer;
    private SphereCollider sphereCollider;

    [SerializeField] float idleRotateSpeed;
    [SerializeField] Animator animator;

    protected void Awake()
    {
        myEntityType = EntityType.Enemy;

        health = startHealth;

        Reset_Shot_Count();

        lineRenderer = GetComponent<LineRenderer>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = attacking_distance;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
    }

    protected override void Start()
    {
        base.Start();

        // agent.updatePosition = false;
        newMenuManager = NewMenuManager.instance;

        newReplayManager = NewReplayManager.instance;
        //newReplayManager.RegisterEnemy(this);


        renderers = new Renderer[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            renderers[i] = meshRenderers[i].gameObject.GetComponent<Renderer>();
        }

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if(health)
            {
                meshRenderers[i].material = materials[1];
            }
            else
            {
                meshRenderers[i].material = materials[0];
            }
        }

        shootTimer = shootTime;

        //agent.updatePosition = true;
    }

    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material activeMaterial;

    private void SetMaterial(Material newMaterial)
    {
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.material = newMaterial;
        }
    }
    
    void Update()
    {
        //SetMaterial(idleMaterial);

        if (myPlayerList.Count <= 0)
        {
            //SetMaterial(activeMaterial);
            barrel.transform.Rotate(0, idleRotateSpeed * Time.deltaTime, 0);
        }

        //Debug.Log("Enemy update:" + id);

        if (enemyState == EnemyState.Dead || newMenuManager.roundState != NewMenuManager.RoundState.Gameplay) return;

        float _enemy_distance = Vector3.Distance(transform.position, newMenuManager.newReplayManager.players[newMenuManager.runIndex].transform.position);
        //print("enemy distane: " + _enemy_distance);
        if (_enemy_distance < 9) return;

        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }
        else
        {
            //Debug.Log("Enemy shootTimer:" + id);
            //Debug.Log("Enemy playerList:" + myPlayerList.Count);
            if (myPlayerList.Count > 0)
            {
                //.Log("Runindex: " + newMenuManager.runIndex);
                //Debug.Log("Roundstate: " + newMenuManager.roundState);
                if (myPlayerList.Contains(newMenuManager.newReplayManager.players[newMenuManager.runIndex].transform))
                {
                    //Debug.Log("Enemy SHOOTCHECK:" + id);

                    shootTimer = UnityEngine.Random.Range(shootTime - .5f, shootTime + .5f);
                    //Instantiate(projectile, projectileSpawn.position, Quaternion.identity);
                    animator.SetTrigger("Shoot");
                }
            }
        }

    }

    public float Firerate_NoFireDist;

    public void Fire_Bullet()
    {
        //print("fire! 3");
        Projectile newProjectile = Instantiate(projectile, projectileSpawn.position, Quaternion.identity).GetComponent<Projectile>();
        newProjectile.myEnemy = this;
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

    [SerializeField] private float pushStrength;
    [SerializeField] private float flyStrength;

    public PunchManager.PunchType punched = PunchManager.PunchType.None;

    [SerializeField] private DummyTurret dummyTurret;
    [SerializeField] private float dummyFlyStrength;

    bool replayHit = false;

    public override void Hit_Entity(Entities perpetrator, PunchManager.PunchType punchType)
    {
        Debug.Log(health);
        if(!replayHit) enemyReplayState = EnemyReplayState.Active;

        if (enemyState != EnemyState.Dead)
        {
            if(health)
            {
                if(punchType == PunchType.Left)
                {
                    //enemyRigidBody.AddForce((transform.position - perpetrator.transform.position).normalized * pushStrength, ForceMode.Impulse);
                    if (!replayHit) punched = PunchManager.PunchType.Left;
                    if(!replayHit)SaveSnapshot(newReplayManager.m_time);
                    Debug.Log("I got bunched by a beft");
                }
                else if(punchType == PunchType.Right)
                {

                    for (int i = 0; i < meshRenderers.Length; i++)
                    {
                        meshRenderers[i].material = materials[0];
                    }

                    
                    enemyRigidBody.AddForce(transform.up * (flyStrength /4), ForceMode.Impulse);
                    enemyRigidBody.useGravity = false;
                    if (!replayHit) punched = PunchManager.PunchType.Right;
                    if (!replayHit) SaveSnapshot(newReplayManager.m_time);
                    health = false;
                    Debug.Log("I got bunched by a bight");
                }


            }
            else
            {
                Debug.Log("I fucking died");

                StartUI.Punch_Hit_Enemy();
                AudioManager.PlaySound(SFX.Punch_Hit);

                enemyState = EnemyState.Dead;

                enemyRigidBody.useGravity = true;

                dummyTurret.ActivateDummy(dummyFlyStrength, perpetrator.transform.position, barrel.position, transform.position, barrel.rotation, transform.rotation);

                punched = PunchManager.PunchType.Left;
                lineRenderer.enabled = false;

                if (!replayHit) SaveSnapshot(newReplayManager.m_time);
                NewMenuManager newMenuManager = NewMenuManager.instance;

                newMenuManager.EnemyDeath();

                foreach (var meshRenderer in meshRenderers)
                {
                    meshRenderer.enabled = false;
                }
            }
        }
        replayHit = false;
    }

    public void ReviveEnemy()
    {
        Debug.Log("Hi I'm getting revived: " + id);
        enemyState = EnemyState.Alive;

        health = startHealth;

        enemyRigidBody.useGravity = true;

        punched = PunchManager.PunchType.None;
        lineRenderer.enabled = true;

        dummyTurret.DectivateDummy();

        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = true;

            if(health)
            {

                meshRenderer.material = materials[1];
            }
            else
            {

                meshRenderer.material = materials[0];
            }
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

    public NewSnapshotInfoEnemy SaveSnapshot(float snapShotTime)
    {

        NewSnapshotInfoEnemy NewSnapshotInfo = new NewSnapshotInfoEnemy()
        {
            replay_id = id,
            replay_position = transform.position,
            replay_linearVelocity = enemyRigidBody.linearVelocity,
            replay_rotation = transform.rotation,
            replay_angularVelocity = enemyRigidBody.angularVelocity,
            replay_wasPunched = punched,
            replay_health = health
        };

        if (punched != PunchManager.PunchType.None && startHealth == true)
        {
            
            myScuffedPunches.Add(new Vector2((float)punched, snapShotTime));

            Debug.Log("/////////State: " + enemyReplayState);
            Debug.Log("New member!: here's everyone: ");
            for (int i = 0; i < myScuffedPunches.Count; i++)
            {
                Debug.Log(myScuffedPunches[i]);
                Debug.Log(health);
            }

            punched = PunchManager.PunchType.None;
        }


        return NewSnapshotInfo;
    }
    public void LoadSnapshot(NewSnapshotInfoEnemy info, float snapshotTime)
    {
        transform.position = info.replay_position;
        enemyRigidBody.linearVelocity = info.replay_linearVelocity;
        transform.rotation = info.replay_rotation;
        enemyRigidBody.angularVelocity = info.replay_angularVelocity;
        health = info.replay_health;


        //Debug.Log("I'm: " + id + " and I'm now loading: " + info.replay_wasPunched + "at " + newReplayManager.snapshotIndex);

        if(startHealth == true)
        {
            for (int i = 0; i < myScuffedPunches.Count; i++)
            {
                Debug.Log("///State: " + enemyReplayState);
                Debug.Log("Loading%Checking: " + myScuffedPunches[i]);
                Debug.Log("against: " + snapshotTime);
                Debug.Log("health: " + health);

                if (myScuffedPunches[i].y < snapshotTime)
                {
                    Debug.Log(myScuffedPunches[i] + "SUCCEEEEEDSSSS!!! OOOOOOO");
                    replayHit = true;

                    if(i == 0 && (PunchType)myScuffedPunches[i].x == PunchType.Right)
                    {
                        health = true;
                    }
                    Hit_Entity(this, (PunchType)myScuffedPunches[i].x);
                    myScuffedPunches.Remove(myScuffedPunches[i]);
                }
                else
                {
                    //Debug.Log(myScuffedPunches[i] + "FAILS!!! XXX");

                }
            }
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