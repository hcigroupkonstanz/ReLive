using UnityEngine;
using TMPro;
using Relive.Playback;
using System;

namespace Relive.UI.Playback
{
    public class PlaybackDurationTimeText : MonoBehaviour
    {
        private TextMeshProUGUI text;
        void OnEnable()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            text.text = TimeSpan.FromSeconds(PlaybackManager.Instance.PlaybackLengthSeconds).ToString(@"hh\:mm\:ss\:fff");
        }
    }
}
