using UnityEngine;

public class DummyTurret : MonoBehaviour
{
    [SerializeField] private GameObject d_barrel;
    [SerializeField] private Rigidbody d_barrelrb;
    [SerializeField] private MeshRenderer d_barrelmr;
    [SerializeField] private GameObject d_base;
    [SerializeField] private Rigidbody d_baserb;
    [SerializeField] private MeshRenderer d_basemr;

    [SerializeField] private ParticleSystem[] particleSystems;

    private void Start()
    {
        DectivateDummy();
    }

    public void ActivateDummy(float flyStrength, Vector3 punchPosition, Vector3 barrelPos, Vector3 basePos, Quaternion barrelRot, Quaternion baseRot)
    {
        d_barrel.SetActive(true);
        d_base.SetActive(true);

        d_barrel.transform.position = barrelPos;
        d_barrel.transform.rotation = barrelRot;
        d_base.transform.position = basePos;
        d_base.transform.rotation = baseRot;

        d_barrelrb.AddForce((d_barrel.transform.position - punchPosition).normalized * flyStrength, ForceMode.Impulse);
        d_baserb.AddForce((d_base.transform.position - punchPosition).normalized * flyStrength, ForceMode.Impulse);

        d_barrelmr.enabled = true;
        d_basemr.enabled = true;

        foreach (var particleSystem in particleSystems)
        {
            particleSystem.Play();
        }
    }
    public void DectivateDummy()
    {
       // StartUI.Punch_Hit_Enemy();

        d_barrelmr.enabled = false;
        d_basemr.enabled = false;

        d_barrel.SetActive(false);
        d_base.SetActive(false);
    }
}
