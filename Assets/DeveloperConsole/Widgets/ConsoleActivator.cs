using UnityEngine;

public class ConsoleActivator : MonoBehaviour
{
    void Update()
    {
        var uiActive = Console.DeveloperConsole.Instance;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            uiActive.active = uiActive.active == false ? true : false;
        }
    }
}