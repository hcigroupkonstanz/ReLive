using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace HCIKonstanz.Colibri
{
    public class SyncConfiguration
    {
        private static string ConfigFile { get => Path.Combine(Application.persistentDataPath, "colibri_config"); }
        private static SyncConfiguration _current;
        public static SyncConfiguration Current
        {
            get
            {
#if UNITY_EDITOR
                if (_current == null)
                {
                    if (File.Exists(ConfigFile))
                        _current = JsonConvert.DeserializeObject<SyncConfiguration>(File.ReadAllText(ConfigFile));
                    else
                    {
                        _current = new SyncConfiguration();
                        _current.Save();
                    }
                }

                return _current;
#else
                if (_current == null)
                {

                    var config = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
                    if (File.Exists(config))
                    {
                        Debug.Log("Loading configuration");
                        _current = JsonConvert.DeserializeObject<SyncConfiguration>(File.ReadAllText(config));
                    }
                    else
                    {
                        Debug.LogError("No config file found!");
                        _current = new SyncConfiguration
                        {
                            RestPort = 55210,
                            UnityPort = 55212,
                            Protocol = "http://",
                            ServerAddress = "localhost",
                            EnableRenderstreaming = true,
                            EnableSync = true
                        };
                    }
                }

                return _current;
#endif
            }
        }

        public int RestPort = 55210;
        public int UnityPort = 55212;
        public string Protocol = "http://";
        public string ServerAddress = "";
        public bool EnableSync = true;
        public bool EnableRenderstreaming = true;

        public void Save()
        {
#if UNITY_EDITOR
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(_current));
#endif
        }
    }
}
