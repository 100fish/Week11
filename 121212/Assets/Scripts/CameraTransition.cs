using System;
using UnityEngine;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] private float sceneTransitionTime;
    [SerializeField] private float sceneTransitionTimer;

    [SerializeField] private CustomRenderTexture camART;
    [SerializeField] private RenderTexture camBRT;

    TransitionCause transitionCause;

    [SerializeField] private Color[] colors;


    public event Action EndCameraTransition;

    public enum TransitionCause
    {
        None,
        Enemy,
        Environment,
        Progression,
    }

    void Start()
    {
        sceneTransitionTimer = sceneTransitionTime;
    }

    public void SetupFirstCamera(Camera newCam)
    {
        newCam.targetTexture = camBRT;
        Shader.SetGlobalFloat("_MyFireProgress", 2);
    }

    public void StartTransition(Camera camA, Camera camB, TransitionCause cause)
    {
        camA.targetTexture = camART;
        camB.targetTexture = camBRT;

        camA.cullingMask = 247;

        transitionCause = cause;

        Shader.SetGlobalColor("_MyFireColour", colors[(int)cause]);
        camART.Update();
    }

    private void Update()
    {
        if (transitionCause == TransitionCause.None)
            return;

        camART.Update();

        if (sceneTransitionTimer > 0)
        {
            sceneTransitionTimer -= Time.deltaTime;

            Shader.SetGlobalFloat("_MyFireProgress", 1 - Mathf.InverseLerp(0, sceneTransitionTime, sceneTransitionTimer));
            camART.Update();
        }
        else
        {
            EndCameraTransition.Invoke();

            

            sceneTransitionTimer = sceneTransitionTime;
            Shader.SetGlobalFloat("_MyFireProgress", -1);
            transitionCause = TransitionCause.None;
        }

    }
}