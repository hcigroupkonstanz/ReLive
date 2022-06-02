using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

public class CustomFade : MonoBehaviour
{

    private Color currentColor = new Color(0, 0, 0, 0); // default starting color: black and fully transparent
    private Color targetColor = new Color(0, 0, 0, 0);  // default target color: black and fully transparent
    private Color deltaColor = new Color(0, 0, 0, 0);   // the delta-color is basically the "speed / second" at which the current color should change

    static Material fadeMaterial = null;
    static int fadeMaterialColorID = -1;

    public bool fading = false;
    public readonly BoolReactiveProperty IsFading = new BoolReactiveProperty(false);

    void OnEnable()
    {
        if (fadeMaterial == null)
        {
            fadeMaterial = new Material(Shader.Find("Custom/SteamVR_Fade"));
            fadeMaterialColorID = Shader.PropertyToID("fadeColor");
        }
    }

    public Task Fade(Color color, float duration)
    {
        fading = true;
        IsFading.Value = true;
        targetColor = color;
        deltaColor = (targetColor - currentColor) / duration;
        return IsFading.First(v => !v).ToTask();
    }

    void OnPostRender()
    {
        // print(fading);
        if (currentColor != targetColor)
        {
            // if the difference between the current alpha and the desired alpha is smaller than delta-alpha * deltaTime, then we're pretty much done fading:
            if (Mathf.Abs(currentColor.a - targetColor.a) < Mathf.Abs(deltaColor.a) * Time.deltaTime)
            {
                currentColor = targetColor;
                deltaColor = new Color(0, 0, 0, 0);
                IsFading.Value = false;
                fading = false;
            }
            else
            {
                currentColor += deltaColor * Time.deltaTime;
            }
        }

        if (currentColor.a > 0 && fadeMaterial)
        {
            fadeMaterial.SetColor(fadeMaterialColorID, currentColor);
            fadeMaterial.SetPass(0);
            GL.Begin(GL.QUADS);

            GL.Vertex3(-1, -1, 0);
            GL.Vertex3(1, -1, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(-1, 1, 0);
            GL.End();
        }
    }
}
