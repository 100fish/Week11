using UnityEngine;

public class StartUiColliders : MonoBehaviour
{
    public int UI_int;

    bool _set = false;
    private void OnTriggerEnter(Collider other)
    {
        if (_set) return;

        print("on enter; 1");
        if (other.tag != "Player") return;
        //print("trig: " + other.tag);

        if(UI_int < 4)
        {
            _set = true;
            StartUI.Move_Through_UI(UI_int);
            return;
        }

        print("on enter; 2");

        if (UI_int == 5)
        {
            _set = true;
            print("on enter; 3");

            StartUI.Set_UI(UI_int);
            NewMenuManager.instance.Uppercut_On();
        }
       
    }
}
