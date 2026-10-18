using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NewPlayerSnapshotInfo
{
    public int replay_id = -1;
    //default to below the floor so that their out of the way
    public Vector3 replay_position = new Vector3(0, -1000, 0);
    public Quaternion replay_rotation = Quaternion.identity;
    public PunchManager.PunchType replay_punched = PunchManager.PunchType.None;
    public bool replay_dead = false;
    public List<Entities> affectedObjects = new List<Entities>();

    public PlayerAnimationState replay_animationState = PlayerAnimationState.Idle;
}
