using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LegacyRPRenderTextureBlitter : MonoBehaviour
{
    [SerializeField] Camera m_rtCamera = null;

    Camera m_cam;

    private void OnEnable()
    {
        m_cam = GetComponent<Camera>();

        //Render nothing
        m_cam.clearFlags = CameraClearFlags.Nothing;
        m_cam.cullingMask = 0;

        Camera.onPreRender += BiltTexture;
    }

    private void OnDisable()
    {
        Camera.onPreRender -= BiltTexture;
    }

    private void BiltTexture(Camera cam)
    {
        if (m_cam == cam && null != m_rtCamera.targetTexture)
        {
            Graphics.Blit(m_rtCamera.targetTexture, (RenderTexture)null);
        }
    }
}
