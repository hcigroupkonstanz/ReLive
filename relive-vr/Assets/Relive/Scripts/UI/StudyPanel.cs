using Relive.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Relive.UI.Sessions
{
    public class StudyPanel : MonoBehaviour
    {
        public static StudyPanel Instance;
        public StudyItem StudyItemPrefab;
        public GameObject SessionContent;
        public SessionPanel SessionPanel;

        public bool Visible
        {
            get { return gameObject.activeSelf; }
        }

        private List<StudyItem> sessionItems = new List<StudyItem>();

        private void Awake()
        {
            Instance = this;
        }

        public async void DisplayStudies(RemoteDataProvider dataProvider)
        {
            ClearCatalog();
            // gameObject.SetActive(true);
            var studies = await dataProvider.GetStudies();

            // populate session list
            foreach (var study in studies)
            {
                StudyItem sessionItem = Instantiate(StudyItemPrefab, SessionContent.transform);
                sessionItem.SessionPanel = SessionPanel;
                sessionItem.StudyPanel = this;
                sessionItem.Study = study.Name;
                sessionItem.DataProvider = dataProvider;
                sessionItems.Add(sessionItem);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            // hide
            gameObject.SetActive(false);
        }

        private void ClearCatalog()
        {
            foreach (StudyItem sessionRow in sessionItems)
            {
                Destroy(sessionRow.gameObject);
            }
            sessionItems = new List<StudyItem>();
        }
    }
}
