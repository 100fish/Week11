using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SunRotator : TimeEventResult
{
    [SerializeField]
    private float rotationSpeed;

    [SerializeField]
    private float rotationSpeedIntensifier;

    [SerializeField]
    Light spotLight1;

    [SerializeField]
    Light spotLight2;

    [SerializeField]
    private float spotLightMin;

    [SerializeField]
    private float spotLightMax;

    void Update()
    {
        transform.Rotate(Time.unscaledDeltaTime * rotationSpeed, 0, 0);

        Vector3 upDifference = Vector3.up - transform.forward;
        float lerpBy = Mathf.Lerp(spotLightMin, spotLightMax, 1 - upDifference.y);

        spotLight1.intensity = lerpBy;
        spotLight2.intensity = lerpBy;

    }

    public override void OnTimeSlowStart()
    {
        rotationSpeed *= rotationSpeedIntensifier;
    }

    public override void OnTimeSlowStop()
    {
        rotationSpeed = 1;

    }
}
