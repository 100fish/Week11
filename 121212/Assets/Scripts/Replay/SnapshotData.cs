using System.Collections.Generic;
//using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;


//this is the data for each snapshot FRAME
public struct SnapshotData
{
    /*
    //what time the frame was captured
    public float frameTime;
    
    //the list of object info and the corresponding IDs to keep track of
    public SnapshotInfo[] snapshots;

    public SnapshotInfoEnemy[] enemySnapshots;

    //public List<SnapshotInfoEnemy> enemySnapshots;

    public SnapshotData(float time)
    {
        frameTime = time;
        snapshots = new SnapshotInfo[10];
        for (int i = 0; i < snapshots.Length; i++)
        {
            snapshots[i] = new SnapshotInfo();

            snapshots[i].replay_id = i;
            snapshots[i].replay_position = Vector3.zero;
            snapshots[i].replay_rotation = Quaternion.identity;
            snapshots[i].replay_shootInfo = default;
        }

        enemySnapshots = new SnapshotInfoEnemy[40];
        for (int i = 0; i < enemySnapshots.Length; i++)
        {
            enemySnapshots[i] = new SnapshotInfoEnemy();

            enemySnapshots[i].replay_dead = false;
        }
    }

    public void AddObjectSnapshot(SnapshotInfo data)
    {
        snapshots[data.replay_id] = data;
    }

    public void AddEnemySnapshot(SnapshotInfoEnemy data)
    {
        enemySnapshots[data.replay_id] = data;
    }

    public SnapshotInfo GetObjectSnapshot(int id)
    {
        return snapshots[id];
    }
    */
}
