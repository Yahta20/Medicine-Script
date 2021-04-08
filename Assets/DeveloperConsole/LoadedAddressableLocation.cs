using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class LoadedAddressableLocation : MonoBehaviour
{
    string lable = "resources";
    public IList<IResourceLocation> AssetsLocation { get; } = new List<IResourceLocation>();

    public async Task InitResources()
    {
        string commandString = "-";
        await AddressableLocationLoader.GetAll(lable, AssetsLocation);

        foreach (var location in AssetsLocation)
            commandString += location.PrimaryKey + ", ";

        Debug.Log("Prefabs:" + commandString);  //вывожу на консоль имена перфабов которые нахоядся в адресебл и помечены лейблом resources

        AssetsLocation.Clear();  //очищаю что-бы не дублировалось при повторном вызове
    }

    public async Task InitCommand()
    {
        string lable = "commands";

        string commands = string.Empty;

        await AddressableLocationLoader.GetAll(lable, AssetsLocation);

        //foreach (var command in AssetsLocation)
        //{
        //    commands += command.PrimaryKey;

        //    Debug.Log("list command - " + commands);
        //}

        for (int i = 0; i < AssetsLocation.Count; i++)
        {
            commands += AssetsLocation[i].PrimaryKey;

            Debug.Log("list command - " + commands);
        }

        AssetsLocation.Clear();
    }
}