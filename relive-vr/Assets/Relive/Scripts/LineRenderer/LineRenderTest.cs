using System;
using UnityEngine;

namespace Relive.Visualizations
{
    [RequireComponent(typeof(FastLineRenderer))]
    public class LineRenderTest : MonoBehaviour
    {
        public Vector3[] LinePoints = new Vector3[0];
        public Color[] LineColors = new Color[0];
        public int StartIndex = 0;
        public int EndIndex = 1;

        private FastLineRenderer lineRenderer;

        private void Start()
        {
            lineRenderer = GetComponent<FastLineRenderer>();
            lineRenderer.SetPoints(LinePoints, LineColors);
            EndIndex = LinePoints.Length;
        }

        private void Update()
        {
            StartIndex = Math.Min(Math.Max(0, StartIndex), LinePoints.Length - 1);
            EndIndex = Math.Min(Math.Max(0, EndIndex), LinePoints.Length - 1);
            lineRenderer.ShowLineSegment(StartIndex, EndIndex);
        }
    }
}
