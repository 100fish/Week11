using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NewSnapshotInfoEnemy
{
    public int replay_id = -1;
    //default to below the floor so that their out of the way
    public Vector3 replay_position = new Vector3(0, -1000, 0);
    public Vector3 replay_linearVelocity = Vector3.zero;
    public Quaternion replay_rotation = Quaternion.identity;
    public Vector3 replay_angularVelocity = Vector3.zero;

    public PunchManager.PunchType replay_wasPunched = PunchManager.PunchType.None;
    public bool replay_health = false;
    public List<Entities> affectedObjects = new List<Entities>();
}