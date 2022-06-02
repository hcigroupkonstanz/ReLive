using ReLive.Core;
using System;

namespace ReLive.Entities
{
    public class Entity : Traceable
    {
        public readonly string EntityId;
        private readonly string sessionId;

        public string ParentEntityId
        {
            set => SetData("parentEntityId", value);
        }

        public EntityType EntityType
        {
            set => SetData("entityType", Enum.GetName(typeof(EntityType), value).ToLower());
        }

        public EntitySpace Space
        {
            set => SetData("space", Enum.GetName(typeof(EntitySpace), value).ToLower());
        }

        protected override string GetChannel() => "entities";
        protected override string GetId() => EntityId;
        protected override void SetIdData()
        {
            SetData("entityId", EntityId);
            SetData("sessionId", sessionId);
        }

        public Entity(string entityId, string sessionId, string name)
        {
            EntityId = entityId;
            this.sessionId = sessionId;

            SetData("name", name);

            InitializeTracing(EntityId, sessionId, StateType.Entity);
        }
    }
}
