using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//this is the data for each snapshot FRAME
public struct NewSnapshotData
{
    //what time the frame was captured
    public float frameTime;

    //the list of object info and the corresponding IDs for PLAYERS
    public NewSnapshotInfo[] snapshots; ///rename this shit bro :sob: so bad :sob: zhen bu hao

    //the list of object info and the corresponding IDs for ENEMIES
    public NewSnapshotInfoEnemy[] enemySnapshots;

    public NewSnapshotData(float time)
    {
        //SAVE: the time of the frame
        frameTime = time;

        //SAVE: The player information 
        ///[NEEDS CHANGING BECAUSE PLAYERS WILL BE LIST NOT ARRAY]
        snapshots = new NewSnapshotInfo[10];///update to be dynamic
        //for each player SAVE:
        for (int i = 0; i < snapshots.Length; i++)
        {
            //create a new CLASS for the player's data for that frame
            snapshots[i] = new NewSnapshotInfo();

            //SAVE: the player's replay ID
            snapshots[i].replay_id = i;
            //DEFAULT: if the player's position
            snapshots[i].replay_position = Vector3.zero;
            //DEFAULT: if the player's rotation
            snapshots[i].replay_rotation = Quaternion.identity;
            //DEFAULT: if the player shot
            snapshots[i].replay_shootInfo = default;
        }

        //SAVE: The enemy infrmation
        ///[NEEDS CHANGING BECAUSE ENEMIES WILL BE LIST NOT ARRAY]
        enemySnapshots = new NewSnapshotInfoEnemy[40];///update to be dynamic
        for (int i = 0; i < enemySnapshots.Length; i++)
        {
            //create a new CLASS for the enemies's data for that frame
            enemySnapshots[i] = new NewSnapshotInfoEnemy();

            //DEFAULT: if the enemy died that frame. That's all we save
            enemySnapshots[i].replay_dead = false;
        }
    }

    //used by ReplayManager when saving player info
    public void AddObjectSnapshot(NewSnapshotInfo data)
    {
        snapshots[data.replay_id] = data;
    }

    //used by ReplayManager when saving Enemy info
    public void AddEnemySnapshot(NewSnapshotInfoEnemy data)
    {
        enemySnapshots[data.replay_id] = data;
    }

    //Used by ReplayManager when loading snapshots
    public NewSnapshotInfo GetObjectSnapshot(int id)
    {
        return snapshots[id];
    }
}
