using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


//this is the data for each snapshot FRAME
public struct NewSnapshotData
{
    //what time the frame was captured
    public float frameTime;

    //the list of object info and the corresponding IDs for PLAYERS
    public List<NewPlayerSnapshotInfo> playerSnapshots;

    //the list of object info and the corresponding IDs for ENEMIES
    public List<NewSnapshotInfoEnemy> enemySnapshots;

    //This is called when we create a NEW frame of snapshot data
    public NewSnapshotData(float time )
    {
        //SAVE: the time of the frame
        frameTime = time;

        //SAVE: The player information 
        playerSnapshots = new List<NewPlayerSnapshotInfo>();

        //for each player SAVE:
        for (int i = 0; i < playerSnapshots.Count; i++)
        {
            //create a new CLASS for the player's data for that frame
            playerSnapshots[i] = new NewPlayerSnapshotInfo();

            //SAVE: the player's replay ID
            playerSnapshots[i].replay_id = i;
            //DEFAULT: if the player's position
            playerSnapshots[i].replay_position = Vector3.zero;
            //DEFAULT: if the player's rotation
            playerSnapshots[i].replay_rotation = Quaternion.identity;
            //DEFAULT: if the player shot
            playerSnapshots[i].replay_punched = PunchManager.PunchType.None;
        }

        //SAVE: The enemy infrmation
        enemySnapshots = new List<NewSnapshotInfoEnemy>();
        for (int i = 0; i < enemySnapshots.Count; i++)
        {
            //create a new CLASS for the enemies's data for that frame
            enemySnapshots[i] = new NewSnapshotInfoEnemy();

            //SAVE: the enemies's replay ID
            enemySnapshots[i].replay_id = i;
            //DEFAULT: if the enemies's position
            enemySnapshots[i].replay_position = Vector3.zero;
            enemySnapshots[i].replay_linearVelocity = Vector3.zero;
            //DEFAULT: if the enemies's rotation
            enemySnapshots[i].replay_rotation = Quaternion.identity;
            enemySnapshots[i].replay_angularVelocity = Vector3.zero;
            //DEFAULT: if the enemies got punched and how
            enemySnapshots[i].replay_wasPunched = PunchManager.PunchType.None;
        }
    }

    //used by ReplayManager when saving player info
    public void AddPlayerSnapshot(NewPlayerSnapshotInfo data)
    {
        //If this is a new player (past the count of the current players for this frame) then add it to the end of the list
        if(data.replay_id >= playerSnapshots.Count)
        {
            playerSnapshots.Add(data);
        }
        //If this is an old player then write over the data
        else
        {
            playerSnapshots[data.replay_id] = data;
        }
    }

    //used by ReplayManager when saving Enemy info
    public void AddEnemySnapshot(NewSnapshotInfoEnemy data)
    {
        //If this is a new enemy (past the count of the current players for this frame) then add it to the end of the list
        if (data.replay_id >= enemySnapshots.Count)
        {
            enemySnapshots.Add(data);
        }
        //If this is an old player then write over the data
        else
        {
            enemySnapshots[data.replay_id] = data;
        }
    }

    //Used by ReplayManager when loading snapshots
    public NewPlayerSnapshotInfo GetPlayerSnapshot(int id)
    {
        return playerSnapshots[id];
    }
    public NewSnapshotInfoEnemy GetEnemySnapshot(int id)
    {
        return enemySnapshots[id];
    }
}
