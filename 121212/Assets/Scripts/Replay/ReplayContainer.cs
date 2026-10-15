using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Replay Container")]
public class ReplayContainer : ScriptableObject
{
    /*
    [SerializeField]
    public List<SnapshotData> m_snapshots;

    public void Init()
    {
        m_snapshots = new List<SnapshotData>();
        m_snapshots.Add(new SnapshotData(0));
    }

    public void AddSnapshot(SnapshotData snapshot)
    {
        m_snapshots.Add(snapshot);
    }

    //get the data of the snapshot FRAME
    public bool GetSnapshot(int index, out SnapshotData data)
    {   //if the index is greater than the amount of snapshot FRAMES on file
        if (index >= m_snapshots.Count)
        {
            data = new SnapshotData(-1); //create a new snapshot frame? with a time of minus one?
                                         //I think its because you have to return something for an out paramater
            return false;
        }

        if (index < 0)
        {
            data = new SnapshotData(-1);
            return false;
        }

        data = m_snapshots[index];
        return true;
    }
    */
}