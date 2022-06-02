using UnityEngine;
using TMPro;

namespace Relive.UI.Playback
{
    public class PlaybackSpeedText : MonoBehaviour
    {
        private TextMeshProUGUI text;
        void OnEnable()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            text.text = Time.timeScale + "x";
        }
    }
}
