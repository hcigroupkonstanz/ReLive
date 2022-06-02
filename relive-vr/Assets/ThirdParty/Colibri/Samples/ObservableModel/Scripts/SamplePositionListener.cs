using UniRx;
using UnityEngine;

namespace HCIKonstanz.Colibri.Samples
{
    [RequireComponent(typeof(SampleObservableModel))]
    public class SamplePositionListener : MonoBehaviour
    {
        private SampleObservableModel _so;

        private void OnEnable()
        {
            _so = GetComponent<SampleObservableModel>();
            _so.RemoteChange()
                .TakeUntilDisable(this)
                .Where(changes => changes.Contains("Position") || changes.Contains("Rotation"))
                .Subscribe(_ =>
                {
                    transform.localPosition = _so.Position;
                    transform.localRotation = _so.Rotation;
                });
        }

        private void Update()
        {
            _so.Position = transform.localPosition;
            _so.Rotation = transform.localRotation;
        }
    }
}
