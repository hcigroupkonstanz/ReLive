using UnityEngine;

namespace Relive.UI
{
    public class GradientGenerator : MonoBehaviour
    {
        // Start is called before the first frame update
        public static Texture2D GetTexture(int width, Color firstColor, Color secondColor)
        {
            Texture2D texture = new Texture2D(width, 1);
            Color[] colours = new Color[width];
            Gradient gradient = new Gradient();
            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            GradientColorKey[] colorKey = new GradientColorKey[2];
            colorKey[0].color = firstColor;
            colorKey[0].time = 0.0f;
            colorKey[1].color = secondColor;
            colorKey[1].time = 1.0f;

            GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)

            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 0.0f;
            alphaKey[1].time = 1.0f;

            gradient.SetKeys(colorKey, alphaKey);
            for (int i = 0; i < width; i++)
            {
                colours[i] = gradient.Evaluate((float) i / (width - i));
            }

            texture.SetPixels(colours);
            texture.Apply();
            return texture;
        }
    }
}