using System;
using System.Collections.Generic;
using Relive.Data;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace Relive.UI.Sessions
{
    public class SessionPanelVR : MonoBehaviour
    {
        public static SessionPanelVR Instance;
        public GameObject SessionContent;
        public SessionItemVR SessionItemVRPrefab;
        private IDataProvider dataProvider;
        private List<Session> sessions = new List<Session>();
        public List<SessionItemVR> SessionItemsVR = new List<SessionItemVR>();

        private LayoutElement layoutElement;

        private void Awake()
        {
            Instance = this;
            layoutElement = GetComponent<LayoutElement>();
        }

        
        public void Deactivate()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            BoxCollider[] BoxColliders = GetComponentsInChildren<BoxCollider>();
            BoxColliders.Concat(GetComponents<BoxCollider>());
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = false;
            }

            layoutElement.ignoreLayout = true;
        }

        public void Activate()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;
            BoxCollider[] BoxColliders = GetComponentsInChildren<BoxCollider>();
            BoxColliders.Concat(GetComponents<BoxCollider>());
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = true;
            }
            DisplaySessions();

            layoutElement.ignoreLayout = false;
        }

        public async void DisplaySessions()
        {
            ClearCatalog();
            dataProvider = DataManager.Instance.DataProvider;
            if (dataProvider == null)
                return;
            sessions = await dataProvider.GetSessions();

            // populate session list
            foreach (Session session in sessions)
            {
                SessionItemVR sessionItemVR = Instantiate(SessionItemVRPrefab, SessionContent.transform);
                sessionItemVR.Session = session;
                SessionItemsVR.Add(sessionItemVR);
            }
        }

        private void ClearCatalog()
        {
            foreach (SessionItemVR sessionItem in SessionItemsVR)
            {
                Destroy(sessionItem.gameObject);
            }
            SessionItemsVR = new List<SessionItemVR>();
        }
    }

}
