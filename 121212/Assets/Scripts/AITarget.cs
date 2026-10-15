using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static PlayerMovement;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class AITarget : Entities
{
    /*
    private NewReplayManager newReplayManager;
    private List<Transform> target = new List<Transform>();

    [SerializeField] private float destination_until_stop = 0.5f;

    private NavMeshAgent agent;
    public int target_position;

    [SerializeField] private float attacking_distance = 20f;
    [SerializeField] private float time_between_shots_max = 3f;
    [SerializeField] private float randomness_between_shots = 0.5f;
    [SerializeField] private float charge_time_max = 0.25f;
    public float time_between_shots = 0;
    public float charge_time = 0;

    //griff
    [SerializeField]
    private float shootDelay;
    public float shootDelayTimer = 0;
    private Vector3 delayTarget;
    private bool shooting = false;

    public bool dead;
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

    LineRenderer lineRenderer;

    private MenuManager menuManager;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject _transform = transform.parent.GetChild(0).gameObject;

        for(int i = 0; _transform.transform.childCount > i; i++)
        {
            Transform _target = _transform.transform.GetChild(i);
            target.Add(_target.transform);
            _target.gameObject.SetActive(false);
        }

        agent.Warp(new Vector3(target[0].position.x, target[0].position.y, target[0].position.z));
        target_position = 1;

        agent.updatePosition = true;

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

        //agent.updatePosition = true;
    }

    void Update()
    {

        if(dead) return;

        Vector3 _destination = target[target_position].position;

        agent.destination = _destination;

        Vector3 _destination_no_y = new Vector3(_destination.x, 0, _destination.z);
        Vector3 _transform_no_y = new Vector3(transform.position.x, 0, transform.position.z);
        float _distance_to_destination = Vector3.Distance(_transform_no_y, _destination_no_y);

        //print("Distance: " + _distance_to_destination);

        if(_distance_to_destination < destination_until_stop) {
            target_position++;
            if (target.Count <= target_position) target_position = 0;
        }

        if (shooting == true && shootDelayTimer > 0)
        {
            shootDelayTimer -= Time.deltaTime;
        }
        else if(shooting == true && shootDelayTimer <= 0)
        {
           // ShootManager.Shoot(transform, delayTarget, transform, new Color(255, 0, 0, 255), gameObject);
            shooting = false;
        }
        else
        {
            Shoot_Countdown();

        }
    }

    public Gradient LineColor;

    public void Shoot_Countdown()
    {
        if (charge_time > 0)
        {
            if(Can_See_Target() == false)
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
            direction = Vector3.Lerp(direction, _new_direction, (charge_time/charge_time_max) / ((1 - (charge_time / charge_time_max) * 5)));

            enemyTorsoRotator.SetNewTarget(save_shot_target, shootDelay);
            if (charge_time <= 0) StartShot(direction);
            return;
        }
        else lineRenderer.enabled = false;

        time_between_shots -= Time.deltaTime;

        if (time_between_shots <= 0)
        {
            Detect_A_Shot();
        }
    }

    public override void Hit_Entity(GameObject perpetrator, PunchManager.PunchType punchType)
    {
        ///Add punch behaviour here 
        if(!dead)
        {
            dead = true;
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

    public void Reset_Shot_Count()
    {
        time_between_shots = time_between_shots_max + Random.Range(-randomness_between_shots, randomness_between_shots);
    }

    Transform save_shot_target;
    public LayerMask WallLayer;
    Vector3 direction = Vector3.zero;

    public void Detect_A_Shot()
    {
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
        //RaycastHit hit;
        //Vector3 _direction = MathH.Direction_Between_Two_Vectors(transform.position, save_shot_target.position);
        ////float _distance = Vector3.Distance(transform.position, save_shot_target.position);
        //Physics.Raycast(transform.position, _direction, out hit, attacking_distance, LayerMask.NameToLayer("Wall"));
        

        Debug.DrawLine(transform.position, save_shot_target.position, Color.purple, attacking_distance);

        if (Physics.Linecast(transform.position, save_shot_target.position, WallLayer)) return false;
        return true;
    }

    public void StartShot(Vector3 _targetDirection)
    {
        //Debug.Log("saving target");
        //Debug.Log("target id is: " + _target.gameObject.GetComponent<PlayerMovement>().id);
        //Debug.Log("target pos is: " + _target.position);
        direction = MathH.Direction_Between_Two_Vectors(transform.position, save_shot_target.position);
        shootDelayTimer = shootDelay;
        delayTarget = _targetDirection;
        shooting = true;

        lineRenderer.enabled = false;

        AudioManager.PlaySound(SFX.Shot, 0.1f);
        ShootManager.Shoot(transform, direction, transform, new Color(255, 0, 0, 255), gameObject, (newMenuManager.roundState != NewMenuManager.RoundState.DeathUI));

        //ShootManager.Shoot(transform, direction, transform, new Color(255, 0, 0, 255), gameObject);

    }

    public NewSnapshotInfoEnemy SaveSnapshot()
    {

        NewSnapshotInfoEnemy NewSnapshotInfo = new NewSnapshotInfoEnemy()
        {
            replay_id = id,
            replay_dead = dead
        };

        return NewSnapshotInfo;
    }*/
    
    public override void Hit_Entity(GameObject perpetrator, PunchManager.PunchType punchType)
    {
        throw new System.NotImplementedException();
    }
}
