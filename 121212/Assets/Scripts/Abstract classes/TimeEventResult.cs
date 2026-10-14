using UnityEngine;

public abstract class TimeEventResult : MonoBehaviour
{
    public abstract void OnTimeSlowStart();
    public abstract void OnTimeSlowStop();
}
