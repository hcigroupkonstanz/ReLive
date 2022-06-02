using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relive.Visualizations
{
    public class Visualizer : MonoBehaviour
    {

        List<IVisualizable> visualizables = new List<IVisualizable>();

        void OnPostRender()
        {
            foreach (IVisualizable visualizable in visualizables)
            {
                visualizable.Visualize();
            }
        }

        public void registerVisualizable(IVisualizable visualizable)
        {
            visualizables.Add(visualizable);
        }

        public void deregisterVisualizable(IVisualizable visalizable)
        {
            visualizables.Remove(visalizable);
        }

    }
}
