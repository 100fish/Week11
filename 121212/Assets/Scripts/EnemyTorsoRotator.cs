using UnityEngine;

public class EnemyTorsoRotator : MonoBehaviour
{
    [SerializeField]
    public float rotateTime;
    private float rotateTimer;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    private Quaternion newRotation;

    private void Update()
    {
        if (rotateTimer > 0)
        {
            rotateTimer -= Time.deltaTime;
            newRotation = Quaternion.Lerp(targetRotation, startRotation, rotateTimer / rotateTime);

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, newRotation.eulerAngles.y, transform.eulerAngles.z);
        }

    }

    public void SetNewTarget(Transform target, float shootDelayTime)
    {
        startRotation = transform.rotation; 
        targetRotation = Quaternion.LookRotation((target.position - transform.position).normalized, Vector3.up);


        rotateTime = shootDelayTime;
        rotateTimer = rotateTime;
    }

}
