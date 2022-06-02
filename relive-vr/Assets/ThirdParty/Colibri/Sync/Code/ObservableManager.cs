using HCIKonstanz.Colibri.Core;
using HCIKonstanz.Colibri.Networking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace HCIKonstanz.Colibri.Sync
{
    public abstract class ObservableManager<T> : SingletonBehaviour<ObservableManager<T>>
        where T : ObservableModel<T>
    {
        public T Template;

        protected abstract string Channel { get; }

        private static readonly System.Random _random = new System.Random();
        private readonly Dictionary<string, Action<JToken>> _registeredCommands = new Dictionary<string, Action<JToken>>();

        private BehaviorSubject<bool> _isInitialized = new BehaviorSubject<bool>(false);
        public IObservable<bool> Initialized => _isInitialized.Where(x => x).First();

        protected WebServerConnection _connection;
        protected readonly List<T> _currentModels = new List<T>();

        private bool _isAddingModel;

        protected override void Awake()
        {
            base.Awake();
            Action<JToken> addCmd = obj =>
            {
                var objId = obj["Id"];
                if (objId != null)
                {
                    var id = objId.Value<int>();
                    var model = AddRemoteModel(id);
                    model.RemoteUpdate(obj as JObject);
                }
            };

            AddCommand("add", addCmd);
            AddCommand("update", addCmd);
            AddCommand("request", data =>
            {
                var vals = data as JArray ?? new JArray();
                foreach (var m in vals)
                {
                    var mId = m["id"];
                    if (mId != null)
                    {
                        var id = mId.Value<int>();
                        var model = AddRemoteModel(id);
                        model.RemoteUpdate(m as JObject);
                    }
                }

                _isInitialized.OnNext(true);
            });

            AddCommand("remove", obj =>
            {
                if (obj["Id"] != null)
                    RemoveModel(obj["Id"].Value<int>());
            });
        }


        protected virtual void OnEnable()
        {
            _connection = WebServerConnection.Instance;
            _connection.OnConnected += InitRemote;
            _connection.OnDisconnected += TerminateRemote;

            if (_connection.Status == ConnectionStatus.Connected)
                InitRemote();

            if (Template == null)
            {
                Debug.LogError("No template set");
                enabled = false;
            }
        }

        protected virtual void OnDisable()
        {
            _connection.OnConnected -= InitRemote;
            _connection.OnDisconnected -= TerminateRemote;

            _connection.SendCommand("channel", "deregister", new JObject { { "channel", Channel } });

            TerminateRemote();
        }


        protected void AddCommand(string command, Action<JToken> action)
        {
            if (_registeredCommands.ContainsKey(command))
                Debug.LogWarning($"Overwriting command {command} in {Channel}!");

            _registeredCommands[command] = action;
        }

        protected void RemoveCommand(string command)
        {
            _registeredCommands.Remove(command);
        }


        private void InitRemote()
        {
            if (_isInitialized.Value)
                return;

            _connection.SendCommand("channel", "register", new JObject { { "channel", Channel } });
            _connection.SendCommand(Channel, "request", null);
            _connection.OnMessageReceived += OnServerMessage;
        }


        private void TerminateRemote()
        {
            if (!_isInitialized.Value)
                return;

            var modelsCopy = _currentModels.ToArray();
            foreach (var model in modelsCopy)
            {
                RemoveModel(model);
            }

            _connection.OnMessageReceived -= OnServerMessage;
            _isInitialized.OnNext(false);
        }

        private void OnServerMessage(string channel, string command, JToken payload)
        {
            if (channel == Channel)
            {
                if (_registeredCommands.ContainsKey(command))
                    _registeredCommands[command](payload);
                else
                {
                    Debug.LogWarning($"Unknown command {command} in {channel}");
                    // register empty command to suppress future warnings
                    AddCommand(command, obj => { });
                }
            }
        }


        protected virtual T AddRemoteModel(int id)
        {
            var model = Get(id);
            if (model)
                return model;

            _isAddingModel = true;
            model = Instantiate(Template);
            model.Id = id;
            RegisterModel(model);

            _isAddingModel = false;

            return model;
        }

        private void RegisterModel(T model)
        {
            model.name = $"{typeof(T).Name} ({model.Id})";
            _currentModels.Add(model);

            model.LocalChange().Subscribe(async changes =>
                {
                    await _connection.Connected;
                    _connection.SendCommand(Channel, "update", model.ToJson(changes));
                });
        }


        protected virtual void RemoveModel(int id)
        {
            var model = Get(id);
            if (model)
                RemoveModel(model);
        }


        protected virtual void RemoveModel(T model)
        {
            _currentModels.Remove(model);
            if (model != null)
                Destroy(model.gameObject);
        }


        public IEnumerable<T> Get()
        {
            return _currentModels;
        }

        public T Get(int id)
        {
            return _currentModels.FirstOrDefault(m => m.Id == id);
        }


        public void AddLocalModel(T model)
        {
            if (!_isAddingModel)
            {
                model.Id = _random.Next();
                _connection.SendCommand(Channel, "add", new JObject { { "Id", model.Id } });
                _connection.SendCommand(Channel, "update", model.ToJson());
                RegisterModel(model);
            }
        }

        public void RemoveLocalModel(T model)
        {
            if (_currentModels.Contains(model))
            {
                _connection.SendCommand(Channel, "remove", new JObject { { "Id", model.Id } });
                RemoveModel(model);
            }
        }
    }
}
