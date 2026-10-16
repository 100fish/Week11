using UnityEngine;

public abstract class Entities : MonoBehaviour
{
    public abstract void Hit_Entity(GameObject perpetrator, PunchManager.PunchType punchType);

    ///Add tomorrow
    //public abstract void Register_Entity();
}
