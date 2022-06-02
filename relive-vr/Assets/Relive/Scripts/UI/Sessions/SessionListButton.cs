using Relive.Playback;
using Relive.UI.Input;
using UnityEngine;

namespace Relive.UI.Sessions
{
    public class SessionListButton : MonoBehaviour
    {
        public SessionPanel SessionSelection;

        public void OnButtonClicked()
        {
            if (SessionSelection.Visible)
            {
                PlaybackManager.Instance.Play();
                SessionSelection.Hide();
                InputManagerScreen.Instance.PlaybackTimeSlider.interactable = true;
            }
            else
            {
                PlaybackManager.Instance.Pause();
                SessionSelection.DisplaySessions();
                InputManagerScreen.Instance.PlaybackTimeSlider.interactable = false;
            }

        }
    }
}
