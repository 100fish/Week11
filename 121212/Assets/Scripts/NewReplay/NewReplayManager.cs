using System;
using System.Collections.Generic;
using UnityEngine;

public class NewReplayManager : MonoBehaviour
{
    #region Singleton
    public static NewReplayManager instance;

    public event Action EndReplay;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] public Transform playerSpawn;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        newReplayContainer.Init();
        instance = this;
    }
    #endregion

    //What the replaymanager is currently doing
    //Its only used inside of this script and replaycontainer (i think)
    public enum State
    {
        Idle, //When the system isnt recording and isnt playing back
        Active, //When the system is recording and playing back
    }

    //This is a scriptable object
    //it holds a list of snapshot structs
    //Which each hold lists of information for each recorded frame
    public NwReplayContainer newReplayContainer;

    //This is the list of every player
    //When one player's time is up, the index changes and the next player is activated
    //No players are instantiated (will have to change)
    public List<NewPlayerMovement> players = new List<NewPlayerMovement>();

    //This is a list of the enemies
    //Since we're changing the enemies this will also have to change
    //Their ID is set by menumanager at the start of the round
    //But they register themselves in their start function
    public List<Enemy> enemies = new List<Enemy>();

    //This isnt really used for the replay, its just to avoid reloading the whole scene each time
    //The ResetabbleObjects register themselves in their start function
    //Then they are reset by the menumanager trhough ResetScene()
    private List<ResettableObject> resettableObjects = new List<ResettableObject>();

    //The current state of playback
    public State currentState;

    //The time between frames of replay
    public float snapshotDelta;

    //the timer used to track when its time to record a new frame. updated by fixed deltatime every fixed frame
    private float m_snapshotDeltaTotal;
    //private float m_time = 0(TextGenerato);

    public float m_time { get; private set; }

    //Used for replay, the current replay frame time
    public int snapshotIndex = 0;
    private float m_nextSnapshotTime;
    private bool m_hasNextSnapshot = true;

    //Called by menumanager at the very start of a run
    //Resets the playback and recording
    public void StartRoundPlaybackAndRecording(int runIndex)
    {
        /*Debug.Log("...................");
        Debug.Log("//START " + runIndex + "th RECOUNT//");
        for (int i = 0; i < newReplayContainer.m_snapshots.Count; i++)
        {
            Debug.Log("Frame at: " + newReplayContainer.m_snapshots[i].frameTime);
            for (int ii = 0; ii < newReplayContainer.m_snapshots[i].enemySnapshots.Count; ii++)
            {
                Debug.Log("Enemy: " + newReplayContainer.m_snapshots[i].enemySnapshots[ii].replay_id + " of " + newReplayContainer.m_snapshots[i].enemySnapshots.Count);
                Debug.Log("Enemy's Position this frame " + newReplayContainer.m_snapshots[i].enemySnapshots[ii].replay_position);
                Debug.Log("Enemy's Punch this frame: " + newReplayContainer.m_snapshots[i].enemySnapshots[ii].replay_wasPunched);
            }
        }
        Debug.Log("//END  " + runIndex + "th RECOUNT//");*/

        EndReplay?.Invoke();
        currentState = State.Active; //turns it on
        m_time = 0; //Sets time to zero
        snapshotIndex = 0; //Sets the playback/recording to the beginning
        //Give enemies IDs
        //Once there's a function to play the replay
        //Between levels
        ///move this there
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].id = i;
            enemies[i].ReviveEnemy();
            enemies[i].enemyReplayState = Enemy.EnemyReplayState.Playback;
        }
        //capture first frame
        TakeAndLoadSnapshots();
    }

    public void StopRound(int runIndex)
    {
        currentState = State.Idle;//leaves replaymanage to sit and wait
        
        Debug.Log("...................");
        Debug.Log("//START " + runIndex + "th RECOUNT//");
        for (int i = 0; i < newReplayContainer.m_snapshots.Count; i++)
        {
            Debug.Log("Frame at: " + newReplayContainer.m_snapshots[i].frameTime + " is: " + snapshotIndex);
            for (int ii = 0; ii < newReplayContainer.m_snapshots[i].enemySnapshots.Count; ii++)
            {
                Debug.Log("Enemy: " + newReplayContainer.m_snapshots[i].enemySnapshots[ii].replay_id + " of " + newReplayContainer.m_snapshots[i].enemySnapshots.Count);
                Debug.Log("Enemy's Position this frame " + newReplayContainer.m_snapshots[i].enemySnapshots[ii].replay_position);
                Debug.Log("Enemy's Punch this frame: " + newReplayContainer.m_snapshots[i].enemySnapshots[ii].replay_wasPunched);
            }
        }
        Debug.Log("//END  " + runIndex + "th RECOUNT//");

        EndReplay?.Invoke();


        currentState = State.Idle;//leaves replaymanage to sit and wait
        m_time = 0; //Sets time to zero
        snapshotIndex = 0; //Sets the playback/recording to the beginning
    }

    //Called by menumanager when a round ends
    //Ends the recording and playback
    public void Stop()
    {
        Debug.Log("stopping in script");
        currentState = State.Idle;//leaves replaymanage to sit and wait
        newReplayContainer.Init();

        m_time = 0; //Sets time to zero
        snapshotIndex = 0; //Sets the playback/recording to the beginning
    }

    //Changes the character that the player controls to the next one in the array
    public void NextActivePlayer(int currentPlayerIndex)
    {
        int newPlayerIndex = currentPlayerIndex + 1;

        //if the count of players is lower than the new index (current+1)
        if(players.Count <= newPlayerIndex)
        {
            players.Add(Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity).GetComponent<NewPlayerMovement>());
        }
        else
        {
            players[newPlayerIndex].gameObject.SetActive(true);
            players[newPlayerIndex].transform.position = playerSpawn.position;
            players[newPlayerIndex].transform.rotation = Quaternion.identity;
            players[newPlayerIndex].transform.Rotate(new Vector3(0,90,0));
        }

        players[newPlayerIndex].Reset_Player();

        //Activate the new player script
        //Debug.Log("Recent player's ID: " + players[newPlayerIndex].id);
        players[newPlayerIndex].SetStateControlled();
        players[newPlayerIndex].id = newPlayerIndex;

        //If there was a previous player
        if (currentPlayerIndex > -1)
        {
            //Set that previous player to replay mode
            players[currentPlayerIndex].SetStatePlayback();
        }
    }

    

    private void FixedUpdate()
    {
        if (currentState == State.Active)
        {
            //we're counting until we reach the snapshotdelta
            //when we capture a new frame of data.
            //you set the snapshot delta in the inspector
            m_snapshotDeltaTotal += Time.fixedDeltaTime;

            //If we're past due to take a new snapshot
            if (m_snapshotDeltaTotal >= snapshotDelta)
            {

                //Take a new snapshot AND
                //Load all the data for objects set to replay

                TakeAndLoadSnapshots();

                //reset the counter for the next frame
                m_snapshotDeltaTotal -= snapshotDelta;
            }
            //Keep our time up with real time
            m_time += Time.fixedDeltaTime;
        }
    }

    //This is where all the meat is
    private void TakeAndLoadSnapshots()
    {//this runs every frame that its time to record

        #region Snapshot Setup

            //Make a new snapshotData struct
            //We always make a new one, because we're always recording SOMETHING
            //Even if its a mix of loading and saving
            //And so is their causality
            NewSnapshotData currentSnapshotData;

        //if we're past the end of the recorded snapshots
        /*Debug.Log("................");
        Debug.Log("//SNAPSHOT FRAME INFO//");
        Debug.Log("Oldest Frame Is at " + newReplayContainer.m_snapshots[^1].frameTime + " and is smaller than " + m_time);*/
        ///If you get errors in this section the <= is why
        //Debug.Log("SnapshotIndex: " + snapshotIndex);
        //Debug.Log("Frametime: " + newReplayContainer.m_snapshots[^1].frameTime);
        //Debug.Log("M_Time: " + m_time);
            if (newReplayContainer.m_snapshots[^1].frameTime <= m_time)
            {
                //then make a new snapshot (all defaulted up)
                //Debug.Log("Making a new frame at " + m_time + " time, with index at " + snapshotIndex);
                
                currentSnapshotData = new NewSnapshotData(m_time);
            }
            else
            {
                //otherwise set our new one to the one we already recorded at this index
                //Debug.Log("Grabbing an old frame at " + m_time + " time, with index at " + snapshotIndex);
                currentSnapshotData = newReplayContainer.m_snapshots[snapshotIndex];
            }
        //the above if/else makes sure that if there's an old snapshot to work with, we work with that
        //if there isn't, we make a new one. 


        //Debug.Log("//END SNAPSHOT FRAME INFO//");

        #endregion
        #region Player Loading and Saving

        //foreach player ('players' array is of player movement scripts)
        for (int i = 0; i < players.Count; i++)
            {
            //Debug.Log("index is " + i + " and playerID is " + players[i].id);

            //we want to check if the player is inactive, controlled, or has already been controlled and now is being played back.
            ///Maybe with the new system, this needs reworking? Because either a player is being played back or its the active player
            switch (players[i].movementState)
            {
                case NewPlayerMovement.MovementState.Controlled:
                    currentSnapshotData.AddPlayerSnapshot(players[i].SaveSnapshot());
                    break;

                case NewPlayerMovement.MovementState.Playback:

                    //if we're past the snapshot frames on file then start saving frames of motionlessness instead so we don't get errors
                    if (currentSnapshotData.playerSnapshots.Count <= i)
                    {
                        currentSnapshotData.AddPlayerSnapshot(players[i].SaveSnapshot());
                        ///If you want to turn the replayed players off when they die, this is a good place
                    }
                    else
                    {
                        NewPlayerSnapshotInfo frameToLoad;

                        //If the snapshot at the current player's ID is blank or there isnt one there
                        if (currentSnapshotData.playerSnapshots[players[i].id] != null)
                        {
                            frameToLoad = newReplayContainer.m_snapshots[snapshotIndex].playerSnapshots[players[i].id];
                        }
                        else
                        {
                            frameToLoad = FindPlayerNonNullFrame(newReplayContainer, snapshotIndex, i - 1);
                        }

                        players[i].LoadSnapshot(frameToLoad);

                    }
                    break;

            }
            }
        //At the end of this for loop we've gone through every player
        //If they're being controlled we saved their position
        //And if they're being played back we've loaded their position
        ///This system assumes there WILL be something to load, maybe a problem?

        #endregion
        #region Enemy Loading and Saving

        //This works very similarily to the player part
        /*Debug.Log(".....................");
        Debug.Log("//ENEMY LOAD AND SAVE START//");
        Debug.Log("LOADING/SAVING " + enemies.Count + " ENEMIES");*/
        for (int i = 0; i < enemies.Count; i++)
        {
            //For every enemy in the list
            //(this is a pretty static list, it could be an array
            //We're going to check its state, and do something based on that

            //In the first round, every enemy will be active, and behaving normally.
            //So the state machine will see that its active, see that nothing has been recorded for this frame
            //(as in, the list of snapshots is less than that index of enemy, and because we go in order this does tell us that 

            //So, at the end of the first round we should have snapshots for every enemy, up until the point the round ended



            //We check the enemies state
            /*Debug.Log("Current enemy to load/save: " + enemies[i].id);
            Debug.Log("Which has a state of : " + enemies[i].enemyReplayState);
            Debug.Log("And has a punch of : " + enemies[i].punched);*/
            switch (enemies[i].enemyReplayState)
            {
                case Enemy.EnemyReplayState.Active:
                    //Checks if there is a frame for this enemy, passes if there isn't
                    if (currentSnapshotData.enemySnapshots.Count <= i)
                    {
                        enemies[i].enemyRigidBody.constraints = (RigidbodyConstraints)0;
                        currentSnapshotData.AddEnemySnapshot(enemies[i].SaveSnapshot(m_time));
                    }
                    else
                    {
                        //Debug.Log("CHANGINGTOPLAYBACK---------------------------------------------------------------------------------");
                        //enemies[i].enemyReplayState = Enemy.EnemyReplayState.Playback;

                        enemies[i].enemyRigidBody.constraints = (RigidbodyConstraints)0;
                        currentSnapshotData.AddEnemySnapshot(enemies[i].SaveSnapshot(m_time));
                    }

                    break;                  
                case Enemy.EnemyReplayState.Playback:
                    enemies[i].enemyRigidBody.constraints = (RigidbodyConstraints)126;

                    //if we're past the snapshot frames on file then start saving frames of motionlessness instead so we don't get errors
                    if (currentSnapshotData.enemySnapshots.Count <= i)
                    {
                        if(currentSnapshotData.frameTime != 0)
                        {
                            currentSnapshotData.AddEnemySnapshot(enemies[i].SaveSnapshot(m_time));
                            enemies[i].enemyReplayState = Enemy.EnemyReplayState.Active;
                        }
                        ///If you want to turn the replayed players off when they die, this is a good place
                    }
                    else
                    {
                        NewSnapshotInfoEnemy frameToLoad;

                        //If the snapshot at the current player's ID is blank or there isnt one there
                        if (currentSnapshotData.enemySnapshots[enemies[i].id] != null)
                        {
                            frameToLoad = newReplayContainer.m_snapshots[snapshotIndex].enemySnapshots[enemies[i].id];
                        }
                        else
                        {
                            frameToLoad = FindEnemyNonNullFrame(newReplayContainer, snapshotIndex, i - 1);
                        }

                        enemies[i].LoadSnapshot(frameToLoad, m_time);

                    }
                    break;
            }
        }

        //Debug.Log("//ENEMY LOAD AND SAVE END//");

        #endregion

        //So now after the player section and the enemy section
        //we have a snapshot that has all of the new data
        ///but not dynamically
        //and we've loaded all of our all data
        ///Wthiout respect to causality



        //then we add that snapshot frame to the list in the container
        ///or here
        if (newReplayContainer.m_snapshots[^1].frameTime <= m_time)
        {
            //if we're past the end of the list of snapshots, add a new one
            //Debug.Log("Saving the new frame at " + m_time + " time, with index at " + snapshotIndex);
            newReplayContainer.AddSnapshot(currentSnapshotData);

        }
        else
        {
            //otherwise write the one we already recorded at this index
            //Debug.Log("Saving the OLD frame at " + m_time + " time, with index at " + snapshotIndex);
            newReplayContainer.m_snapshots[snapshotIndex] = currentSnapshotData;
        }

        //and we increase the index to move onto the next frame
        snapshotIndex++;
        //Debug.Log("End of this takeandloadSnapshot");
    }
    private NewPlayerSnapshotInfo FindPlayerNonNullFrame(NwReplayContainer newReplayContainer, int snapshotIndex, int index)
    {
        if (newReplayContainer.m_snapshots[snapshotIndex].playerSnapshots[players[index].id] != null)
            return newReplayContainer.m_snapshots[snapshotIndex].playerSnapshots[players[index].id];
        else
            return FindPlayerNonNullFrame(newReplayContainer, snapshotIndex-1, index);
    }
    private NewSnapshotInfoEnemy FindEnemyNonNullFrame(NwReplayContainer newReplayContainer, int snapshotIndex, int index)
    {
        if (newReplayContainer.m_snapshots[snapshotIndex].enemySnapshots[enemies[index].id] != null)
            return newReplayContainer.m_snapshots[snapshotIndex].enemySnapshots[enemies[index].id];
        else
            return FindEnemyNonNullFrame(newReplayContainer, snapshotIndex-1, index);
    }
}
