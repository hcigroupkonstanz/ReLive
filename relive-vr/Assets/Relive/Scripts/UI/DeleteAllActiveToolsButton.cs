using System.Collections;
using System.Collections.Generic;
using Relive.Tools;
using Relive.UI.Hierarchy;
using UnityEngine;

public class DeleteAllActiveToolsButton : MonoBehaviour
{
    public void OnDeleteButtonClick()
    {
        ActiveToolPanelVR.Instance.DeleteTools();
    }
}
