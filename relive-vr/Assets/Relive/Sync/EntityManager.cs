using HCIKonstanz.Colibri.Core;
using HCIKonstanz.Colibri.Sync;
using Newtonsoft.Json.Linq;
using Relive.Data;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Relive.Sync
{
    public class EntityManager : SingletonBehaviour<EntityManager>, IModelListener
    {
        public readonly List<Entity> Entities = new List<Entity>();
        public readonly BehaviorSubject<bool> IsInitialized = new BehaviorSubject<bool>(false);

        private bool ignoreNextUpdate = false;

        protected override void Awake()
        {
            base.Awake();
            StudyManager.Instance.RegisterStudyListener(this);
            EntityGameObject.OnEntityChanged += HandleLocalEntityUpdate;
        }

        public string GetChannelName() => "entities";

        public void OnSyncInitialized(JArray initialEntity)
        {
            foreach (var ev in initialEntity)
            {
                Entities.Add(ev.ToObject<Entity>());
            }

            Debug.Log($"Fetched {Entities.Count} entities from server");
            IsInitialized.OnNext(true);
        }


        public void OnSyncUpdate(JObject update)
        {
            var entityUpdate = update.ToObject<Entity>();
            var localEntity = Entities.FirstOrDefault(e => e.sessionId == entityUpdate.sessionId && e.entityId == entityUpdate.entityId);

            if (localEntity != null)
            {
                // update local entity
                var entityGo = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity == localEntity);
                if (entityGo != null && entityGo.Entity.IsVisible != entityUpdate.IsVisible)
                {
                    ignoreNextUpdate = true;
                    entityGo.Hide = !entityUpdate.IsVisible;
                }
            }
            else
            {
                Debug.Log($"Adding new entity {entityUpdate.entityId} due to remote update");
                Entities.Add(entityUpdate);
            }
        }

        public void OnSyncDelete(JObject deletedObject)
        {
            var deletedEntity = deletedObject.ToObject<Entity>();
            var removedEntities = Entities.RemoveAll(e => e.entityId == deletedEntity.entityId && e.sessionId == deletedEntity.sessionId);
            Debug.Log($"Removed {removedEntities} entities due to remote upate");
        }

        private void HandleLocalEntityUpdate(EntityGameObject entityGo)
        {
            if (ignoreNextUpdate)
            {
                ignoreNextUpdate = false;
                return;
            }

            SyncCommands.Instance.SendModelUpdate(GetChannelName(), new JObject
            {
                { "sessionId", entityGo.Entity.sessionId },
                { "entityId", entityGo.Entity.entityId },
                { "isVisible", entityGo.Entity.IsVisible },
            });
        }
    }
}
