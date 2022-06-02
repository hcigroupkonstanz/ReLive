using UnityEngine;
using TMPro;

public class LoadingTextMeshPro : MonoBehaviour
{
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void OnEnable()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (DataManager.Instance.DataProvider != null && DataManager.Instance.DataProvider.GetProgress() > -1)
        {
            text.text = "Loading. Please wait " + DataManager.Instance.DataProvider.GetProgress() + "%";
        }
    }
}
