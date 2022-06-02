using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowAllEntityNamesButton : MonoBehaviour
{
    public static ShowAllEntityNamesButton Instance;
    public TextMeshProUGUI ButtonText;
    public bool ShowAllEntityNames = false;

    void Awake()
    {
        Instance = this;
    }

    public void OnToggleValueChanged(bool enabled)
    {
        if (enabled)
        {
            ShowAllEntityNames = true;
            ButtonText.text = "Hide All Entity Names";
        }
        else
        {
            ShowAllEntityNames = false;
            ButtonText.text = "Show All Entity Names";
        }
    }
}
