using System.Collections.Generic;
using Relive.Tools;
using UnityEngine;

namespace Relive.UI.Toolbox
{
    public class Toolbox : MonoBehaviour
    {
        public static Toolbox Instance;
        public List<Tool> Tools;

        void Awake()
        {
            Instance = this;
        }
    }
}
