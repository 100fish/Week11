using UnityEngine;

public class ResettableMovingEnemy : ResettableObject
{
    AITarget aITarget;

    protected override void Start()
    {
        base.Start();

        aITarget = GetComponent<AITarget>();
    }

    public override void ReplayReset()
    {
        base.ReplayReset();

        aITarget.target_position = 1;
        aITarget.dead = false;

        foreach (var meshRenderer in aITarget.meshRenderers)
        {
            meshRenderer.material = aITarget.aliveMaterial;
        }
    }

}
