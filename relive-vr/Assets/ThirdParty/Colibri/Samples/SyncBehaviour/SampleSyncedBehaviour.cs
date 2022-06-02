using HCIKonstanz.Colibri.Sync;
using UnityEngine;

namespace HCIKonstanz.Colibri.Samples
{
    public class SampleSyncedBehaviour : SyncedBehaviour<SampleSyncedBehaviour>
    {
        public override string Channel => "SampleSyncedBehaviour";

        [Sync, SerializeField]
        private int RandomValue;

        [Sync]
        public string EditorTestString = "123";

        [Sync]
        private Vector3 Position
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        [Sync]
        public Vector3 Scale
        {
            get { return transform.localScale; }
            set { transform.localScale = value; }
        }

        [Sync]
        public Quaternion Rotation
        {
            get { return transform.localRotation; }
            set { transform.localRotation = value; }
        }


        [Sync]
        public Color Color
        {
            get { return _renderer ? _renderer.material.color : Color.black; }
            set
            {
                if (_renderer)
                    _renderer.material.color = value;
                LocalColor = value;
            }
        }

        public Color LocalColor;

        private Renderer _renderer;
        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            // apply updates from unity editor (for demo purposes)
            Color = LocalColor;
        }
    }
}
