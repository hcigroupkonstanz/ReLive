using UnityEngine;

namespace Relive.Data
{
    public abstract class Syncable<T>
        where T : Syncable<T>
    {
        public delegate void UpdateHandler(T t);
        public event UpdateHandler OnLocalUpdate;
        public event UpdateHandler OnRemoteUpdate;

        // local or remote
        public event UpdateHandler OnUpdate;

        public void UpdateLocal()
        {
            OnLocalUpdate?.Invoke(this as T);
            OnUpdate?.Invoke(this as T);
        }

        public void UpdateRemote()
        {
            OnRemoteUpdate?.Invoke(this as T);
            OnUpdate?.Invoke(this as T);
        }
    }
}
