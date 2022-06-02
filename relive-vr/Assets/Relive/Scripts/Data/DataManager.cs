using HCIKonstanz.Colibri;
using Relive.Data;
using Relive.Sync;
using Relive.UI.Sessions;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public IDataProvider DataProvider;
    public StudyPanel StudyPanel;

    private async void Awake()
    {
        Instance = this;

        if (SyncConfiguration.Current.EnableSync)
        {
            var remoteProvider = new RemoteDataProvider();
            DataProvider = remoteProvider;

            // check if there are already active studies
            var studies = await remoteProvider.GetStudies();
            if (!studies.Any(study => study.IsActive))
                StudyPanel.DisplayStudies(remoteProvider);
        }
        else
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("Select a log file", "Assets/Data/study_logs", "json");
            DataProvider = new JsonFileDataProvider(path);
#endif
        }
    }

    private void OnDestroy()
    {
        if (SyncConfiguration.Current.EnableSync)
        {
            // clear out the files that RemoteDataProvider has downloaded
            string path = Application.temporaryCachePath;

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
