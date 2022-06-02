using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToolValueBadge : MonoBehaviour
{
    public TextMeshProUGUI ValueText;
    public Image BackgroundImage;
    public Camera VRCamera;

    public string Value;
    public Color32 Color;
    public Vector3 Position;

    private float canvasHeight;

    void Start()
    {
        VRCamera = Camera.main;
        canvasHeight = this.GetComponent<RectTransform>().rect.height * this.GetComponent<RectTransform>().localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        ValueText.text = Value;
        if (BackgroundImage.color != Color)
        {
            BackgroundImage.color = Color;
        }

        this.transform.position = Position + new Vector3(0f, canvasHeight / 2, 0f);
        // this.transform.LookAt(VRCamera.transform.position, Vector3.up);
        this.transform.rotation = Quaternion.LookRotation(transform.position - VRCamera.transform.position);

        if ((Color.r * 0.299 + Color.g * 0.587 + Color.b * 0.114) > 186)
        {
            ValueText.color = new Color(0, 0, 0, 255);
        }
        else
        {
            ValueText.color = new Color(255, 255, 255, 255);
        }

    }
}
