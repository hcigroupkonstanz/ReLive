using System;
using UnityEngine;

namespace ReLive.Utils
{
    [DisallowMultipleComponent]
    public abstract class SingletonBehaviour<T> : MonoBehaviour
        where T : SingletonBehaviour<T>
    {
        private static bool _createdInstance;
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (!_instance && !_createdInstance)
                {
                    try
                    {
                        // try searching scene for available instance
                        _instance = FindObjectOfType<T>();

                        // No singleton found in scene, create a new one
                        if (!_instance)
                            _instance = new GameObject($"[{typeof(T).Name}]").AddComponent<T>();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                _createdInstance = true;

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this as T;
        }
    }
}
