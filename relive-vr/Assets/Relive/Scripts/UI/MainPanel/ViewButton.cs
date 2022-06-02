using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.MainPanel
{
    public class ViewButton : MonoBehaviour
    {
        // Start is called before the first frame update

        public ViewGroup ViewGroup;
        public Toggle Toggle;

        void Start()
        {
            ViewGroup.Subscribe(this);
            Toggle = GetComponent<Toggle>();
            Toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(); });
        }

        private void ToggleValueChanged()
        {
            if (Toggle.isOn)
            {
                ViewGroup.OnViewSelected(this);
            }
            else
            {
                ViewGroup.OnViewExit(this);
            }
        }
    }
}