using TMPro;
using UniRx;
using UnityEngine;

namespace HCIKonstanz.Colibri.Samples
{
    [RequireComponent(typeof(TextMeshPro))]
    public class SampleTextListener : MonoBehaviour
    {
        private SampleObservableModel _so;
        private TextMeshPro _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshPro>();
            _so = GetComponentInParent<SampleObservableModel>();

            _so.ModelChange()
                .TakeUntilDestroy(this)
                .Where(changes => changes.Contains("TestString"))
                .Subscribe(_ =>
                {
                    _text.text = _so.TestString;
                });

            _text.text = _so.TestString;
        }
    }
}
