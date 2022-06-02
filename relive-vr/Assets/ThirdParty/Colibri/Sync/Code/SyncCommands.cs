using HCIKonstanz.Colibri.Networking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HCIKonstanz.Colibri.Sync
{
    [DefaultExecutionOrder(-1000)]
    public class SyncCommands
    {
        private static SyncCommands _instance;
        public static SyncCommands Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SyncCommands();
                return _instance;
            }
        }


        private WebServerConnection _connection;
        private readonly Dictionary<string, List<Action<bool>>> _boolListeners = new Dictionary<string, List<Action<bool>>>();
        private readonly Dictionary<string, List<Action<int>>> _intListeners = new Dictionary<string, List<Action<int>>>();
        private readonly Dictionary<string, List<Action<float>>> _floatListeners = new Dictionary<string, List<Action<float>>>();
        private readonly Dictionary<string, List<Action<string>>> _stringListeners = new Dictionary<string, List<Action<string>>>();
        private readonly Dictionary<string, List<Action<Vector2>>> _vector2Listeners = new Dictionary<string, List<Action<Vector2>>>();
        private readonly Dictionary<string, List<Action<Vector3>>> _vector3Listeners = new Dictionary<string, List<Action<Vector3>>>();
        private readonly Dictionary<string, List<Action<Quaternion>>> _quaternionListeners = new Dictionary<string, List<Action<Quaternion>>>();
        private readonly Dictionary<string, List<Action<Color>>> _colorListeners = new Dictionary<string, List<Action<Color>>>();
        private readonly Dictionary<string, List<Action<bool[]>>> _boolArrayListeners = new Dictionary<string, List<Action<bool[]>>>();
        private readonly Dictionary<string, List<Action<int[]>>> _intArrayListeners = new Dictionary<string, List<Action<int[]>>>();
        private readonly Dictionary<string, List<Action<float[]>>> _floatArrayListeners = new Dictionary<string, List<Action<float[]>>>();
        private readonly Dictionary<string, List<Action<string[]>>> _stringArrayListeners = new Dictionary<string, List<Action<string[]>>>();
        private readonly Dictionary<string, List<Action<Vector2[]>>> _vector2ArrayListeners = new Dictionary<string, List<Action<Vector2[]>>>();
        private readonly Dictionary<string, List<Action<Vector3[]>>> _vector3ArrayListeners = new Dictionary<string, List<Action<Vector3[]>>>();
        private readonly Dictionary<string, List<Action<Quaternion[]>>> _quaternionArrayListeners = new Dictionary<string, List<Action<Quaternion[]>>>();
        private readonly Dictionary<string, List<Action<Color[]>>> _colorArrayListeners = new Dictionary<string, List<Action<Color[]>>>();
        private readonly Dictionary<string, List<Action<JToken>>> _jsonListeners = new Dictionary<string, List<Action<JToken>>>();

        private readonly Dictionary<string, List<Action<JArray>>> _modelInitListeners = new Dictionary<string, List<Action<JArray>>>();
        private readonly Dictionary<string, List<Action<JObject>>> _modelUpdateListeners = new Dictionary<string, List<Action<JObject>>>();
        private readonly Dictionary<string, List<Action<JObject>>> _modelDeleteListeners = new Dictionary<string, List<Action<JObject>>>();


        public SyncCommands()
        {
            _connection = WebServerConnection.Instance;
            _connection.OnMessageReceived += OnServerMessage;
        }


        private void OnServerMessage(string channel, string command, JToken data)
        {
            switch (command)
            {
                case "bool":
                    Invoke(channel, _boolListeners, data.Value<bool>());
                    break;
                case "int":
                    Invoke(channel, _intListeners, data.Value<int>());
                    break;
                case "float":
                    Invoke(channel, _floatListeners, data.Value<float>());
                    break;
                case "string":
                    Invoke(channel, _stringListeners, data.Value<string>());
                    break;
                case "vector2":
                    Invoke(channel, _vector2Listeners, data.ToVector2());
                    break;
                case "vector3":
                    Invoke(channel, _vector3Listeners, data.ToVector3());
                    break;
                case "quaternion":
                    Invoke(channel, _quaternionListeners, data.ToQuaternion());
                    break;
                case "color":
                    Invoke(channel, _colorListeners, data.ToColor());
                    break;

                case "bool[]":
                    Invoke(channel, _boolArrayListeners, data.Select(x => (bool)x).ToArray());
                    break;
                case "int[]":
                    Invoke(channel, _intArrayListeners, data.Select(x => (int)x).ToArray());
                    break;
                case "float[]":
                    Invoke(channel, _floatArrayListeners, data.Select(x => (float)x).ToArray());
                    break;
                case "string[]":
                    Invoke(channel, _stringArrayListeners, data.Select(x => (string)x).ToArray());
                    break;
                case "vector2[]":
                    Invoke(channel, _vector2ArrayListeners, data.Select(x => x.ToVector2()).ToArray());
                    break;
                case "vector3[]":
                    Invoke(channel, _vector3ArrayListeners, data.Select(x => x.ToVector3()).ToArray());
                    break;
                case "quaternion[]":
                    Invoke(channel, _quaternionArrayListeners, data.Select(x => x.ToQuaternion()).ToArray());
                    break;
                case "color[]":
                    Invoke(channel, _colorArrayListeners, data.Select(x => x.ToColor()).ToArray());
                    break;

                case "JSON":
                    Invoke(channel, _jsonListeners, data);
                    break;

                case "modelInit":
                    if (_modelInitListeners.ContainsKey(channel))
                    {
                        foreach (var listener in _modelInitListeners[channel].ToArray())
                        {
                            if (data is JArray jdata)
                            {
                                listener.Invoke(jdata);
                            }
                        }
                        _modelInitListeners.Remove(channel);
                    }
                    break;

                case "modelUpdate":
                    // cause TypeLoadExceptions sometimes??
                    //Invoke<JObject>(channel, _modelUpdateListeners, (JObject)data);
                    if (_modelUpdateListeners.ContainsKey(channel))
                    {
                        foreach (var listener in _modelUpdateListeners[channel].ToArray())
                        {
                            if (data is JObject jdata)
                                listener.Invoke(jdata);
                        }
                    }
                    break;

                case "modelDelete":
                    // cause TypeLoadExceptions sometimes??
                    //Invoke<JObject>(channel, _modelDeleteListeners, (JObject)data);
                    if (_modelDeleteListeners.ContainsKey(channel))
                    {
                        foreach (var listener in _modelDeleteListeners[channel].ToArray())
                        {
                            listener.Invoke(data.Value<JObject>());
                        }
                    }
                    break;
            }
        }

        private void Invoke<T>(string channel, Dictionary<string, List<Action<T>>> listeners, T val)
        {
            if (listeners.ContainsKey(channel) && val != null)
            {
                foreach (var listener in listeners[channel].ToArray())
                    listener.Invoke(val);
            }
        }





        /*
         *  Sending data
         */

        public void SendData(string channel, bool data) => _connection.SendCommand(channel, "bool", data);
        public void SendData(string channel, int data) => _connection.SendCommand(channel, "int", data);
        public void SendData(string channel, float data) => _connection.SendCommand(channel, "float", data);
        public void SendData(string channel, string data) => _connection.SendCommand(channel, "string", data);
        public void SendData(string channel, Vector2 data) => _connection.SendCommand(channel, "vector2", data.ToJson());
        public void SendData(string channel, Vector3 data) => _connection.SendCommand(channel, "vector3", data.ToJson());
        public void SendData(string channel, Quaternion data) => _connection.SendCommand(channel, "quaternion", data.ToJson());
        public void SendData(string channel, Color data) => _connection.SendCommand(channel, "color", data.ToJson());
        public void SendData(string channel, bool[] data) => _connection.SendCommand(channel, "bool[]", new JArray(data));
        public void SendData(string channel, int[] data) => _connection.SendCommand(channel, "int[]", new JArray(data));
        public void SendData(string channel, float[] data) => _connection.SendCommand(channel, "float[]", new JArray(data));
        public void SendData(string channel, string[] data) => _connection.SendCommand(channel, "string[]", new JArray(data));
        public void SendData(string channel, Vector2[] data) => _connection.SendCommand(channel, "vector2[]", new JArray(data.Select(x => x.ToJson())));
        public void SendData(string channel, Vector3[] data) => _connection.SendCommand(channel, "vector3[]", new JArray(data.Select(x => x.ToJson())));
        public void SendData(string channel, Quaternion[] data) => _connection.SendCommand(channel, "quaternion[]", new JArray(data.Select(x => x.ToJson())));
        public void SendData(string channel, Color[] data) => _connection.SendCommand(channel, "color[]", new JArray(data.Select(x => x.ToJson())));
        public void SendData(string channel, JToken data) => _connection.SendCommand(channel, "JSON", data);

        //public void SendModelUpdate(string channel, JObject data) => _connection.SendCommand(channel, "modelUpdate", data);
        //public void SendModelDelete(string channel, int id) => _connection.SendCommand(channel, "modelDelete", new JObject { { "Id", id } });
        //public void SendModelDelete(string channel, string id) => _connection.SendCommand(channel, "modelDelete", new JObject { { "Id", id } });




        /*
         *  Listeners
         */

        private void AddListener<T>(string channel, Dictionary<string, List<Action<T>>> listeners, Action<T> listener)
        {
            if (!listeners.ContainsKey(channel))
                listeners.Add(channel, new List<Action<T>>());
            listeners[channel].Add(listener);
        }

        private void RemoveListener<T>(string channel, Dictionary<string, List<Action<T>>> listeners, Action<T> listener)
        {
            if (listeners.ContainsKey(channel))
            {
                var list = listeners[channel];
                list.Remove(listener);
                if (list.Count == 0)
                    listeners.Remove(channel);
            }
        }

        public void AddBoolListener(string channel, Action<bool> listener) => AddListener(channel, _boolListeners, listener);
        public void RemoveBoolListener(string channel, Action<bool> listener) => RemoveListener(channel, _boolListeners, listener);

        public void AddIntListener(string channel, Action<int> listener) => AddListener(channel, _intListeners, listener);
        public void RemoveIntListener(string channel, Action<int> listener) => RemoveListener(channel, _intListeners, listener);

        public void AddFloatListener(string channel, Action<float> listener) => AddListener(channel, _floatListeners, listener);
        public void RemoveFloatListener(string channel, Action<float> listener) => RemoveListener(channel, _floatListeners, listener);

        public void AddStringListener(string channel, Action<string> listener) => AddListener(channel, _stringListeners, listener);
        public void RemoveStringListener(string channel, Action<string> listener) => RemoveListener(channel, _stringListeners, listener);

        public void AddVector2Listener(string channel, Action<Vector2> listener) => AddListener(channel, _vector2Listeners, listener);
        public void RemoveVector2Listener(string channel, Action<Vector2> listener) => RemoveListener(channel, _vector2Listeners, listener);

        public void AddVector3Listener(string channel, Action<Vector3> listener) => AddListener(channel, _vector3Listeners, listener);
        public void RemoveVector3Listener(string channel, Action<Vector3> listener) => RemoveListener(channel, _vector3Listeners, listener);

        public void AddQuaternionListener(string channel, Action<Quaternion> listener) => AddListener(channel, _quaternionListeners, listener);
        public void RemoveQuaternionListener(string channel, Action<Quaternion> listener) => RemoveListener(channel, _quaternionListeners, listener);

        public void AddColorListener(string channel, Action<Color> listener) => AddListener(channel, _colorListeners, listener);
        public void RemoveColorListener(string channel, Action<Color> listener) => RemoveListener(channel, _colorListeners, listener);

        public void AddBoolArrayListener(string channel, Action<bool[]> listener) => AddListener(channel, _boolArrayListeners, listener);
        public void RemoveBoolArrayListener(string channel, Action<bool[]> listener) => RemoveListener(channel, _boolArrayListeners, listener);

        public void AddIntArrayListener(string channel, Action<int[]> listener) => AddListener(channel, _intArrayListeners, listener);
        public void RemoveIntArrayListener(string channel, Action<int[]> listener) => RemoveListener(channel, _intArrayListeners, listener);

        public void AddFloatArrayListener(string channel, Action<float[]> listener) => AddListener(channel, _floatArrayListeners, listener);
        public void RemoveFloatArrayListener(string channel, Action<float[]> listener) => RemoveListener(channel, _floatArrayListeners, listener);

        public void AddStringArrayListener(string channel, Action<string[]> listener) => AddListener(channel, _stringArrayListeners, listener);
        public void RemoveStringArrayListener(string channel, Action<string[]> listener) => RemoveListener(channel, _stringArrayListeners, listener);

        public void AddVector2ArrayListener(string channel, Action<Vector2[]> listener) => AddListener(channel, _vector2ArrayListeners, listener);
        public void RemoveVector2ArrayListener(string channel, Action<Vector2[]> listener) => RemoveListener(channel, _vector2ArrayListeners, listener);

        public void AddVector3ArrayListener(string channel, Action<Vector3[]> listener) => AddListener(channel, _vector3ArrayListeners, listener);
        public void RemoveVector3ArrayListener(string channel, Action<Vector3[]> listener) => RemoveListener(channel, _vector3ArrayListeners, listener);

        public void AddQuaternionArrayListener(string channel, Action<Quaternion[]> listener) => AddListener(channel, _quaternionArrayListeners, listener);
        public void RemoveQuaternionArrayListener(string channel, Action<Quaternion[]> listener) => RemoveListener(channel, _quaternionArrayListeners, listener);

        public void AddColorArrayListener(string channel, Action<Color[]> listener) => AddListener(channel, _colorArrayListeners, listener);
        public void RemoveColorArrayListener(string channel, Action<Color[]> listener) => RemoveListener(channel, _colorArrayListeners, listener);


        public void AddJSONListener(string channel, Action<JToken> listener) => AddListener(channel, _jsonListeners, listener);
        public void RemoveJSONListener(string channel, Action<JToken> listener) => RemoveListener(channel, _jsonListeners, listener);

        public void AddModelListener(IModelListener listener)
        {
            var modelChannel = listener.GetChannelName() + "Model";
            AddListener(modelChannel + "Init", _modelInitListeners, listener.OnSyncInitialized);
            AddListener(modelChannel, _modelUpdateListeners, listener.OnSyncUpdate);
            AddListener(modelChannel, _modelDeleteListeners, listener.OnSyncDelete);

            // fetch initial data
            _connection.SendCommand(modelChannel, "modelFetch", null);
        }

        public void RemoveModelListener(IModelListener listener)
        {
            var modelChannel = listener.GetChannelName() + "Model";
            RemoveListener(modelChannel + "Init", _modelInitListeners, listener.OnSyncInitialized);
            RemoveListener(modelChannel, _modelUpdateListeners, listener.OnSyncUpdate);
            RemoveListener(modelChannel, _modelDeleteListeners, listener.OnSyncDelete);
        }

        public void SendModelUpdate(string channel, JObject model)
        {
            var modelChannel = channel + "Model";
            _connection.SendCommand(modelChannel, "modelUpdate", model);
        }

        public void SendModelDelete(string channel, JObject deletedObj)
        {
            var modelChannel = channel + "Model";
            _connection.SendCommand(modelChannel, "modelDelete", deletedObj);
        }
    }
}
