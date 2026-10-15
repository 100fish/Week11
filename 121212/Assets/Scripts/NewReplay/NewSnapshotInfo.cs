using UnityEngine;

[System.Serializable]
public class NewSnapshotInfo
{
    public int replay_id = -1;
    //default to below the floor so that their out of the way
    public Vector3 replay_position = new Vector3(0, -1000, 0);
    public Quaternion replay_rotation = Quaternion.identity;
    //public PlayerMovement.ShootInfo replay_shootInfo;
}
