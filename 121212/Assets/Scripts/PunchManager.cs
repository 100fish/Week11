using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PunchManager : MonoBehaviour
{
    public enum PunchType
    {
        None,
        Left,
        Right
    }

    private NewMenuManager newMenuManager;

    static PunchManager instance;

    public void Start()
    {
        //Singleton Setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);


        newMenuManager = NewMenuManager.instance;
    }

    public static void Punch(GameObject puncher, Transform punchPosition, PunchType punchType, bool canPunch, float punchRange, Entities[] entities)
    {
        if (canPunch)
        {
            //For every entity that can be punched
            //(This function is static so the list of enemies has to be passed in by the caller lol
            //if they have a reference to replaymanager they can get it from there
            //or get a reference to replaymanager from menumanager
            for (int i = 0; i < entities.Length; i++)
            {
                //If an entity is within range
                if (Vector3.Distance(punchPosition.position, entities[i].transform.position) < punchRange)
                {
                    //And there's nothing blocking the punch
                    if (Physics.Linecast(punchPosition.position, entities[i].transform.position))
                    {
                        entities[i].Hit_Entity(puncher, punchType);
                    }
                }
            }
        }
    }
}
