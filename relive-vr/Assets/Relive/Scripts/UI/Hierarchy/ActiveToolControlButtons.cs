using UnityEngine;

namespace Relive.UI.Hierarchy
{
    public class ActiveToolControlButtons : MonoBehaviour
    {
        public HierarchyToolItem HierarchyToolItem;

        public void OnPropertyButtonClick()
        {
            ActiveToolPanelVR.Instance.PropertyWindowActivated(HierarchyToolItem);
        }

        public void OnDetachButtonClick()
        {
            ActiveToolPanelVR.Instance.DetachTool(HierarchyToolItem);
        }

        public void OnDeleteButtonClick()
        {
            ActiveToolPanelVR.Instance.DeleteTool(HierarchyToolItem);
        }
    }
}