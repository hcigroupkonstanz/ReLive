using Newtonsoft.Json;
using System.Collections.Generic;

namespace Relive.Data
{
    public class Session : Syncable<Session>
    {
        public string sessionId;
        public long startTime;
        public long endTime;
        public string name;
        public string description;

        [JsonProperty("color")]
        public string Color;

        [JsonProperty("isActive")]
        public bool IsActive;
        [JsonProperty("isExpanded")]
        public bool IsExpanded;

        [JsonProperty("eventFilters")]
        public Dictionary<string, bool> EventFilters = new Dictionary<string, bool>();
    }
}