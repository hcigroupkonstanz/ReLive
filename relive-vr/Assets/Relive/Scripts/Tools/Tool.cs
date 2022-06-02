using System.Collections.Generic;
using UnityEngine;
using Relive.Data;
using Relive.Tools.Parameter;
using System;

namespace Relive.Tools
{
    public abstract class ToolInstance
    {
        public delegate void ColorUpdateDelegate(ToolInstance instance);
        public event ColorUpdateDelegate OnColorUpdate;

        public Color Color;
        public Session Session;
        public bool IsActive;

        public abstract string GetResult();
        public void TriggerColorUpdate()
        {
            OnColorUpdate.Invoke(this);
        }
    }

    public abstract class Tool : MonoBehaviour
    {
        [Header("General Tool Properties")]
        public string Id = Guid.NewGuid().ToString();
        public string Name = "";

        public int MinEntities = 0;
        public int MaxEntities = 0; // 0 -> Entities not allowed | -1 -> infinite
        public int MinEvents = 0;
        public int MaxEvents = 0; // 0 -> Events not allowed | -1 -> infinite
        public Sprite Image;
        public string Properties = "";
        public bool RenderVisualization = true;
        public bool Singleton = false;
        public int NotebookIndex = 0;

        public readonly List<string> Entities = new List<string>();
        public readonly List<string> Events = new List<string>();
        public readonly Dictionary<string, ToolParameter> Parameters = new Dictionary<string, ToolParameter>();
        public readonly List<ToolInstance> Instances = new List<ToolInstance>();

        public delegate void ToolInstanceDelegate(ToolInstance instance);
        public event ToolInstanceDelegate InstanceAdded;
        public event ToolInstanceDelegate InstanceRemoved;

        // FIXME: events can't be called in children
        protected void InvokeInstanceAdded(ToolInstance instance) { InstanceAdded?.Invoke(instance); }
        protected void InvokeInstanceRemoved(ToolInstance instance) { InstanceRemoved?.Invoke(instance); }

        public abstract void RemoveEntity(string entityName);
        public abstract void AddEntity(string entityName);
        public abstract bool AddEntity(EntityGameObject entityGameObject); // Use only this function to add entities to the tool!!!

        public abstract void RemoveEvent(string eventName);
        public abstract void AddEvent(string eventName);
        public abstract bool AddEvent(EventGameObject eventGameObject);

        public abstract void AddInstance(Session session, Color color);
        public abstract void RemoveInstance(Session session);
    }
}

