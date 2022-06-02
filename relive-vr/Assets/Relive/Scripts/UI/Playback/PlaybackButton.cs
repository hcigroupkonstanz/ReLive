using Relive.Playback;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Playback
{
    [RequireComponent(typeof(Toggle))]
    public class PlaybackButton : MonoBehaviour
    {
        private Toggle toggle;
        private void Start()
        {
            toggle = GetComponent<Toggle>();
        }

        public void PlayPause()
        {
            if (toggle.isOn)
            {
                PlaybackManager.Instance.Play();
            }
            else
            {
                PlaybackManager.Instance.Pause();
            }
        }
    }
}