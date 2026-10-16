using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//Make a new scriptableobject
[CreateAssetMenu(menuName = "Nw Replay Container")]
public class NwReplayContainer : ScriptableObject
{//THIS THING STORES THE REPLAY
    //ALL THE DATA

    //List of snapshots
    [SerializeField]
    public List<NewSnapshotData> m_snapshots;

    //Sets it up, called by replayManager in awake
    public void Init()
    {
        m_snapshots = new List<NewSnapshotData>();//Create the blank list ready to have snapshots logged
        m_snapshots.Add(new NewSnapshotData(0));///Adds an empty snapshot at the beginning, not sure why?
    }

    //Called by ReplayManager in TakeAndLoad
    //This is only called when the gameplay has progressed further than 
    //what has already been recorded
    public void AddSnapshot(NewSnapshotData snapshot)
    {
        m_snapshots.Add(snapshot);
    }

    //get the data of a specific SNAPSHOT frame,
    ///ok I dont think this even gets called so lets not worry about it
    //cause we do playback in a different way to the original tutorial
    public bool GetSnapshot(int index, out NewSnapshotData data)
    {   //if the index is past the amount of frames stored
        if (index >= m_snapshots.Count)
        {
            //then create a new snapshotframe, um with a time of -1 for some reason
            //idrk whats going on here
            data = new NewSnapshotData(-1); 
            return false; //return false cause we couldn't find a frame
        }

        if (index < 0)
        {
            data = new NewSnapshotData(-1);
            return false; //idk whats happening here either oops
        }

        //if we really can find an index though, here it is!
        //and we return true with it
        data = m_snapshots[index];
        return true;
    }

    public void ClearAllReplay()
    {
        m_snapshots.Clear();
    }
}