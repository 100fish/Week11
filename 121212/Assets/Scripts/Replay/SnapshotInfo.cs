using UnityEngine;

[System.Serializable]
public class SnapshotInfo
{
    public int replay_id = -1;
    public Vector3 replay_position = new Vector3(0, -1000, 0);
    public Quaternion replay_rotation = Quaternion.identity;
    //public PlayerMovement.ShootInfo replay_shootInfo;
}
