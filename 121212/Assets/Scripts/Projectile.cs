using UnityEngine;
using UnityEngine.Animations;

public class Projectile : Entities
{

    private NewMenuManager newMenuManager;

    private NewPlayerMovement targetPlayer;

    private Rigidbody rb;
    private SphereCollider sc;
    private MeshRenderer mr;
    private LineRenderer lr;
    private ParticleSystem ps;

    [SerializeField] private float aimingTime;
    private float aimingTimer;

    [SerializeField] private float aimingSpeed;
    [SerializeField] private float firingSpeed;
    [SerializeField] private Vector3 targetVector;

    [SerializeField] Material aimingMaterial;
    [SerializeField] Material firingMaterial;
    [SerializeField] Material hittableMaterial;

    [SerializeField] Gradient inactiveGradient;
    [SerializeField] Gradient activeGradient;


    private bool fired;

    private void Awake()
    {
        myEntityType = EntityType.Projectile;
    }

    protected override void Start()
    {
        base.Start();

        newMenuManager = NewMenuManager.instance;
        targetPlayer = newMenuManager.newReplayManager.players[newMenuManager.runIndex];

        newMenuManager.newReplayManager.EndReplay += Deactivate;

        rb = GetComponent<Rigidbody>();
        sc = GetComponent<SphereCollider>();
        mr = GetComponent<MeshRenderer>();
        lr = GetComponent<LineRenderer>();
        ps = GetComponent<ParticleSystem>();

        aimingTimer = aimingTime;

        mr.material = aimingMaterial;
        previousMaterial = aimingMaterial;


        lr.positionCount = 2;

        lr.colorGradient = inactiveGradient;
    }

    public enum ProjectileState
    {
        Aiming,
        Firing
    }

    Material previousMaterial;

    private void Update()
    {
        if(!fired)
        {
            transform.LookAt(targetPlayer.transform);
        }

        /*
        if(Vector3.Distance(transform.position, targetPlayer.transform.position) < targetPlayer.punchRange)
        {
            previousMaterial = mr.material;
            mr.material = hittableMaterial;
        }
        else
        {
            mr.material = previousMaterial;
        }*/


        //Vector3[] newTargets = new Vector3[2] { transform.position, targetPlayer.transform.position };
        //lr.SetPositions(newTargets);

        if (!fired)
        {
            rb.AddForce(transform.forward * aimingSpeed * Time.deltaTime, ForceMode.Impulse);

            if(aimingTimer > 0)
            {
                aimingTimer -= Time.deltaTime;
            }
            else
            {
                //lr.colorGradient = activeGradient;

                previousMaterial = firingMaterial;
                mr.material = firingMaterial;
                //ps.Play();

                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z * 1.5f);

                fired = true;
            }

        }
        else
        {
            rb.AddForce(transform.forward * firingSpeed * Time.deltaTime, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetPlayer.gameObject)
        {
            if(targetPlayer.PunchHappening())
            {
                Deactivate();
            }
            else
            {
                targetPlayer.Hit_Entity(this, PunchManager.PunchType.None);
            }
        }
        else if(other.gameObject.tag != "Enemy" && other.gameObject.tag != "Player")
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        newMenuManager.newReplayManager.EndReplay -= Deactivate;
        mr.enabled = false;
        sc.enabled = false;
        //ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public override void Hit_Entity(Entities perp, PunchManager.PunchType punchType)
    {
        Deactivate();
    }
}
