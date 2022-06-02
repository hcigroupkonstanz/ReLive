using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Relive.Visualizations
{
    public class FastLineRenderer : MonoBehaviour
    {
        public Material LineMaterial;
        public Mesh LineMesh;
        private readonly List<InstancedLineRenderer> lineRenderers = new List<InstancedLineRenderer>();

        private Texture2D linePositions;
        private Texture2D lineColors;

        public void SetPoints(Vector3[] linePoints, Color[] colors)
        {
            Cleanup();

            if (linePoints.Length < 2)
            {
                Debug.LogError("Not enough points for line");
                return;
            }

            // TODO: split into multiple renderers & textures if there are too many points (> SystemInfo.maxTextureSize^2)
            if (linePoints.Length > SystemInfo.maxTextureSize * SystemInfo.maxTextureSize)
                Debug.LogWarning($"Too many points, system is limited to {SystemInfo.maxTextureSize} * {SystemInfo.maxTextureSize} points!");

            // colours do not support negative values - we therefore need to determine a local2world matrix
            // to transform all values into positive ones
            Vector3 minPosition = Vector3.zero;
            for (int i = 0; i < linePoints.Length; i++)
            {
                minPosition.x = Mathf.Min(minPosition.x, linePoints[i].x);
                minPosition.y = Mathf.Min(minPosition.y, linePoints[i].y);
                minPosition.z = Mathf.Min(minPosition.z, linePoints[i].z);
            }
            transform.position = minPosition;

            // now transform all vectors into positive ones
            for (int i = 0; i < linePoints.Length; i++)
                linePoints[i] -= minPosition;


            // textures are limited in size, but can use 2D array
            // -> lines above threshold are moved into next row on texture
            var texWidth = Mathf.Min(linePoints.Length, SystemInfo.maxTextureSize);
            var texHeight = (int)Mathf.Ceil((float)linePoints.Length / texWidth);

            linePositions = new Texture2D(texWidth, texHeight, TextureFormat.RGB9e5Float, false, true);
            linePositions.filterMode = FilterMode.Point;
            linePositions.wrapMode = TextureWrapMode.Clamp;

            // colors not initialized yet
            lineColors = new Texture2D(texWidth, texHeight, TextureFormat.RGB9e5Float, false, true);
            lineColors.filterMode = FilterMode.Point;
            lineColors.wrapMode = TextureWrapMode.Clamp;

            var texPixelCount = texWidth * texHeight;
            Color[] texPositions = new Color[texPixelCount];
            Color[] texColors = new Color[texPixelCount];
            for (int i = 0; i < texPixelCount; i++)
            {
                if (i < linePoints.Length)
                {
                    texPositions[i] = new Color(linePoints[i].x, linePoints[i].y, linePoints[i].z);
                    texColors[i] = colors[i];
                }
                else
                {
                    texPositions[i] = Color.clear;
                    texColors[i] = Color.clear;
                }
            }


            linePositions.SetPixels(texPositions);
            linePositions.Apply();
            lineColors.SetPixels(texColors);
            lineColors.Apply();

            var renderer = new InstancedLineRenderer
            {
                LineMaterial = LineMaterial,
                LineMesh = LineMesh,
                // lines have at least 2 segments (array of 2 points is 1 line) -> -1
                LineCount = linePoints.Length - 1,
                LinePositionTex = linePositions,
                ColorTex = lineColors,
                Anchor = transform
            };

            lineRenderers.Add(renderer);
        }

        public void ShowLineSegment(int startIndex, int endIndex)
        {
            if (lineRenderers.Count == 0)
                return;

            // enable all
            foreach (var renderer in lineRenderers)
            {
                renderer.StartIndex = 0;
                renderer.EndIndex = renderer.LineCount;
            }

            // disable start/end specifically
            lineRenderers.First().StartIndex = startIndex;
            lineRenderers.Last().EndIndex = endIndex;
        }

        private void LateUpdate()
        {
            foreach (var renderer in lineRenderers)
                renderer.LateUpdate();
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            foreach (var renderer in lineRenderers)
                renderer.OnDestroy();
            lineRenderers.Clear();

            if (linePositions)
                Destroy(linePositions);
            if (lineColors)
                Destroy(lineColors);
        }
    }
}
