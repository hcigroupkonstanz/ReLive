using UnityEngine;
using Relive.Data;
using System.Linq;
using Relive.Tools.Parameter;
using Relive.UI.Input;
using Relive.Visualizations;
using System.Collections.Generic;

namespace Relive.Visualizations
{
    public static class ColorGenerator
    {
        private static int currentColorIndex = 0;
        private static int currentSessionColorIndex = 0;

        private static List<string> colors = new List<string>
        {
            "#2f4f4f",
            "#556b2f",
            "#6b8e23",
            "#a0522d",
            "#2e8b57",
            "#800000",
            "#006400",
            "#708090",
            "#483d8b",
            "#008080",
            "#b8860b",
            "#bdb76b",
            "#4682b4",
            "#d2691e",
            "#9acd32",
            "#cd5c5c",
            "#00008b",
            "#32cd32",
            "#8fbc8f",
            "#8b008b",
            "#b03060",
            "#48d1cc",
            "#9932cc",
            "#ff4500",
            "#ff8c00",
            "#ffd700",
            "#6a5acd",
            "#0000cd",
            "#00ff00",
            "#00ff7f",
            "#dc143c",
            "#00bfff",
            "#0000ff",
            "#a020f0",
            "#adff2f",
            "#ff6347",
            "#b0c4de",
            "#ff00ff",
            "#ffff54",
            "#6495ed",
            "#dda0dd",
            "#90ee90",
            "#ff1493",
            "#ffa07a",
            "#afeeee",
            "#ee82ee",
            "#7fffd4",
            "#ffdab9",
            "#ff69b4",
            "#ffb6c1"
        };

        private static List<string> sessionColors = new List<string>
        {
            "#FFFF00", //yellow
            "#00FF00", //green
            "#FF0099", //pink
            "#00FFFF", //blue
            "#9900FF", //purple
            "#09FBD3", //green2
            "#FC6E22"  //orange
        };

        private static List<string> usedColors = new List<string>();

        private static List<string> usedSessionColors = new List<string>();

        public static Color GenerateColor()
        {
            if (colors.Count > 0)
            {
                currentColorIndex = Random.Range(0, colors.Count);
                Color color;
                ColorUtility.TryParseHtmlString(colors[currentColorIndex], out color);
                usedColors.Add(colors[currentColorIndex].ToUpper());
                colors.Remove(colors[currentColorIndex]);
                return color;
            }
            else
            {
                Debug.LogError("No more colors available. Use white color instead.");
                return Color.white;
            }

        }

        public static void ReleaseColor(Color color)
        {
            string colorString = "#" + ColorUtility.ToHtmlStringRGB(color);
            if (usedColors.Contains(colorString))
            {
                colors.Add(colorString);
                usedColors.Remove(colorString);
            }

        }

        public static Color GenerateSessionColor()
        {
            if (sessionColors.Count > 0)
            {
                currentColorIndex = Random.Range(0, sessionColors.Count);
                Color color;
                ColorUtility.TryParseHtmlString(sessionColors[currentColorIndex], out color);
                usedSessionColors.Add(sessionColors[currentColorIndex].ToUpper());
                sessionColors.Remove(sessionColors[currentColorIndex]);
                return color;
            }
            else
            {
                Debug.LogError("No more session colors available. Use white color instead.");
                return Color.white;
            }
        }

        public static string GenerateSessionColorHex()
        {
            if (sessionColors.Count > 0)
            {
                currentColorIndex = Random.Range(0, sessionColors.Count);
                string returnColor = sessionColors[currentColorIndex];
                usedSessionColors.Add(sessionColors[currentColorIndex].ToUpper());
                sessionColors.Remove(sessionColors[currentColorIndex]);
                return returnColor;
            }
            else
            {
                Debug.LogError("No more session colors available. Use white color instead.");
                return "#FFFFFF";
            }
        }

        public static void ReleaseSessionColor(Color color)
        {
            string colorString = "#" + ColorUtility.ToHtmlStringRGB(color);
            if (usedSessionColors.Contains(colorString))
            {
                sessionColors.Add(colorString);
                usedSessionColors.Remove(colorString);
            }
        }

    }

}