using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

public class PunchManager : MonoBehaviour
{
    public enum PunchType
    {
        None,
        Left,
        Right
    }

    private NewMenuManager newMenuManager;

    public static PunchManager instance;

    public List<Entities> entities = new List<Entities>();

    private void Awake()
    {
        //Singleton Setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        newMenuManager = NewMenuManager.instance;
    }

    public static void Punch(NewPlayerMovement puncher, Transform punchPosition, PunchType punchType, bool canPunch, float punchRange)
    {
        if (canPunch)
        {
            //For every entity that can be punched
            //(This function is static so the list of enemies has to be passed in by the caller lol
            //if they have a reference to replaymanager they can get it from there
            //or get a reference to replaymanager from menumanager
            //Debug.Log(instance.entities.Count);
            for (int i = 0; i < instance.entities.Count; i++)
            {
                //If an entity is within range
                if (Vector3.Distance(punchPosition.position, instance.entities[i].transform.position) < punchRange)
                {
                    //And there's nothing blocking the punch
                    if (Physics.Linecast(punchPosition.position, instance.entities[i].transform.position))
                    {
                        //Debug.Log("Puncher: " + puncher.id);
                        //Debug.Log("Instance: " + instance.entities[i]);

                        Debug.Log("///////////////");
                        Debug.Log("BANG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Debug.Log("///////////////");

                        if (instance.entities[i] != null)
                        {
                            Entities.EntityType hitType = instance.entities[i].myEntityType;

                            if (hitType != Entities.EntityType.Player)
                            {

                                instance.entities[i].Hit_Entity(puncher, punchType);
                            }

                            if (puncher.newMenuManager.enemyDeathCount != puncher.newMenuManager.newReplayManager.enemies.Count
                                && hitType == Entities.EntityType.Enemy)
                            {
                                puncher.Hit_Entity(puncher, PunchType.None);
                            }
                        }
                    }
                }
            }
        }
    }
}
