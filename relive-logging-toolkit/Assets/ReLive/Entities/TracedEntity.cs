using ReLive.Sessions;
using ReLive.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;

namespace ReLive.Entities
{
    [DefaultExecutionOrder(-999)]
    public abstract class TracedEntity : MonoBehaviour
    {
        // in frames. lower values = more logging
        // TODO: better name / description
        public IntReactiveProperty SampleFrequency = new IntReactiveProperty(1);

        public bool AutoGenerateEntityId = true;

        [ConditionalField("AutoGenerateEntityId", true)]
        public string EntityId;

        public TracedEntity ParentEntity;

        public bool TraceLocalPosition = false;
        public bool TraceLocalRotation = false;
        public GameObject LocalRelativeToGameObject;

        // TODO: Not yet implemented
        // [ConditionalField("AutoGenerateSessionId", true)]
        // public bool AutoIncrementEntityId = false;

        protected Entity entity;

        protected abstract void InitializeEntity();

        private async void Awake()
        {
            /*
             * Entity setup
             */
            if (AutoGenerateEntityId || string.IsNullOrEmpty(EntityId))
                EntityId = Guid.NewGuid().ToString();

            //var sessionId = await SessionController.Instance.SessionStart;
            var sessionId = await SessionController.Instance.SessionStart;
            if (gameObject == null) return;
            entity = new Entity(EntityId, sessionId, name);

            // try to search automatically for parent entities
            // TODO: GetComponentInParent also looks at self???
            if (!ParentEntity && transform.parent != null)
                ParentEntity = transform.parent.GetComponentInParent<TracedEntity>();

            // TODO: may not be initialized yet
            if (ParentEntity)
                entity.ParentEntityId = ParentEntity.EntityId;

            /*
             * Iterate through all attached components to find [Logged] attributes
             */
            foreach (var component in GetComponents<Component>())
            {
                var props = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs.Where(a => a is TracedAttribute))
                    {
                        // TODO: add serialization based on prop.PropertyType?
                        // TODO: compile lambda
                        component
                            .ObserveEveryValueChanged(c => prop.GetValue(c))
                            .TakeUntilDestroy(this)
                            .Subscribe(val => entity.AddTrace(prop.Name, val));
                    }
                }

                var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    object[] attrs = field.GetCustomAttributes(true);
                    foreach (object attr in attrs.Where(a => a is TracedAttribute))
                    {
                        // TODO: serialize field value based on field.FieldType?
                        // TODO: compile lambda (?)
                        component
                            .ObserveEveryValueChanged(c => field.GetValue(c))
                            .TakeUntilDestroy(this)
                            .Subscribe(val => entity.AddTrace(field.Name, val));
                    }
                }
            }

            InitializeEntity();
            entity.ScheduleChanges();
        }

        private async void OnEnable()
        {
            await SessionController.Instance.SessionStart;

            entity.AddTrace("status", Enum.GetName(typeof(StateStatus), StateStatus.Active).ToLower());

            Observable.EveryUpdate()
                // TODO: this should react to changes in SampleFrequency
                .SampleFrame(SampleFrequency.Value)
                .Subscribe(v => entity.ScheduleChanges());

            transform.ObserveEveryValueChanged(t => t.position)
                .TakeUntilDisable(this)
                .Subscribe(t =>
                {
                    entity.AddTrace("position", ToJson(t));

                    if (TraceLocalPosition)
                    {
                        if (LocalRelativeToGameObject)
                        {
                            Vector3 localPositionRelative = LocalRelativeToGameObject.transform.InverseTransformPoint(t);
                            entity.AddTrace("localPosition", ToJson(localPositionRelative));
                        }
                        else
                        {
                            entity.AddTrace("localPosition", ToJson(transform.localPosition));
                        }
                    }
                });

            transform.ObserveEveryValueChanged(t => t.rotation)
                .TakeUntilDisable(this)
                .Subscribe(t =>
                {
                    entity.AddTrace("rotation", ToJson(t));

                    if (TraceLocalRotation)
                    {
                        if (LocalRelativeToGameObject)
                        {
                            Quaternion localRotationRelative = Quaternion.Inverse(LocalRelativeToGameObject.transform.rotation) * t;
                            entity.AddTrace("localRotation", ToJson(localRotationRelative));
                        }
                        else
                        {
                            entity.AddTrace("localRotation", ToJson(transform.localRotation));
                        }
                    }
                });

            transform.ObserveEveryValueChanged(t => t.localScale)
                .TakeUntilDisable(this)
                .Subscribe(t => entity.AddTrace("scale", ToJson(t)));
        }

        private static JObject ToJson(Vector3 v)
        {
            var json = new JObject();
            json.Add("x", v.x);
            json.Add("y", v.y);
            json.Add("z", v.z);
            return json;
        }

        private static JObject ToJson(Quaternion q)
        {
            var json = new JObject();
            json.Add("x", q.x);
            json.Add("y", q.y);
            json.Add("z", q.z);
            json.Add("w", q.w);
            return json;
        }

        private void OnDisable()
        {
            entity.AddTrace("status", Enum.GetName(typeof(StateStatus), StateStatus.Inactive).ToLower());
            entity.ScheduleChanges();
        }

        private void OnDestroy()
        {
            entity.AddTrace("status", Enum.GetName(typeof(StateStatus), StateStatus.Deleted).ToLower());
            // TODO: could overlap with OnDisable()
            entity.ScheduleChanges();
        }


        // TODO: rename to "WatchProperty" (but how to call RemovePropertyListener()?)
        // TODO: overload with only string, use reflection to get property getter
        public void AddPropertyListener(MonoBehaviour origin, string property, Func<string> listener)
        {
            this.ObserveEveryValueChanged(_ => listener(), FrameCountType.FixedUpdate)
                .TakeUntilDestroy(origin)
                .Subscribe(val => LogProperty(property, val));
        }

        public void RemovePropertyListener()
        {
            // TODO
        }

        public async void LogProperty(string property, object value)
        {
            await SessionController.Instance.SessionStart;
            if (entity == null) return;
            entity.AddTrace(property, value);
        }

        public async void AddEntityProperty(string property, object value)
        {
            await SessionController.Instance.SessionStart;
            entity.SetData(property, value);
            entity.ScheduleChanges();
        }
    }
}
