using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relive.RenderStreaming
{
    public class Debug_MouseOver : MonoBehaviour
    {
        public bool IsMouseOver;

        void Update()
        {
            GetComponent<Renderer>().material.color = IsMouseOver ? Color.red : Color.blue;
        }
    }
}
