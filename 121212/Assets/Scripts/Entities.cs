using UnityEngine;

public abstract class Entities : MonoBehaviour
{

    public enum EntityType
    {
        Player,
        Enemy,
        Projectile
    }

    public EntityType myEntityType;

    protected PunchManager punchManager;

    public abstract void Hit_Entity(Entities perpetrator, PunchManager.PunchType punchType);

    public void Get_PunchManager()
    {
        punchManager = PunchManager.instance;
    }
    public void Register_Punchable()
    {
        Get_PunchManager();
        Debug.Log(punchManager);
        punchManager.entities.Add(this);
    }

    public void Unregister_Punchable()
    {
        Get_PunchManager();
        punchManager.entities.Remove(this);
    }

    protected virtual void Start()
    {
        Get_PunchManager();
    }

    protected virtual void OnEnable()
    {
        Register_Punchable();
    }

    protected virtual void OnDisable()
    {
        Unregister_Punchable();
    }
}
