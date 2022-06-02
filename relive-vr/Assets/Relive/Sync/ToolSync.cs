using Newtonsoft.Json.Linq;
using Relive.Tools;
using Relive.UI.Hierarchy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HCIKonstanz.Colibri.Sync;
using System;
using UniRx;
using Newtonsoft.Json;
using Relive.Tools.Parameter;
using Cysharp.Threading.Tasks;

namespace Relive.Sync
{
    [Serializable]
    public class ToolPrefab
    {
        public string Type;
        public Tool Prefab;
    }

    [Serializable]
    class SyncedToolInstance
    {
        [JsonProperty("sessionId")]
        public string SessionId;
        [JsonProperty("color")]
        public string Color;
    }

    [Serializable]
    class SyncedTool
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("minEvents")]
        public int MinEvents;
        [JsonProperty("maxEvents")]
        public int MaxEvents;
        [JsonProperty("minEntities")]
        public int MinEntities;
        [JsonProperty("maxEntities")]
        public int MaxEntities;
        [JsonProperty("renderVisualization")]
        public bool RenderVisualization;
        [JsonProperty("notebookIndex")]
        public int NotebookIndex;
        [JsonProperty("parameters")]
        public Dictionary<string, JObject> Parameters;
        [JsonProperty("events")]
        public string[] Events;
        [JsonProperty("entities")]
        public string[] Entities;
        [JsonProperty("instances")]
        public SyncedToolInstance[] Instances;
    }

    public class ToolSync : MonoBehaviour, IModelListener
    {
        // Workaround because dictionarys do not work for editing in Unity Editor
        public List<ToolPrefab> ToolPrefabs = new List<ToolPrefab>();
        private ActiveToolPanelVR toolManager;

        // FIXME: workaround since e.g. adding new tool from remote causes local 'add tool' event
        //private bool ignoreNextEvent = false;
        // FIXME2: must be per tool
        private readonly Dictionary<Tool, bool> ignoreNextEvent = new Dictionary<Tool, bool>();

        private void OnEnable()
        {
            Observable.WhenAll(
                StateManager.Instance.IsInitialized.First(v => v),
                EntityManager.Instance.IsInitialized.First(v => v),
                SessionManager.Instance.IsInitialized.First(v => v)
            )
                .DelayFrame(1) // FIXME: workaround since scene/entities needs some time to be set up
                .Subscribe(_ =>
            {
                SyncCommands.Instance.AddModelListener(this);
                toolManager = ActiveToolPanelVR.Instance;
                toolManager.OnToolAdded += OnLocalToolAdded;
                toolManager.OnToolDeleted += OnLocalToolDeleted;
            });
        }

        private void OnDisable()
        {
            SyncCommands.Instance.RemoveModelListener(this);
            toolManager.OnToolAdded -= OnLocalToolAdded;
            toolManager.OnToolDeleted -= OnLocalToolDeleted;
        }

        public string GetChannelName() => "tools";

        public void OnSyncInitialized(JArray initialTools)
        {
            foreach (JObject tool in initialTools)
                OnSyncUpdate(tool);

            Debug.Log($"Fetched {initialTools.Count} tools from server");
        }

        public void OnSyncUpdate(JObject updatedTool)
        {
            var remoteTool = updatedTool.ToObject<SyncedTool>();
            var localTool = toolManager.Tools.FirstOrDefault(t => t.Id == remoteTool.Id);

            if (localTool != null)
            {
                UpdateLocalTool(localTool, remoteTool);
            }
            else
            {
                Debug.Log("Creating new tool from remote source");
                CreateLocalTool(remoteTool);
            }

            toolManager.ReorderTools();
        }

        private void UpdateLocalTool(Tool localTool, SyncedTool remoteTool)
        {
            localTool.RenderVisualization = remoteTool.RenderVisualization;
            localTool.NotebookIndex = remoteTool.NotebookIndex;

            // add new instances
            foreach (var remoteInstance in remoteTool.Instances)
            {
                var localInstance = localTool.Instances.FirstOrDefault(i => i.Session.sessionId == remoteInstance.SessionId);
                if (localInstance != null)
                {
                    if (ColorUtility.TryParseHtmlString(remoteInstance.Color, out var col))
                    {
                        if (localInstance.Color != col)
                        {
                            localInstance.Color = col;
                            localInstance.TriggerColorUpdate();
                        }
                    }
                }
                else
                {
                    var session = SessionManager.Instance.Sessions.FirstOrDefault(s => s.sessionId == remoteInstance.SessionId);
                    if (ColorUtility.TryParseHtmlString(remoteInstance.Color, out var col))
                        localTool.AddInstance(session, col);
                    else
                        localTool.AddInstance(session, Color.black);
                }
            }
            // remove old instances
            var deleteInstances = localTool.Instances.Where(local => !remoteTool.Instances.Any(remote => remote.SessionId == local.Session.sessionId));
            foreach (var di in deleteInstances.ToArray())
                localTool.RemoveInstance(di.Session);

            foreach (var kv in remoteTool.Parameters)
            {
                if (localTool.Parameters.ContainsKey(kv.Key))
                    ApplyToolParameter(localTool.Parameters[kv.Key], JsonToParameter(kv.Value), localTool);
                else
                {
                    var parameter = JsonToParameter(kv.Value);
                    localTool.Parameters.Add(kv.Key, parameter);

                    // FIXME: code duplication & terrible code
                    if (toolManager.Tools.Any(t => localTool)) // already added -> register listeners, as 'RegisterToolListener()' won't run
                    {
                        IObservable<bool> listener = null;
                        if (parameter is BoolToolParameter bp)
                            listener = localTool.ObserveEveryValueChanged(_ => bp.Value).TakeUntilDestroy(localTool);
                        else if (parameter is NumberToolParameter np)
                            listener = localTool.ObserveEveryValueChanged(_ => np.Value).Select(_ => true).TakeUntilDestroy(localTool);
                        else if (parameter is ColorToolParameter cp)
                            listener = localTool.ObserveEveryValueChanged(_ => cp.Value).Select(_ => true).TakeUntilDestroy(localTool);
                        else if (parameter is DropdownToolParameter dp)
                            listener = localTool.ObserveEveryValueChanged(_ => dp.Value).Select(_ => true).TakeUntilDestroy(localTool);
                        else
                            Debug.LogError("Unknown tool parameter");

                        if (listener != null)
                            listener
                                .Skip(1) // ignore first change -> start value
                                .BatchFrame(10, FrameCountType.FixedUpdate)
                                .Subscribe(_ =>
                                {
                                    // tool already destroyed
                                    if (!localTool)
                                        return;

                                    OnLocalUpdate(localTool);
                                });
                    }
                }
            }

            // FIXME: must be *after* parameters due to PropertyTool
            var deletedEntities = localTool.Entities.Where(local => !remoteTool.Entities.Any(remote => remote == local));
            foreach (var e in deletedEntities.ToArray())
                localTool.RemoveEntity(e);

            foreach (var entity in remoteTool.Entities)
                localTool.AddEntity(entity);

            var deletedEvents = localTool.Events.Where(local => !remoteTool.Events.Any(remote => remote == local));
            foreach (var e in deletedEvents.ToArray())
                localTool.RemoveEvent(e);

            foreach (var entity in remoteTool.Events)
                localTool.AddEvent(entity);
        }

        private void CreateLocalTool(SyncedTool origin)
        {
            var toolPrefab = ToolPrefabs.FirstOrDefault(t => t.Type == origin.Type);
            if (toolPrefab != null)
            {
                var tool = Instantiate(toolPrefab.Prefab);
                tool.Id = origin.Id;
                UpdateLocalTool(tool, origin);

                RegisterToolListeners(tool);
                ignoreNextEvent[tool] = true;
                toolManager.AddTool(tool, false);

                // FIXME: terrible workaround
                if (origin.Type == "camera")
                    Destroy(tool.gameObject);
            }
            else
            {
                Debug.LogWarning("Tried to instantiate unknown tool " + origin.Name);
            }
        }


        private async void OnLocalToolAdded(Tool tool)
        {
            if (ignoreNextEvent.ContainsKey(tool) && ignoreNextEvent[tool])
            {
                ignoreNextEvent[tool] = false;
                return;
            }

            // wait one frame so that local code can add the entities etc.
            await UniTask.DelayFrame(1);

            ignoreNextEvent.Add(tool, false);
            RegisterToolListeners(tool);
            OnLocalUpdate(tool);
        }

        private SyncedTool SerializeTool(Tool tool)
        {
            var paramDictionary = new Dictionary<string, JObject>();
            foreach (var kv in tool.Parameters)
                paramDictionary.Add(kv.Key, ParameterToJson(kv.Value));

            return new SyncedTool
            {
                Type = GetToolType(tool),
                Id = tool.Id,
                MinEvents = tool.MinEvents,
                MaxEvents = tool.MaxEvents,
                MinEntities = tool.MinEntities,
                MaxEntities = tool.MaxEntities,
                Name = tool.Name,
                RenderVisualization = tool.RenderVisualization,
                Instances = tool.Instances.Select(i => new SyncedToolInstance
                {
                    Color = "#" + ColorUtility.ToHtmlStringRGB(i.Color),
                    SessionId = i.Session.sessionId
                }).ToArray(),
                Entities = tool.Entities.ToArray(),
                Events = tool.Events.ToArray(),
                NotebookIndex = tool.NotebookIndex,
                Parameters = paramDictionary
            };
        }

        private string GetToolType(Tool tool)
        {
            if (tool is AngleTool)
                return "angle";
            if (tool is FrustumTool)
                return "frustum";
            if (tool is MeasuringTool)
                return "distance";
            if (tool is PropertyTool)
                return "property";
            if (tool is TrailTool)
                return "trail";
            if (tool is CameraTool)
                return "camera";
            if (tool is EventTimerTool)
                return "eventTimer";

            Debug.LogError("Unknown tool type for tool: " + tool.Name);
            return "unknown";
        }

        private void RegisterToolListeners(Tool tool)
        {
            var listeners = new List<IObservable<bool>>();
            listeners.Add(this.ObserveEveryValueChanged(_ => tool.Entities.Count).Select(_ => true).TakeUntilDestroy(tool));
            listeners.Add(this.ObserveEveryValueChanged(_ => tool.Events.Count).Select(_ => true).TakeUntilDestroy(tool));
            listeners.Add(this.ObserveEveryValueChanged(_ => tool.Instances.Count).Select(_ => true).TakeUntilDestroy(tool));

            // listen for color changes in instances
            tool.InstanceAdded += instance =>
                instance.OnColorUpdate += _ => OnLocalUpdate(tool);
            foreach (var instance in tool.Instances)
                instance.OnColorUpdate += _ => OnLocalUpdate(tool);

            // listen for any parameter changes
            foreach (var parameter in tool.Parameters.Values)
            {
                IObservable<bool> listener = null;
                if (parameter is BoolToolParameter bp)
                    listener = tool.ObserveEveryValueChanged(_ => bp.Value).TakeUntilDestroy(tool);
                else if (parameter is NumberToolParameter np)
                    listener = tool.ObserveEveryValueChanged(_ => np.Value).Select(_ => true).TakeUntilDestroy(tool);
                else if (parameter is ColorToolParameter cp)
                    listener = tool.ObserveEveryValueChanged(_ => cp.Value).Select(_ => true).TakeUntilDestroy(tool);
                else if (parameter is DropdownToolParameter dp)
                    listener = tool.ObserveEveryValueChanged(_ => dp.Value).Select(_ => true).TakeUntilDestroy(tool);
                else
                    Debug.LogError("Unknown tool parameter");

                listeners.Add(listener);
            }

            Observable
                .Merge(listeners)
                .Skip(1) // ignore first change -> start value
                .BatchFrame(20, FrameCountType.FixedUpdate)
                .Subscribe(_ =>
                {
                    // tool already destroyed
                    if (!tool)
                        return;

                    OnLocalUpdate(tool);
                });
        }

        private void OnLocalUpdate(Tool tool)
        {
            if (ignoreNextEvent[tool])
            {
                ignoreNextEvent[tool] = false;
                return;
            }

            var syncedTool = SerializeTool(tool);
            var jTool = JObject.FromObject(syncedTool);
            SyncCommands.Instance.SendModelUpdate(GetChannelName(), jTool);
        }

        private void OnLocalToolDeleted(Tool tool)
        {
            if (ignoreNextEvent[tool])
            {
                ignoreNextEvent[tool] = false;
                return;
            }

            ignoreNextEvent.Remove(tool);
            SyncCommands.Instance.SendModelDelete(GetChannelName(), new JObject {
                { "id", tool.Id }
            });
        }

        public void OnSyncDelete(JObject deletedObject)
        {
            var toolId = deletedObject["id"].Value<string>();
            var toolName = deletedObject["name"].Value<string>();
            Debug.Log($"Removing tool {toolId} remotely");

            var tool = toolManager.Tools.FirstOrDefault(t => t.Id == toolId);
            if (tool != null)
            {
                ignoreNextEvent[tool] = true;
                toolManager.DeleteTool(tool.Id);
                ignoreNextEvent.Remove(tool);
            }

            var toolUi = toolManager.UiTools.FirstOrDefault(t => t.HackName == toolName);
            if (toolUi != null)
            {
                toolManager.UiTools.Remove(toolUi);
                Destroy(toolUi.gameObject);
            }
        }

        private void ApplyToolParameter(ToolParameter p, ToolParameter update, Tool tool)
        {
            if (p is BoolToolParameter bp)
            {
                var u = update as BoolToolParameter;
                if (bp.Value != u.Value)
                {
                    ignoreNextEvent[tool] = true;
                    bp.ChangeValue(u.Value);
                }
            }
            else if (p is ColorToolParameter cp)
            {
                var u = update as ColorToolParameter;
                if (cp.Value != u.Value)
                {
                    ignoreNextEvent[tool] = true;
                    cp.ChangeColor(u.Value);
                }
            }
            else if (p is DropdownToolParameter dp)
            {
                var u = update as DropdownToolParameter;
                if (dp.Value != u.Value)
                {
                    ignoreNextEvent[tool] = true;
                    dp.Value = u.Value;
                }
            }
            else if (p is NumberToolParameter np)
            {
                var u = update as NumberToolParameter;
                if (np.Value != u.Value)
                {
                    ignoreNextEvent[tool] = true;
                    np.ChangeValue(u.Value);
                }
            }
            else
            {
                Debug.LogError("Unknown tool parameter type for name: " + p.Name);
            }
        }

        private ToolParameter JsonToParameter(JObject p)
        {
            var type = p["type"].Value<string>();
            var name = p["name"].Value<string>();
            var value = p["value"];

            switch (type)
            {
                case "bool":
                    return new BoolToolParameter { Name = name, Value = value.Value<bool>() };

                case "color":
                    ColorUtility.TryParseHtmlString(value.Value<string>(), out var color);
                    return new ColorToolParameter { Name = name, Value = color };

                case "dropdown":
                    return new DropdownToolParameter
                    {
                        Name = name,
                        Value = value.Value<int>(),
                        Options = p["options"].ToObject<List<string>>()
                    };

                case "number":
                    return new NumberToolParameter
                    {
                        Name = name,
                        Value = value.Value<float>(),
                        MinValue = p["minValue"].Value<float>(),
                        StepSize = p["stepSize"].Value<float>(),
                        Unit = p["unit"].Value<string>(),
                    };

                default:
                    Debug.LogError("Unknown tool parameter type " + type);
                    return null;
            }
        }

        private JObject ParameterToJson(ToolParameter p)
        {
            var jsonParameter = new JObject { { "name", p.Name } };

            if (p is BoolToolParameter bp)
            {
                jsonParameter["type"] = "bool";
                jsonParameter["value"] = bp.Value;
            }
            else if (p is ColorToolParameter cp)
            {
                jsonParameter["type"] = "color";
                jsonParameter["value"] = cp.Value.ToJson();
            }
            else if (p is DropdownToolParameter dp)
            {
                jsonParameter["type"] = "dropdown";
                jsonParameter["value"] = dp.Value;
                jsonParameter["otpions"] = JObject.FromObject(dp.Options);
            }
            else if (p is NumberToolParameter np)
            {
                jsonParameter["type"] = "number";
                jsonParameter["value"] = np.Value;
                jsonParameter["minValue"] = np.MinValue;
                jsonParameter["maxValue"] = np.MaxValue;
                jsonParameter["stepSize"] = np.StepSize;
                jsonParameter["unit"] = np.Unit;
            }
            else
            {
                Debug.LogError("Unknown tool parameter type for name: " + p.Name);
            }


            return jsonParameter;
        }
    }
}
