using System;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    [SerializeField]
    public TrailRenderer[] trailRenderers = new TrailRenderer[10];
        
    private void Start()
    {
        for (int i = 1; i < trailRenderers.Length; i++)
        {
            trailRenderers[i].time = UnityEngine.Random.Range(0.1f, trailRenderers[0].time * .9f);
            trailRenderers[i].widthMultiplier = UnityEngine.Random.Range(0.1f, trailRenderers[0].widthMultiplier * .9f);

            trailRenderers[i].transform.localPosition = new Vector3(UnityEngine.Random.Range(-.5f, .5f),
                                                                    UnityEngine.Random.Range(-.5f, .5f),
                                                                    UnityEngine.Random.Range(-.5f, .5f));
        }
    }

    public void EnableParticles()
    {
        for (int i = 0; i < trailRenderers.Length; i++)
        {
            trailRenderers[i].enabled = true;
        }
    }

    public void DisableParticles()
    {
        for (int i = 0; i < trailRenderers.Length; i++)
        {
            trailRenderers[i].enabled = false;
        }
    }
}
