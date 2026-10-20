using UnityEngine;

public class StartUiColliders : MonoBehaviour
{
    public int UI_int;


    private void OnTriggerEnter(Collider other)
    {
        //print("on enter; 1");
        if (other.tag != "Player") return;
        //print("trig: " + other.tag);

        if(UI_int < 4)
        {
            StartUI.Move_Through_UI(UI_int);
            return;
        }

        //print("on enter; 2");

        if (UI_int == 5)
        {
            //print("on enter; 3");

            StartUI.Set_UI(UI_int);
            NewMenuManager.instance.Uppercut_On();
        }
       
    }
}
