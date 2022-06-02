using HCIKonstanz.Colibri;
using Relive.Sync;
using TMPro;
using UnityEngine;

namespace Relive.UI
{
    public class InfoPanelVR : MonoBehaviour
    {
        public TextMeshProUGUI InfoPanelText;

        private float prevProgress;
        private float prevProgressTime;

        // Update is called once per frame
        void Update()
        {
            if (SyncConfiguration.Current.EnableSync)
            {
                if (StateManager.Instance.IsDownloading)
                {
                    var progress = StateManager.Instance.DownloadProgressMb;
                    if (progress < float.Epsilon)
                        InfoPanelText.text = "Preparing data on server";
                    else
                    {
                        if (progress == prevProgress && (Time.unscaledTime - prevProgressTime) > 3)
                            InfoPanelText.text = "Preparing data on server";
                        else
                            InfoPanelText.text = $"Downloading:\n{progress.ToString("f2")} MB";

                        if (progress != prevProgress)
                        {
                            prevProgress = progress;
                            prevProgressTime = Time.unscaledTime;
                        }
                    }
                }
                else if (StateManager.Instance.IsParsing)
                {
                    InfoPanelText.text = $"Parsing data - application may freeze briefly";
                }
                else
                {
                    InfoPanelText.text = $"Finished!";
                }
            }
            else if (DataManager.Instance.DataProvider != null && DataManager.Instance.DataProvider.GetProgress() > -1)
            {
                InfoPanelText.text = "Loading. Please wait " + DataManager.Instance.DataProvider.GetProgress() + "%";
            }
        }
    }
}
