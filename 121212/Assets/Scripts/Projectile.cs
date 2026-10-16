using UnityEngine;
using UnityEngine.Animations;

public class Projectile : Entities
{

    private NewMenuManager newMenuManager;

    private NewPlayerMovement targetPlayer;

    private Rigidbody rb;
    private MeshRenderer mr;
    private LineRenderer lr;

    [SerializeField] private float aimingTime;
    private float aimingTimer;

    [SerializeField] private float aimingSpeed;
    [SerializeField] private float firingSpeed;

    [SerializeField] Material aimingMaterial;
    [SerializeField] Material firingMaterial;


    private bool fired;

    private void Start()
    {
        newMenuManager = NewMenuManager.instance;
        targetPlayer = newMenuManager.newReplayManager.players[newMenuManager.runIndex];

        newMenuManager.newReplayManager.EndReplay += Deactivate;

        rb = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();
        lr = GetComponent<LineRenderer>();

        aimingTimer = aimingTime;

        mr.material = aimingMaterial;

        lr.positionCount = 2;
    }

    public enum ProjectileState
    {
        Aiming,
        Firing
    }

    private void Update()
    {
        transform.LookAt(targetPlayer.transform);

        Vector3[] newTargets = new Vector3[2] { transform.position, targetPlayer.transform.position };
        lr.SetPositions(newTargets);

        if (!fired)
        {
            rb.AddForce(transform.forward * aimingSpeed * Time.deltaTime, ForceMode.Impulse);

            if(aimingTimer > 0)
            {
                aimingTimer -= Time.deltaTime;
            }
            else
            {
                mr.material = firingMaterial;
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
            targetPlayer.Hit_Entity(gameObject, PunchManager.PunchType.None);
        }
        else if(other.gameObject.tag != "Enemy")
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        newMenuManager.newReplayManager.EndReplay -= Deactivate;
        gameObject.SetActive(false);
    }

    public override void Hit_Entity(GameObject perp, PunchManager.PunchType punchType)
    {
        Deactivate();
    }
}
