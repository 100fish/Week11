using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class TargetManager : MonoBehaviour
{
    private static List<Transform> players = new List<Transform>();

    public void Awake()
    {
        players.Clear();
    }

    public static void Add_Player_To_Scene(Transform _player)
    {
        players.Add(_player);
    }

    public static Transform Get_Active_Target(Transform origin, float _attacking_distance, string mask)
    {
        float _current_distance = Mathf.Infinity;

        Transform _current_target = null;

        //print("distance: " + 0);

        for (var i = 0; i < players.Count; i++)
        {
            //print("distance: " + 1);

            if (players[i].gameObject.activeSelf == false) continue;
            if (players[i].gameObject.GetComponent<PlayerMovement>().Is_Ghost()) continue;

            //print("distance: " + 2);

            float _new_distance = Vector3.Distance(origin.position, players[i].position);

            if (_new_distance < _current_distance || _current_distance == Mathf.Infinity)
            {
               // print("distance: " + 3);
                _current_distance = _new_distance;
                _current_target = players[i];
                break;
            }
        }

        //print("distance: " + _current_distance);

        if (_current_distance > _attacking_distance  ) return null;

        return _current_target;
    }
}
