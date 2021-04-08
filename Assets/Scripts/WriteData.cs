using System;
using System.IO;
using UnityEngine;

public class WriteData : MonoBehaviour
{
    string path = @"C:\Projects\Medicine Script\Assets\Resources_moved\commands.txt";

    string command = "create_ob;Doctor";

    private async void Start()
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                await sw.WriteLineAsync(command);
            }

            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                await sw.WriteLineAsync("Дозапись");
                await sw.WriteAsync("4.5");
            }
            Debug.Log("Write is done");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}