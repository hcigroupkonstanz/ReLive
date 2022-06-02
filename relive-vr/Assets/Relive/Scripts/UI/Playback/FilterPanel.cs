using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Relive.UI.Playback
{
    public class FilterPanel : MonoBehaviour
    {
        public static FilterPanel Instance;

        public List<FilterItem> FilterItems = new List<FilterItem>();
        
        [HideInInspector]
        public PlaybackSession PlaybackSession;

        private float xSpacing = 0.325f;
        private float ySpacing = -0.0175f;
        private float indexSpacing = 0.0295f;
        private int index = 1;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            gameObject.SetActive(false);
        }

        void Update()
        {
            // Show filter panel left to playback panel
            transform.rotation = PlaybackSessionsPanelVR.Instance.transform.rotation;
            transform.position = PlaybackSessionsPanelVR.Instance.transform.position + (xSpacing * PlaybackSessionsPanelVR.Instance.transform.right) + ((ySpacing - index * indexSpacing) * PlaybackSessionsPanelVR.Instance.transform.up);
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(300, PlaybackSessionsPanelVR.Instance.ContentPanel.GetComponent<RectTransform>().rect.height);
        }

        public void OnBackButtonClicked()
        {
            CloseFilterPanel();
        }

        public void OpenFilterPanel(PlaybackSession playbackSession, int index)
        {
            this.index = index;
            this.PlaybackSession = playbackSession;
            foreach (FilterItem filterItem in FilterItems)
            {
                filterItem.PlaybackSession = playbackSession;
            }
            gameObject.SetActive(true);
        }

        public void CloseFilterPanel()
        {
            gameObject.SetActive(false);
        }
    }
}
