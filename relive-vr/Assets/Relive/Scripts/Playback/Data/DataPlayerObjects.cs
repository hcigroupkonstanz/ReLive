using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relive.Playback.Data
{
    public class DataPlayerObjects : MonoBehaviour
    {
        public static DataPlayerObjects Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}
