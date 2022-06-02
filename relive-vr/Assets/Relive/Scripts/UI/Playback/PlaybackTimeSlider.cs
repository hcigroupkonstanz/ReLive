using Relive.Playback;
using Relive.Sync;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Playback
{
    public class PlaybackTimeSlider : MonoBehaviour
    {
        private Slider slider;
        private bool interactable = false;
        private bool userInput = false;

        void OnEnable()
        {
            slider = GetComponent<Slider>();
        }

        void Update()
        {
            // slider.interactable = PlaybackManager.Instance.LoadedSession != null;
            slider.interactable = PlaybackManager.Instance.PlaybackSessionCount != 0;

            // This is only a workaround. Not final
            if (slider.interactable && !interactable)
            {
                // print("Max Value: " + slider.maxValue);
                slider.maxValue = PlaybackManager.Instance.PlaybackLengthSeconds;
            }
            interactable = slider.interactable;
            if (!userInput)
            {
                // print("Value: " + PlaybackManager.Instance.PlaybackTimeSeconds);
                slider.value = NotebookManager.Instance.Notebook.PlaybackTimeSeconds;
            }
        }

        public void OnSliderValueChanged(float value)
        {
            if (userInput)
            {
                PlaybackManager.Instance.JumpTo(value);
            }
        }

        public void OnSlidePointerDown()
        {
            
            if (slider.interactable)
            {
                userInput = true;
                PlaybackManager.Instance.Pause();
                PlaybackManager.Instance.JumpTo(slider.value);
            }
        }

        public void OnSliderPointerUp()
        {
            if (slider.interactable)
            {
                userInput = false;
                PlaybackManager.Instance.Play();
            }
        }
    }
}
