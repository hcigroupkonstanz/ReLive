using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReLive.Events
{
    public class TracedButtonEvents : MonoBehaviour
    {
        public string ButtonName;
        private Button button;

        // Start is called before the first frame update
        private void Awake()
        {
            button = GetComponent<Button>();
            if (!button) return;
            button.onClick.AddListener(TriggerClickEvent);
            if (ButtonName == "") ButtonName = gameObject.name;
        }

        public void TriggerClickEvent()
        {
            ReliveEvent.Log(ReliveEventType.Click, new Dictionary<string, object>
            {
                { "buttonName", ButtonName }
            });
        }
    }
}
