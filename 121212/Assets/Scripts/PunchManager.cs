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
            //for
            for (int i = 0; i < instance.entities.Count; i++)
            {
                //distance check
                if (Vector3.Distance(punchPosition.position, instance.entities[i].transform.position) < punchRange)
                {
                    //collision check
                    if (Physics.Linecast(punchPosition.position, instance.entities[i].transform.position))
                    {
                        //final check
                        if (instance.entities[i] != null)
                        {
                            //get and store entity info
                            Entities.EntityType hitType = instance.entities[i].myEntityType;
                            bool enemyHealth = instance.entities[i].GetComponent<Enemy>().health;
                            //Debug.Log("Enemy health when punched: " + enemyHealth);

                            //hit entity
                            if (hitType != Entities.EntityType.Player)
                            {
                                instance.entities[i].Hit_Entity(puncher, punchType);
                            }

                            //end round by hitting player back
                            if (puncher.newMenuManager.enemyDeathCount != puncher.newMenuManager.newReplayManager.enemies.Count
                                && hitType == Entities.EntityType.Enemy)
                            {
                                if (enemyHealth && punchType == PunchType.Left)
                                {
                                    //fucking using the function wrong
                                    //Passing punchtype right means they ended the round by killing a yellow
                                    //Debug.Log("Yellow");
                                    puncher.Hit_Entity(puncher, PunchType.Right);
                                }
                                else
                                {
                                    //Debug.Log("Red");
                                    puncher.Hit_Entity(puncher, PunchType.None);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
