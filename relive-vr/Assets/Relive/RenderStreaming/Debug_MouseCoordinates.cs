using TMPro;
using Unity.RenderStreaming;
using UnityEngine;

namespace Relive.RenderStreaming
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Debug_MouseCoordinates : MonoBehaviour
    {
        public SimpleCameraController cc;
        public Camera RSCamera;
        private TextMeshProUGUI Text;

        private Debug_MouseOver prevMouseOver;

        private void OnEnable()
        {
            Text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (prevMouseOver != null)
            {
                prevMouseOver.IsMouseOver = false;
                prevMouseOver = null;
            }

            if (cc.m_mouse != null)
            {
                var pos = cc.m_mouse.position;
                Vector2 mousePos = new Vector2(pos.x.ReadValue(), pos.y.ReadValue());
                Text.text = $"X: {mousePos.x}     Y: {mousePos.y}";

                var ray = RSCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out var hit))
                {
                    var mouseOver = hit.transform.GetComponent<Debug_MouseOver>();
                    if (mouseOver)
                    {
                        mouseOver.IsMouseOver = true;
                        prevMouseOver = mouseOver;
                    }
                }
            }
            else
            {
                Text.text = "No mouse";
            }
        }
    }
}
