using HCIKonstanz.Colibri.Networking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace HCIKonstanz.Colibri.Sync
{
    public abstract class ObservableModel<T> : MonoBehaviour
        where T : ObservableModel<T>
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    TriggerLocalChange("Id");
                }
            }
        }


        public class ChangesCallback
        {
            public IList<string> ChangedProperties;
            public T Model;
        };

        private static Subject<ChangesCallback> _allLocalChangesSubject = new Subject<ChangesCallback>();
        public static IObservable<ChangesCallback> AllLocalChanges() => _allLocalChangesSubject.AsObservable();

        private static Subject<ChangesCallback> _allRemoteChangesSubject = new Subject<ChangesCallback>();
        public static IObservable<ChangesCallback> AllRemoteChanges() => _allRemoteChangesSubject.AsObservable();

        public static IObservable<ChangesCallback> AllModelChange() => Observable.Merge(AllLocalChanges(), AllRemoteChanges());

        private Subject<string> _localChangeSubject = new Subject<string>();
        public IObservable<IList<string>> LocalChange() => _localChangeSubject
            .BatchFrame(10, FrameCountType.FixedUpdate)
            .Share()
            .AsObservable();

        private Subject<IList<string>> _remoteChangeSubject = new Subject<IList<string>>();
        public IObservable<IList<string>> RemoteChange() => _remoteChangeSubject;

        public IObservable<IList<string>> ModelChange() => Observable.Merge(LocalChange(), RemoteChange());

        // Allow small delay to set up properties
        private static Subject<T> _modelCreateSubject = new Subject<T>();
        public static IObservable<T> ModelCreated() => _modelCreateSubject.DelayFrame(1).AsObservable();


        private static Subject<T> _modelDestroySubject = new Subject<T>();
        public static IObservable<T> ModelDestroyed() => _modelDestroySubject.AsObservable();



        protected void TriggerLocalChange(string prop)
        {
            _localChangeSubject.OnNext(prop);
        }


        protected virtual void Awake()
        {
            ObservableManager<T>.Instance?.AddLocalModel(this as T);

            _modelCreateSubject.OnNext(this as T);
            LocalChange().Subscribe(changes => _allLocalChangesSubject.OnNext(new ChangesCallback
            {
                ChangedProperties = changes,
                Model = this as T
            }));

            RemoteChange().Subscribe(changes => _allRemoteChangesSubject.OnNext(new ChangesCallback
            {
                ChangedProperties = changes,
                Model = this as T
            }));
        }

        protected virtual void OnDestroy()
        {
            ObservableManager<T>.Instance?.RemoveLocalModel(this as T);

            _modelDestroySubject.OnNext(this as T);
            _localChangeSubject.OnCompleted();
            _localChangeSubject.Dispose();
            _remoteChangeSubject.OnCompleted();
            _remoteChangeSubject.Dispose();
        }


        protected abstract void ApplyRemoteUpdate(JObject updates);
        public void RemoteUpdate(JObject updates)
        {
            try
            {
                ApplyRemoteUpdate(updates);
                // transform json name pattern ("attributeOne") to c# pattern ("AttributeOne")
                var changes = updates.Properties().Select(k => k.Name).ToList();
                _remoteChangeSubject.OnNext(changes);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public abstract JObject ToJson();
        public JObject ToJson(IEnumerable<string> properties)
        {
            var fullJson = ToJson();
            var filteredJson = new JObject {
                { "Id", Id }
            };

            // transform c# name pattern ("AttributeOne") to json pattern ("attributeOne")
            var lowercaseProperties = properties.Distinct().Where(k => k != "Id");

            foreach (var prop in lowercaseProperties)
            {
                try
                {
                    filteredJson.Add(prop, fullJson[prop]);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }


            return filteredJson;
        }
    }
}
