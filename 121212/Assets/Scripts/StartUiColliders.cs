using UnityEngine;

public class StartUiColliders : MonoBehaviour
{
    public int UI_int;


    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag != "Player") return;
        //print("trig: " + other.tag);
        StartUI.Move_Through_UI(UI_int);
    }
}
