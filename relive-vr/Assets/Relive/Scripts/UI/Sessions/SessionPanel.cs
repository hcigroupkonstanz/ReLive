using System.Collections.Generic;
using Relive.Data;
using UnityEngine;

namespace Relive.UI.Sessions
{
    public class SessionPanel : MonoBehaviour
    {
        public static SessionPanel Instance;
        public SessionItem SessionItemPrefab;
        public GameObject SessionContent;
        public bool Visible
        {
            get { return gameObject.activeSelf; }
        }

        public IDataProvider DataProvider;
        private List<Session> sessions = new List<Session>();
        private List<SessionItem> sessionItems = new List<SessionItem>();

        void Awake()
        {
            Instance = this;
        }

        // public void DisplaySessions(IDataProvider dataProvider)
        // {
        //     this.DataProvider = dataProvider;
        //     DisplaySessions();
        // }

        public async void DisplaySessions()
        {
            DataProvider = DataManager.Instance.DataProvider;
            ClearCatalog();
            // gameObject.SetActive(true);
            sessions = await DataProvider.GetSessions();

            // populate session list
            foreach (Session session in sessions)
            {
                SessionItem sessionItem = Instantiate(SessionItemPrefab, SessionContent.transform);
                sessionItem.SessionPanel = this;
                sessionItem.Session = session;
                sessionItem.DataProvider = DataProvider;
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
            foreach (SessionItem sessionRow in sessionItems)
            {
                Destroy(sessionRow.gameObject);
            }
            sessionItems = new List<SessionItem>();
        }
    }
}
