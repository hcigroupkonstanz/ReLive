using Relive.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Sessions
{
    public class StudyItem : MonoBehaviour
    {
        [HideInInspector]
        public StudyPanel StudyPanel;
        [HideInInspector]
        public SessionPanel SessionPanel;
        public string Study
        {
            get => SessionTitle.text;
            set => SessionTitle.text = value;
        }
        public RemoteDataProvider DataProvider;

        public Button SessionPlayButton;
        public Text SessionTitle;

        void OnEnable()
        {
            SessionPlayButton.onClick.AddListener(OnSessionPlayButtonClicked);
        }

        void OnDisable()
        {
            SessionPlayButton.onClick.RemoveListener(OnSessionPlayButtonClicked);
        }

        public void OnSessionPlayButtonClicked()
        {
            DataProvider.ActivateStudy(Study);
            SessionPanel.DisplaySessions();
            StudyPanel.Hide();
        }
    }
}
