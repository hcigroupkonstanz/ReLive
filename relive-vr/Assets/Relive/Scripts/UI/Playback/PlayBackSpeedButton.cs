using Relive.Data;
using Relive.Sync;
using Relive.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Playback
{
    [RequireComponent(typeof(SlideUp), typeof(Toggle))]
    public class PlayBackSpeedButton : MonoBehaviour
    {
        public TextMeshProUGUI text;
        private SlideUp slideAnimation;
        private Notebook notebook;

        private void Start()
        {
            slideAnimation = GetComponent<SlideUp>();

            Toggle toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(toggle); });

            notebook = NotebookManager.Instance.Notebook;
            notebook.OnUpdate += OnPlaybackUpdate;
        }

        private void ToggleValueChanged(Toggle toggle)
        {
            if (toggle.isOn)
            {
                slideAnimation.Activate();
            }
            else
            {
                slideAnimation.Deactivate();
            }
        }

        private void OnPlaybackUpdate(Notebook notebook)
        {
            text.text = notebook.PlaybackSpeed + "x";
        }

        public void ChangePlaybackSpeed(float value)
        {
            notebook.PlaybackSpeed = value;
            notebook.UpdateLocal();
        }
    }
}