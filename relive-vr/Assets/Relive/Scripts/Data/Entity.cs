using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Relive.Data
{
    public class Entity
    {
        public string entityId;
        public string parentEntityId;
        public string sessionId;
        public string entityType;
        public string name;
        public string space;
        public string filePaths;
        public string[] attributes;

        public List<Attachment> attachments;

        // Optional attributes (temporary)
        public string highlightColor;
        public float maxSpeed = 0f;
        public float averageSpeed = 0f;
        public float stdevSpeed = 0f;

        [JsonProperty("isVisible")]
        public bool IsVisible = true;
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

        public string GetData(string key)
        {
            if (_additionalData.ContainsKey(key))
                return _additionalData[key].ToString();
            return GetType().GetField(key)?.GetValue(this)?.ToString();
        }
    }
}