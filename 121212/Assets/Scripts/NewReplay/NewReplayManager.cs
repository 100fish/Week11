using System.Collections.Generic;
using UnityEngine;

public class NewReplayManager : MonoBehaviour
{
    #region Singleton
    public static NewReplayManager instance;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform playerSpawn;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);


        newReplayContainer.Init();
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
    public List<AITarget> enemies = new List<AITarget>();

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
    private float m_time = 0;

    //Used for replay, the current replay frame time
    public int snapshotIndex = 0;
    private float m_nextSnapshotTime;
    private bool m_hasNextSnapshot = true;

    //Called by enemies in their start function
    public void RegisterEnemy(AITarget enemy)
    {
        enemies.Add(enemy);
    }

    public void ClearEnemies()
    {
        enemies.Clear();
    }

    //Called by resettableobjects in their start function
    public void RegisterResettableObject(ResettableObject newEntry)
    {
        resettableObjects.Add(newEntry);
    }

    public void ClearResettableObjects()
    {
        resettableObjects.Clear();
    }

    //Called by menumanager at the very start of a run
    //Resets the playback and recording
    public void StartRoundPlaybackAndRecording()
    {
        m_time = 0; //Sets time to zero
        snapshotIndex = 0; //Sets the playback/recording to the beginning
        currentState = State.Active; //turns it on
    }

    //Called by menumanager when a round ends
    //Ends the recording and playback
    public void Stop()
    {
        currentState = State.Idle;//leaves replaymanage to sit and wait
    }

    //Called by menumanager at the START of a run
    //because inbetween rounds things like the killcam happen
    //or, in 121212, the ending sequence too
    //This is only resets the scene between ROUNDS
    //not between GAMES. The entire game is reset between games
    ///Well, I hope we don't do that in this game
    public void ResetScene()
    {
        //Reset every resettableobject
        //This only resets positios and rotation and scale
        foreach (ResettableObject resettableObject in resettableObjects)
        {
            //resettableObject.ReplayReset();
        }
    }

    //Changes the character that the player controls to the next one in the array
    //This is called with the INCREASED playerindex passed into the function
    public void NextActivePlayer(int playerIndex)
    {
        //Create a new player
        players.Add(Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity).GetComponent<NewPlayerMovement>());

        //Activate the new player script
        players[^1].SetStateControlled();
        players[^1].id = playerIndex;

        //If there was a previous player
        if (playerIndex > 0)
        {
            //Set that previous player to replay mode
            players[playerIndex - 1].SetStatePlayback();
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
            if (newReplayContainer.m_snapshots[^1].frameTime < m_time)
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
                        //we RECORD the player if its being controlled
                        //Debug.Log("player " + players[i].id + " is being saved");

                        //This is a function of the current snapshot
                        //That takes the output of a function of the player's movement script
                        //The player's function gives the movement to the snapshot frame
                        //And the snapshot frame saves it 
                        currentSnapshotData.AddPlayerSnapshot(players[i].SaveSnapshot());
                        break;
                    case NewPlayerMovement.MovementState.Playback:
                        //we LOAD the player if its being played back
                        //Debug.Log("player " + players[i].id + " is being Loaded");

                        //This is the reverse of the previous case
                        //Its a function of the current player
                        //That takes a variable (a class of player info) from the current frame
                        //And loads it (setting the player's pos, rot, and scale)
                        players[i].LoadSnapshot(currentSnapshotData.playerSnapshots[players[i].id]);
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
            //but since we save different things for the enemies, it has to be different code
            //it could be dynamic, but I'm not sure how worth it it would be
            ///What do you think?
            for (int i = 0; i < enemies.Count; i++)
            {
                
                ///its ok to make this blind grab for the enemy, cause the enemies are static
                //We always know one will be there
                
                //We check if the enemy is dead or not IN THE CURRENT SNAPSHOT
                if (currentSnapshotData.enemySnapshots[i].replay_wasPunched != PunchManager.PunchType.None)
                {
                    //If we have it down as dead in the data, then we kill it
                    ///a bit clumsy
                    enemies[i].Hit_Entity(gameObject, currentSnapshotData.enemySnapshots[i].replay_wasPunched);
                }
                else //if (enemies[i].dead)
                {
                    //OR if the enemy has actually died
                    //Then save that
                    ///We should be saving the punch instead
                    //currentSnapshotData.AddEnemySnapshot(enemies[i].SaveSnapshot());
                }
            }

        #endregion

        //So now after the player section and the enemy section
        //we have a snapshot that has all of the new data
        ///but not dynamically
        //and we've loaded all of our all data
        ///Wthiout respect to causality

        //then we add that snapshot frame to the list in the container
        if (newReplayContainer.m_snapshots[^1].frameTime < m_time)
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
}
