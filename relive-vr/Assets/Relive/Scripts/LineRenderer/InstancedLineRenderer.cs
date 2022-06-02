using UnityEngine;
using UnityEngine.Rendering;

namespace Relive.Visualizations
{
    internal class InstancedLineRenderer
    {
        public Mesh LineMesh;
        public Material LineMaterial;

        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer argsBuffer;
        private Bounds _bounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f));
        private MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        public Transform Anchor { get; set; }

        private int lineCount;
        public int LineCount
        {
            get => lineCount;
            set
            {
                lineCount = value;
                CreateArgsBuffer();
                propBlock.SetInt("_lineCount", lineCount);
            }
        }

        private Texture linePositionTex;
        public Texture LinePositionTex
        {
            get => linePositionTex;
            set
            {
                linePositionTex = value;
                propBlock.SetTexture("_positions", linePositionTex);
                propBlock.SetInt("_texWidth", linePositionTex.width);
                propBlock.SetInt("_texHeight", linePositionTex.height);
            }
        }


        private Texture colorTex;
        public Texture ColorTex
        {
            get => colorTex;
            set
            {
                colorTex = value;
                propBlock.SetTexture("_color", colorTex);
            }
        }



        private float _transparency = 1f;
        public float Transparency
        {
            get => _transparency;
            set
            {
                _transparency = value;
                propBlock.SetFloat("_transparencyMultiplier", _transparency);
            }
        }

        public int StartIndex { set => propBlock.SetInt("_startIndex", value); }
        public int EndIndex { set => propBlock.SetInt("_endIndex", value); }


        internal InstancedLineRenderer()
        {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        internal void LateUpdate()
        {
            propBlock.SetMatrix("_localToWorld", Anchor.localToWorldMatrix);

            // TODO: determine based on renderer
            var isVisible = true;

            if (isVisible && lineCount > 0 && Transparency > 0 && LineMesh)
            {
                Graphics.DrawMeshInstancedIndirect(LineMesh, 0, LineMaterial,
                    _bounds, argsBuffer, 0, propBlock, ShadowCastingMode.Off,
                    false, 2, null, LightProbeUsage.Off);
            }
        }

        internal void OnDestroy()
        {
            argsBuffer.Release();
        }

        private void CreateArgsBuffer()
        {
            if (LineMesh)
            {
                args[0] = (uint)LineMesh.GetIndexCount(0);
                args[1] = (uint)lineCount;
                args[2] = (uint)LineMesh.GetIndexStart(0);
                args[3] = (uint)LineMesh.GetBaseVertex(0);
            }
            else
            {
                args[0] = args[1] = args[2] = args[3] = 0;
            }
            argsBuffer.SetData(args);
        }
    }
}
