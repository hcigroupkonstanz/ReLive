using UnityEngine;
using HCIKonstanz.Colibri.Core;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace HCIKonstanz.Colibri.Store
{
    public class RestApi : SingletonBehaviour<RestApi>
    {
        public async Task<T> Get<T>(string appName, string objectName)
        {
            var config = SyncConfiguration.Current;
            using (UnityWebRequest request = UnityWebRequest.Get("http://" + config.ServerAddress + ":9001/api/store/" + appName + "/" + objectName))
            {
                request.method = UnityWebRequest.kHttpVerbGET;
                request.SetRequestHeader("Accept", "application/json");
                await request.SendWebRequest();
                if (!request.isNetworkError && request.responseCode == 200)
                {
                    return JsonUtility.FromJson<T>(request.downloadHandler.text);
                }
            }
            return default(T);
        }

        public async Task<bool> Put(string appName, string objectName, object putObject)
        {
            string jsonData = JsonUtility.ToJson(putObject);
            var config = SyncConfiguration.Current;
            using (UnityWebRequest request = UnityWebRequest.Put("http://" + config.ServerAddress + ":9001/api/store/" + appName + "/" + objectName, jsonData))
            {
                request.method = UnityWebRequest.kHttpVerbPUT;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                await request.SendWebRequest();
                if (!request.isNetworkError && (request.responseCode == 200 || request.responseCode == 201))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> Delete(string appName, string objectName)
        {
            var config = SyncConfiguration.Current;
            using (UnityWebRequest request = UnityWebRequest.Delete("http://" + config.ServerAddress + ":9001/api/store/" + appName + "/" + objectName))
            {
                request.method = UnityWebRequest.kHttpVerbDELETE;
                request.SetRequestHeader("Content-Type", "application/json");
                await request.SendWebRequest();
                if (!request.isNetworkError && request.responseCode == 200)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
