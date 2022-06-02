using UnityEngine;
using UnityEditor;

namespace HCIKonstanz.Colibri
{
    public class SyncConfigEditor : EditorWindow
    {
        [MenuItem("Colibri/Set Server")]
        private static void Init()
        {
            SyncConfigEditor window = ScriptableObject.CreateInstance<SyncConfigEditor>();

            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 250);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            var config = SyncConfiguration.Current;
            EditorGUILayout.LabelField("Sync server address:", EditorStyles.wordWrappedLabel);
            config.ServerAddress = EditorGUILayout.TextField(config.ServerAddress);

            EditorGUILayout.LabelField("REST Port:", EditorStyles.wordWrappedLabel);
            if (int.TryParse(EditorGUILayout.TextField(config.RestPort.ToString()), out var restPort))
                config.RestPort = restPort;

            EditorGUILayout.LabelField("Unity Port:", EditorStyles.wordWrappedLabel);
            if (int.TryParse(EditorGUILayout.TextField(config.UnityPort.ToString()), out var unityPort))
                config.UnityPort = unityPort;

            config.Protocol = EditorGUILayout.ToggleLeft("Use HTTPS", config.Protocol == "https://") ? "https://" : "http://";
            config.EnableSync = EditorGUILayout.ToggleLeft("Enable sync", config.EnableSync);
            config.EnableRenderstreaming = EditorGUILayout.ToggleLeft("Enable Renderstreaming", config.EnableRenderstreaming);

            if (GUILayout.Button("Save & Close"))
            {
                config.Save();
                Close();
            }
        }
    }
}