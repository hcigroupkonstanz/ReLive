using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Relive.Data
{
    public enum SceneViewType
    {
        FreeFly,
        FollowEntity,
        ViewEntity,
        VR,
        Isometric
    }


    public class Notebook : Syncable<Notebook>
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("isPaused")]
        public bool IsPaused = false;

        [JsonProperty("playbackTimeSeconds")]
        public float PlaybackTimeSeconds;

        [JsonProperty("playbackSpeed")]
        public float PlaybackSpeed
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        [JsonProperty("sceneView")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SceneViewType SceneView;

        [JsonProperty("sceneViewOptions")]
        public Dictionary<string, JValue> SceneViewOptions = new Dictionary<string, JValue>();
    }
}
