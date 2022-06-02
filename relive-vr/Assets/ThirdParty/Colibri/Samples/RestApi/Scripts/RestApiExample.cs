using System.Collections;
using System.Collections.Generic;
using HCIKonstanz.Colibri.Store;
using UnityEngine;

public class RestApiExample : MonoBehaviour
{
    public static readonly string APP_NAME = "RestApiExample";
    private RestApi restApi;

    void OnEnable()
    {
        // Initialize REST API
        restApi = RestApi.Instance;
    }

    async void Start()
    {
        // Create example object
        ExampleClass exampleObject = new ExampleClass();
        exampleObject.Id = 1234;
        exampleObject.Name = "Charly Sharp";
        exampleObject.Position = new Vector3(1f, 1f, 1f);
        exampleObject.Rotation = new Quaternion(0f, 0f, 0f, 0f);

        // Save example object using REST API
        bool putSuccess = await restApi.Put(APP_NAME, "exampleObject", exampleObject);
        Debug.Log("Save Example Object Success: " + putSuccess);

        // Get saved example object from REST API
        ExampleClass exampleObjectGet = await restApi.Get<ExampleClass>(APP_NAME, "exampleObject");
        if (exampleObjectGet != null)
        {
            Debug.Log("Get Example Object: " + exampleObjectGet.Name);
        }
        else
        {
            Debug.LogError("Get Example Object failed!");
        }

        // Delete saved example object with REST API
        // bool deleteSuccess = await restApi.Delete(APP_NAME, "exampleObject");
        // Debug.Log("Delete Example Object Success: " + deleteSuccess);

    }

}
