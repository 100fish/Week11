using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraTransition : MonoBehaviour
{
    private NewMenuManager newMenuManager;

    [SerializeField] private LayerMask camACullMask;

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

        newMenuManager = NewMenuManager.instance;
    }

    public void SetupFirstCamera(Camera newCam)
    {
        newCam.targetTexture = camBRT;
        Shader.SetGlobalFloat("_MyFireProgress", 2);
    }

    private Vector3 camASavedPos;
    private Quaternion camASavedRot;

    private Transform camATransform;

    public void StartTransition(Camera camA, Camera camB, NewMenuManager.RoundEndCause cause)
    {
        Debug.Log("CAMERAGO!!!");
        camA.targetTexture = camART;
        camB.targetTexture = camBRT;

        camATransform = camA.transform;

        camASavedPos = camA.transform.position;
        camASavedRot = camA.transform.rotation;

        switch (cause)
        {
            case NewMenuManager.RoundEndCause.fall:
                transitionCause = TransitionCause.Environment;
                break;
            case NewMenuManager.RoundEndCause.projectile:
                transitionCause = TransitionCause.Enemy;
                break;
            case NewMenuManager.RoundEndCause.player:
                transitionCause = TransitionCause.Progression;
                break;
            case NewMenuManager.RoundEndCause.time:
                transitionCause = TransitionCause.Environment;
                break;
            default:
                break;
        }


        Shader.SetGlobalColor("_MyFireColour", colors[(int)transitionCause]);
        camART.Update();
    }

    private void Update()
    {
        if (transitionCause == TransitionCause.None)
            return;

        camATransform.position = camASavedPos;
        camATransform.rotation = camASavedRot;

        camART.Update();

        if (sceneTransitionTimer > 0)
        {
            sceneTransitionTimer -= Time.deltaTime;

            Shader.SetGlobalFloat("_MyFireProgress", 1 - Mathf.InverseLerp(0, sceneTransitionTime, sceneTransitionTimer));
            camART.Update();
        }
        else
        {

            newMenuManager.ChangeRoundState(NewMenuManager.RoundState.Countdown);



            sceneTransitionTimer = sceneTransitionTime;
            Shader.SetGlobalFloat("_MyFireProgress", 1);
            transitionCause = TransitionCause.None;
        }

    }
}