using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ShootManager : MonoBehaviour
{
    List<Shot> GunLines = new List<Shot>();
    int shot_point = 0;

    private MenuManager menuManager;

    static ShootManager Instance;

    public void Start()
    {
        Instance = this;

        menuManager = MenuManager.instance;

        for (int i = 0; i < transform.childCount; i++)
        {
            GunLines.Add(transform.GetChild(i).GetComponent<Shot>());
        }
    }

    public static void Shoot(Transform _start, Vector3 _direction, Transform _gun_point, Color32 _bullet_color, GameObject shooter, bool canShoot )
    {
        if(canShoot)
        {
            RaycastHit hit;
            RaycastHit gunHit;

            Physics.Raycast(_start.position, _direction, out hit, 100);
            Ray r = new Ray(origin: _gun_point.position, direction: _direction);

            float _hit_distance = hit.distance;
            if (_hit_distance <= 0) _hit_distance = 100;

            Instance.GunLines[Instance.shot_point].Shoot(r.GetPoint(0), hit.point, _bullet_color);
            Instance.shot_point++;
            if (Instance.shot_point >= Instance.GunLines.Count) Instance.shot_point = 0;

            Physics.Linecast(_gun_point.position, hit.point, out gunHit);

            //checks hit on the first raycast. If it collides with nothing then it does 
            //the same with the gun hit to account for both the camera and gun pos
            if (!Hit_Check(hit, shooter))
                Hit_Check(gunHit, shooter);
        }
    }

    public static bool Hit_Check(RaycastHit hit, GameObject shooter)
    {
        if (hit.collider != null && hit.collider.gameObject.GetComponent<Entities>() != null)
        {
            if (hit.collider.gameObject == shooter.gameObject)
            {
                return false;
            }

            hit.collider.gameObject.GetComponent<Entities>().Hit_Entity(shooter);
            return true;
        }

        return false;
    }
}
