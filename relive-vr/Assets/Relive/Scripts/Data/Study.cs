using Newtonsoft.Json;

namespace Relive.Data
{
    public class Study : Syncable<Study>
    {
        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "dbName")]
        public string DbName;

        [JsonProperty(PropertyName = "isActive")]
        public bool IsActive;
    }
}
