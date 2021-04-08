using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReadData : MonoBehaviour
{
    readonly string path = @"C:\Projects\Medicine Script\Assets\Resources_moved\commands.txt";

    [SerializeField] private string[] commands;
    [SerializeField] int countString;

    public string[] Commands { get => commands; set => commands = value; }

    private void Awake()
    {
        countString = File.ReadAllLines(path).Length;

        commands = new string[countString];

        //Read();
    }

    public void Read()
    {
        try
        {
            // асинхронное чтение
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
            {
                //while((Commands = await sr.ReadLineAsync()) != null)
                //{
                    //print(Commands);
                //}


                //Commands = sr.ReadToEndAsync().Result;

                for (int i = 0; i < countString; i++)
                {
                    Commands[i] = sr.ReadLineAsync().Result;
                    //Debug.Log(Commands);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}