using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Relive.Data
{
    public class State
    {
        public string parentId;
        public string sessionId;
        public string stateType;
        public long timestamp;
        public string status;

        public JsonVector3 position = null;
        public JsonQuaternion rotation;
        public JsonVector3 scale;

        public string color;
        public string shader;
        public float speed = -1f;
        public float distanceMoved;
        public string orientation;


        /**
         *  Additional / optional data within states is saved here
         */
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();
        public bool ContainsData(string key) => _additionalData.ContainsKey(key);
        public string GetData(string key)
        {
            if (_additionalData.ContainsKey(key))
                return _additionalData[key].ToString();
            return GetType().GetField(key)?.GetValue(this)?.ToString();
        }
        public ICollection<string> GetAdditionalFields() => _additionalData.Keys;
    }
}