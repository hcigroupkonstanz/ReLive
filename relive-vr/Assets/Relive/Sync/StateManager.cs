using Cysharp.Threading.Tasks;
using HCIKonstanz.Colibri;
using HCIKonstanz.Colibri.Core;
using Newtonsoft.Json;
using Relive.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Relive.Sync
{
    public class StateManager : SingletonBehaviour<StateManager>
    {
        public List<State> States { get; private set; }
        public readonly BehaviorSubject<bool> IsInitialized = new BehaviorSubject<bool>(false);
        private UnityWebRequest webRequest;
        private float timer = 0.0f;

        public bool IsDownloading { get; private set; } = true;
        public float DownloadProgressMb => webRequest == null ? 0 : webRequest.downloadedBytes / 1048576f;
        public bool IsParsing { get; private set; }

        private void OnEnable()
        {
            StudyManager.Instance.ActiveStudy
                .First(study => study != null)
                .Subscribe(study => LoadStates(study.Name));
        }

        void Update()
        {
            if (webRequest != null && timer > 1f && IsDownloading)
            {
                Debug.Log("State bytes downloaded: " + (webRequest.downloadedBytes / 1048576f).ToString("f2") + " MB");
                timer = 0f;
            }
            timer += Time.deltaTime;
        }

        private async void LoadStates(string studyName)
        {
            var syncConf = SyncConfiguration.Current;
            using (webRequest = UnityWebRequest.Get($"{syncConf.Protocol}{syncConf.ServerAddress}:{syncConf.RestPort}/studies/{studyName}/states"))
            {
                Debug.Log("Fetching states via http");
                await webRequest.SendWebRequest();
                IsParsing = true;
                IsDownloading = false;
                Debug.Log("Download finished, parsing states");

                var data = webRequest.downloadHandler.data;
                await Task.Run(() =>
                {
                    using (var stream = new MemoryStream(data))
                    using (var writer = new StreamWriter(stream))
                    {
                        States = JsonExtensions.DeserializeCompressed<List<State>>(stream);
                        Debug.Log($"Fetched {States.Count} states via http");
                    }
                });

                IsInitialized.OnNext(true);
                webRequest = null;
                IsParsing = false;
            }
        }
    }
}
