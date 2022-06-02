using System;
using Relive.Playback;
using UnityEngine;
using TMPro;
using Relive.Sync;

namespace Relive.UI.Playback
{
    public class PlaybackTimeText : MonoBehaviour
    {
        private TextMeshProUGUI text;
        void OnEnable()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            try
            {
                text.text = TimeSpan.FromSeconds(NotebookManager.Instance.Notebook.PlaybackTimeSeconds).ToString(@"hh\:mm\:ss\:fff");
            }
            catch { } // ignore errors
        }
    }
}
