using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomRenderTexture : MonoBehaviour
{
    public Camera cam;
    public RawImage image;

    public RenderTexture original;
    private RenderTexture renderTexture;

    void Awake()
    {
        renderTexture = new RenderTexture(original);

        cam.targetTexture = renderTexture;
        image.texture = renderTexture;
    }
}
