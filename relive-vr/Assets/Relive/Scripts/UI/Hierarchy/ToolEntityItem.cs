using UnityEngine;
using TMPro;

namespace Relive.UI.Hierarchy
{
    public class ToolEntityItem : MonoBehaviour
    {
        public TextMeshProUGUI EntityNameText;
        public string Name
        {
            get { return entityName; }
            set
            {
                if (value == "" || value == null)
                {
                    EntityNameText.text = "--";
                }
                else
                {
                    EntityNameText.text = value;
                }
                entityName = value;
            }
        }

        private string entityName;

    }
}
