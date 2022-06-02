using System.Collections;
using System.Collections.Generic;
using Relive.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI
{
    [RequireComponent(typeof(Text))]
    public class LoadingText : MonoBehaviour
    {
        // public IDataProvider DataProvider;
        private Text text;

        // Start is called before the first frame update
        void OnEnable()
        {
            text = GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            // if (DataProvider != null && DataProvider.GetProgress() > -1)
            // {
            //     text.text = "Loading. Please wait " + DataProvider.GetProgress() + "%";
            // }

            if (DataManager.Instance.DataProvider != null && DataManager.Instance.DataProvider.GetProgress() > -1)
            {
                text.text = "Loading. Please wait " + DataManager.Instance.DataProvider.GetProgress() + "%";
            }
        }
    }
}
