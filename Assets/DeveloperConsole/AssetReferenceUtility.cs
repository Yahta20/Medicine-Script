using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetReferenceUtility : MonoBehaviour
{
    public AssetReference objectToLoad;
    public AssetReference accessaryObjectToLoad;
    private GameObject instansiatedObject;
    private GameObject instansiatedAccessaryObject;
    public Vector3 position;

    void Start()
    {
        Addressables.LoadAssetAsync<GameObject>(objectToLoad).Completed += ObjectLoadDone;
    }

    private void ObjectLoadDone(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            var loadedObject = obj.Result;

            instansiatedObject = Instantiate(loadedObject, position, Quaternion.identity);

            Debug.Log("Succeded load object!" + instansiatedObject.name);

            //if (accessaryObjectToLoad != null)
            //{
            //    accessaryObjectToLoad.InstantiateAsync(instansiatedObject.transform).Completed += op =>
            //    {
            //        if (op.Status == AsyncOperationStatus.Succeeded)
            //        {
            //            instansiatedAccessaryObject = op.Result;

            //            Debug.Log(instansiatedAccessaryObject.name);
            //        }
            //    };
            //}
        }
    }
}