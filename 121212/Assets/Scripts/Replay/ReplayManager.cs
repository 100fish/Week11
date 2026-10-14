using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager instance;

    public enum State
    {
        Idle,
        Record,
        Playback
    }

    public ReplayContainer replayContainer;

    public PlayerMovement[] players = new PlayerMovement[10];
    public List<AITarget> enemies = new List<AITarget>();

    private List<ResettableObject> resettableObjects = new List<ResettableObject>();


    public State currentState;

    [Tooltip("What should the frame delta be between snapshots")]
    public float snapshotDelta;

    //the timer used to track when its time to record a new frame. updated by fixed deltatime every fixed frame
    private float m_snapshotDeltaTotal;
    private float m_time = 0;

    public int snapshotIndex = 0;
    private float m_nextSnapshotTime;
    private bool m_hasNextSnapshot = true;

    public void RegisterEnemy(AITarget enemy)
    {
        enemies.Add(enemy);   
    }

    private void Awake()
    {
        instance = this;
        replayContainer.Init();

    }

    public void StartRoundReplay()
    {
        m_time = 0;
        snapshotIndex = 0;
        currentState = State.Record;
    }

    public void Stop()
    {
        currentState = State.Idle;
        foreach (var snapshot in replayContainer.m_snapshots)
        {
            //Debug.Log("finished times are " + snapshot.frameTime);

        }
    }

    public void ResetScene()
    {
        m_time = 0;
        snapshotIndex = 0;

        foreach (ResettableObject resettableObject in resettableObjects)
        {
            resettableObject.ReplayReset();
        }
    }
    
    public void RegisterResettableObject(ResettableObject newEntry)
    {
        resettableObjects.Add(newEntry);
    }

    public void NextActivePlayer(int playerIndex)
    {
        players[playerIndex].gameObject.SetActive(true);

        players[playerIndex].SetStateControlled();
        if(playerIndex > 0)
        {
            players[playerIndex - 1].SetStatePlayback();
        }
    }

    private void FixedUpdate()
    {
        if(currentState == State.Record)
        {
            //we're counting until we reach the snapshotdelta; the time wherein we want to capture a new frame of data.
            //you set the snapshot delta in the inspector
            m_snapshotDeltaTotal += Time.fixedDeltaTime;
            if (m_snapshotDeltaTotal >= snapshotDelta)
            {
                //Debug.Log("takingandloadingsnapshot at " + m_time + " time, with index at " + snapshotIndex);
                TakeAndLoadSnapshots();

                //reset the counter till the next frame
                m_snapshotDeltaTotal -= snapshotDelta;
            }




            m_time += Time.fixedDeltaTime;
        }
    }

    private void TakeAndLoadSnapshots()
    {//this runs every frame that its time to record

        SnapshotData currentSnapshotData;

        //if the last time is before this frame
        if (replayContainer.m_snapshots[^1].frameTime < m_time)
        {
            //then make a new frame
            //Debug.Log("Making a new frame at " + m_time + " time, with index at " + snapshotIndex);
            currentSnapshotData = new SnapshotData(m_time);
        }
        else
        {
            //otherwise get the one we already recorded at this index
            //Debug.Log("Grabbing an old frame at " + m_time + " time, with index at " + snapshotIndex);
            currentSnapshotData = replayContainer.m_snapshots[snapshotIndex];
        }

        //player loading and saving

        for (int i = 0; i < players.Length; i++)
        {
            //Debug.Log("index is " + i + " and playerID is " + players[i].id);

            switch (players[i].movementState)
            {
                case PlayerMovement.MovementState.Inactive:
                    //Debug.Log("player " + players[i].id + " is inactive");
                    //if its inactive it can just be left

                    break;
                case PlayerMovement.MovementState.Controlled:
                    //if the player is controlling it we need to record it
                    //Debug.Log("player " + players[i].id + " is being saved");
                    //currentSnapshotData.AddObjectSnapshot(players[i].SaveSnapshot());
                    break;
                case PlayerMovement.MovementState.Playback:
                    //if its on playback we need to load it
                    //Debug.Log("player " + players[i].id + " is being Loaded");
                    //players[i].LoadSnapshot(currentSnapshotData.snapshots[players[i].id]);
                    break;
                default:
                    break;
            }
        }

        //enemy loading and saving
        for (int i = 0; i < enemies.Count; i++)
        {
            if (currentSnapshotData.enemySnapshots[i].replay_dead)
            {
                enemies[i].Hit_Entity(gameObject);
            }
            else if (enemies[i].dead)
            {
                //currentSnapshotData.AddEnemySnapshot(enemies[i].SaveSnapshot());
            }
        }

        //then we add that snapshot frame to the list in the container
        if (replayContainer.m_snapshots[^1].frameTime < m_time)
        {
            //then add a new frame
            //Debug.Log("Saving the new frame at " + m_time + " time, with index at " + snapshotIndex);
            replayContainer.AddSnapshot(currentSnapshotData);

        }
        else
        {
            //otherwise write the one we already recorded at this index
            //Debug.Log("Saving the OLD frame at " + m_time + " time, with index at " + snapshotIndex);
            replayContainer.m_snapshots[snapshotIndex] = currentSnapshotData;
        }

        //and we increase the index to move onto the next frame
        snapshotIndex++;
        //Debug.Log("End of this takeandloadSnapshot");
    }
}
